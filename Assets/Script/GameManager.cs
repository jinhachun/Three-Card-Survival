using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [BoxGroup("시스템"), Required] [SerializeField] private DeckManager deckManager;
    [BoxGroup("시스템"), Required] [SerializeField] private CardSelector cardSelector;
    [BoxGroup("시스템"), Required] [SerializeField] private EffectResolver effectResolver;
    [BoxGroup("시스템"), Required] [SerializeField] private DayEndRoutineController dayEndRoutine;
    [BoxGroup("시스템"), Required] [SerializeField] private EscapeSystem escapeSystem;

    [BoxGroup("카드")]
    [Required] [SerializeField] private CardRegistry cardRegistry;

    [BoxGroup("UI"), Required] [SerializeField] private HUDController hud;
    [BoxGroup("UI"), Required] [SerializeField] private DeckPileView deckPileView;
    [BoxGroup("UI"), Required] [SerializeField] private GameObject dayEndChoicePanel;
    [BoxGroup("UI"), Required] [SerializeField] private GameObject gameOverPanel;
    [BoxGroup("UI"), Required] [SerializeField] private GameObject gameClearPanel;
    // 씬에 배치 후 연결 (미연결 시 무시)
    [BoxGroup("UI")] [SerializeField] private TMP_Text              escapeChancePreviewText;
    [BoxGroup("UI")] [SerializeField] private TMP_Text              dayAnnouncementText;
    [BoxGroup("UI")] [SerializeField] private BuildingCompletePopup buildingCompletePopup;
    [BoxGroup("UI")] [SerializeField] private PassiveNotifier       passiveNotifier;
    [BoxGroup("UI")] [SerializeField] private RectTransform        buildingNotifyAnchor;

    private GameState _state;
    private GamePhase _phase;
    private bool      _pendingDayAnnouncement;

    void Start() => InitGame();

    void OnEnable()
    {
        cardSelector.OnCardSelected         += OnCardSelected;
        cardSelector.OnAllCardsUnselectable += OnAllCardsUnselectable;
        if (buildingCompletePopup != null) effectResolver.OnBuildingCompleted += buildingCompletePopup.Show;
        if (passiveNotifier != null) effectResolver.OnPassiveApplied += bd =>
        {
            Vector2 wp = buildingNotifyAnchor != null
                ? (Vector2)buildingNotifyAnchor.TransformPoint(buildingNotifyAnchor.rect.center)
                : default;
            passiveNotifier.Notify(bd, wp);
        };
    }

    void OnDisable()
    {
        cardSelector.OnCardSelected         -= OnCardSelected;
        cardSelector.OnAllCardsUnselectable -= OnAllCardsUnselectable;
        if (buildingCompletePopup != null) effectResolver.OnBuildingCompleted -= buildingCompletePopup.Show;
    }

    void InitGame()
    {
        _state = new GameState();
        deckManager.Init(_state);
        effectResolver.Init(deckManager);

        foreach (var entry in cardRegistry.starterDeck)
            for (int j = 0; j < entry.count; j++)
                _state.deck.Add(CardUtils.Clone(entry.card));

        deckManager.Shuffle();
        hud.Init(_state);
        EnterPlayerTurn();
    }

    void EnterPlayerTurn()
    {
        _phase = GamePhase.PlayerTurn;
        hud.Refresh();

        if (_pendingDayAnnouncement)
        {
            _pendingDayAnnouncement = false;
            PlayDayAnnouncement();
        }

        var cards = deckManager.GetThreeCards();

        // 새로 뽑힌 escalatesOnDraw 카드 비용 영구 증가
        var escalated = new System.Collections.Generic.HashSet<string>();
        for (int i = deckManager.LastCarriedOverCount; i < cards.Count; i++)
        {
            var c = cards[i];
            if (c.escalatesOnDraw && escalated.Add(c.cardName))
                _state.costEscalations[c.cardName] = _state.GetCostEscalation(c.cardName) + 1;
        }

        deckPileView?.UpdateCount(_state.deck.Count);
        cardSelector.ShowCards(cards, _state, deckManager.LastCarriedOverCount);
    }

    void OnCardSelected(CardData card, int count, Vector2 cardWorldCenter)
    {
        if (_phase != GamePhase.PlayerTurn) return;

        deckManager.SelectCard(card, count);

        // 이월된 시련 카드 거부 패널티 (DayEnd 선택 시에는 발동 안 함)
        if (card.cardType != CardType.DayEnd)
            foreach (var c in _state.carriedOver)
                if (c.cardType == CardType.Trial)
                    effectResolver.OnRefuseTrial(c, _state);

        if (card.cardType == CardType.DayEnd)
        {
            _phase = GamePhase.DayEndChoice;
            if (escapeChancePreviewText != null)
                escapeChancePreviewText.text = $"Escape chance: {escapeSystem.PreviewEscapeChance(_state):P0}";
            OpenPanel(dayEndChoicePanel);
            return;
        }

        int snapHp = _state.hp, snapFood = _state.food, snapWater = _state.water,
            snapStone = _state.stone, snapWood = _state.wood;

        effectResolver.ApplyCard(card, count, _state);

        if (passiveNotifier != null)
        {
            ShowDelta(ResourceType.HP,    _state.hp    - snapHp,    cardWorldCenter);
            ShowDelta(ResourceType.Food,  _state.food  - snapFood,  cardWorldCenter);
            ShowDelta(ResourceType.Water, _state.water - snapWater, cardWorldCenter);
            ShowDelta(ResourceType.Stone, _state.stone - snapStone, cardWorldCenter);
            ShowDelta(ResourceType.Wood,  _state.wood  - snapWood,  cardWorldCenter);
        }

        if (_state.hp <= 0)
            EnterGameOver();
        else
            EnterPlayerTurn();
    }

    void OnAllCardsUnselectable() => EnterGameOver();

    // DayEnd 삼중 선택지 — 인스펙터에서 버튼 OnClick에 연결
    public void OnChoiceEndDay()
    {
        ClosePanel(dayEndChoicePanel);

        var dayEndCard = _state.usedCards[^1];
        _state.usedCards.RemoveAt(_state.usedCards.Count - 1);
        deckManager.AddCardToDeck(dayEndCard);

        _phase = GamePhase.DayEndRoutine;
        _pendingDayAnnouncement = true;
        dayEndRoutine.Run(_state, deckManager, EnterPlayerTurn);
    }

    public void OnChoiceContinue()
    {
        ClosePanel(dayEndChoicePanel);
        // EndOfDay 선택을 취소 — carriedOver에 되돌려 같은 3장을 다시 표시
        var dayEndCard = _state.usedCards[^1];
        _state.usedCards.RemoveAt(_state.usedCards.Count - 1);
        _state.carriedOver.Add(dayEndCard);
        EnterPlayerTurn();
    }

    public void OnChoiceTryEscape()
    {
        ClosePanel(dayEndChoicePanel);
        if (escapeSystem.TryEscape(_state))
        {
            EnterGameClear();
        }
        else
        {
            effectResolver.ApplyEscapeFailurePenalty(_state);
            if (passiveNotifier != null) passiveNotifier.NotifyPenalty(_state.costPenalty);
            var dayEndCard = _state.usedCards[^1];
            _state.usedCards.RemoveAt(_state.usedCards.Count - 1);
            deckManager.AddCardToDeck(dayEndCard);
            _phase = GamePhase.DayEndRoutine;
            _pendingDayAnnouncement = true;
            dayEndRoutine.Run(_state, deckManager, EnterPlayerTurn);
        }
    }

    void EnterGameOver()
    {
        _phase = GamePhase.GameOver;
        hud.Refresh();
        OpenPanel(gameOverPanel);
    }

    void EnterGameClear()
    {
        _phase = GamePhase.GameClear;
        hud.Refresh();
        OpenPanel(gameClearPanel);
    }

    // GameOver / GameClear 패널의 재시작 버튼 OnClick에 연결
    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    // ── 패널 애니메이션 헬퍼 ─────────────────────────────────────────
    // PanelAnimator 컴포넌트가 있으면 애니메이션, 없으면 즉시 SetActive

    private static void OpenPanel(GameObject panel)
    {
        var anim = panel.GetComponent<PanelAnimator>();
        if (anim != null) anim.Open();
        else              panel.SetActive(true);
    }

    private static void ClosePanel(GameObject panel)
    {
        var anim = panel.GetComponent<PanelAnimator>();
        if (anim != null) anim.Close();
        else              panel.SetActive(false);
    }

    // ── Day 알림 ────────────────────────────────────────────────────

    private void ShowDelta(ResourceType resource, int delta, Vector2 worldPos)
    {
        if (delta != 0) passiveNotifier.NotifyDelta(resource, delta, worldPos);
    }

    private void PlayDayAnnouncement()
    {
        if (dayAnnouncementText == null) return;

        dayAnnouncementText.text = $"Day {_state.day}";
        var go = dayAnnouncementText.gameObject;
        go.SetActive(true);

        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();

        DOTween.Kill(cg);
        cg.alpha = 0f;
        DOTween.Sequence()
            .Append(cg.DOFade(1f, 0.35f).SetEase(Ease.OutCubic))
            .AppendInterval(0.75f)
            .Append(cg.DOFade(0f, 0.45f).SetEase(Ease.InCubic))
            .OnComplete(() => go.SetActive(false));
    }
}

public enum GamePhase { PlayerTurn, DayEndChoice, DayEndRoutine, GameOver, GameClear }

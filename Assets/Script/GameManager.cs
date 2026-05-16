using Sirenix.OdinInspector;
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

    private GameState _state;
    private GamePhase _phase;

    void Start() => InitGame();

    void OnEnable()
    {
        cardSelector.OnCardSelected        += OnCardSelected;
        cardSelector.OnAllCardsUnselectable += OnAllCardsUnselectable;
    }

    void OnDisable()
    {
        cardSelector.OnCardSelected        -= OnCardSelected;
        cardSelector.OnAllCardsUnselectable -= OnAllCardsUnselectable;
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
        var cards = deckManager.GetThreeCards();
        deckPileView?.UpdateCount(_state.deck.Count);
        cardSelector.ShowCards(cards, _state, deckManager.LastCarriedOverCount);
    }

    void OnCardSelected(CardData card, int count)
    {
        if (_phase != GamePhase.PlayerTurn) return;

        deckManager.SelectCard(card, count);

        // 이월된 시련 카드 거부 패널티
        foreach (var c in _state.carriedOver)
            if (c.cardType == CardType.Trial)
                effectResolver.OnRefuseTrial(c, _state);

        if (card.cardType == CardType.DayEnd)
        {
            _phase = GamePhase.DayEndChoice;
            dayEndChoicePanel.SetActive(true);
            return;
        }

        effectResolver.ApplyCard(card, count, _state);

        if (_state.hp <= 0)
            EnterGameOver();
        else
            EnterPlayerTurn();
    }

    void OnAllCardsUnselectable() => EnterGameOver();

    // DayEnd 삼중 선택지 — 인스펙터에서 버튼 OnClick에 연결
    public void OnChoiceEndDay()
    {
        dayEndChoicePanel.SetActive(false);

        // End of Day 카드를 덱에 반환 (다음 날에도 등장해야 함)
        var dayEndCard = _state.usedCards[^1];
        _state.usedCards.RemoveAt(_state.usedCards.Count - 1);
        deckManager.AddCardToDeck(dayEndCard);

        _phase = GamePhase.DayEndRoutine;
        dayEndRoutine.Run(_state, deckManager, EnterPlayerTurn);
    }

    public void OnChoiceContinue()
    {
        dayEndChoicePanel.SetActive(false);
        // usedCards 마지막 카드가 방금 선택한 하루의 끝 카드
        var dayEndCard = _state.usedCards[^1];
        _state.usedCards.RemoveAt(_state.usedCards.Count - 1);
        deckManager.AddCardToDeck(dayEndCard);
        deckManager.Shuffle();
        EnterPlayerTurn();
    }

    public void OnChoiceTryEscape()
    {
        dayEndChoicePanel.SetActive(false);
        if (escapeSystem.TryEscape(_state))
            EnterGameClear();
        else
        {
            effectResolver.ApplyEscapeFailurePenalty(_state);
            EnterPlayerTurn();
        }
    }

    void EnterGameOver()
    {
        _phase = GamePhase.GameOver;
        hud.Refresh();
        gameOverPanel.SetActive(true);
    }

    void EnterGameClear()
    {
        _phase = GamePhase.GameClear;
        hud.Refresh();
        gameClearPanel.SetActive(true);
    }

    // GameOver / GameClear 패널의 재시작 버튼 OnClick에 연결
    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}

public enum GamePhase { PlayerTurn, DayEndChoice, DayEndRoutine, GameOver, GameClear }

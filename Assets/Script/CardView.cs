using System;
using System.Text;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Required] [SerializeField] private TextMeshProUGUI nameText;
    [Required] [SerializeField] private Image           typeBackground;
    [Required] [SerializeField] private TextMeshProUGUI typeText;
    [Required] [SerializeField] private Image           cardImage;
    [Required] [SerializeField] private TextMeshProUGUI descText;
    [Required] [SerializeField] private Image           stackShadow;
    [Required] [SerializeField] private TextMeshProUGUI countBadge;
    [Required] [SerializeField] private Button          button;
    [Required] [SerializeField] private CanvasGroup     canvasGroup;

    private static readonly Color ColTrial    = new(0.78f, 0.18f, 0.18f);
    private static readonly Color ColEndOfDay = new(0.18f, 0.35f, 0.75f);
    private static readonly Color ColBuilding = new(0.70f, 0.48f, 0.10f);
    private static readonly Color ColNormal   = new(0.22f, 0.55f, 0.22f);

    private RectTransform _rect;
    private Vector2 _restPos;
    private bool _canSelect;
    private int _count;
    private bool _isNew;

    void Awake() => _rect = GetComponent<RectTransform>();

    public void Setup(CardData card, int count, bool canSelect, bool isNew, Action<Vector2> onClick, GameState state = null)
    {
        _count = count;
        _isNew = isNew;

        nameText.text = card.cardName;

        (typeText.text, typeBackground.color) = card.cardType switch
        {
            CardType.Trial    => ("TRIAL",      ColTrial),
            CardType.DayEnd   => ("END OF DAY", ColEndOfDay),
            CardType.Building => ("BUILD",      ColBuilding),
            _                 => ("",           ColNormal),
        };

        cardImage.sprite  = card.cardSprite;
        cardImage.enabled = true;
        cardImage.color   = card.cardSprite != null ? Color.white : new Color(0.72f, 0.72f, 0.75f);

        descText.text = BuildDescText(card, count, state);

        stackShadow.gameObject.SetActive(count > 1);
        countBadge.gameObject.SetActive(count > 1);
        if (count > 1) countBadge.text = $"×{count}";

        _canSelect               = canSelect;
        canvasGroup.alpha        = canSelect ? 1f : 0.4f;
        canvasGroup.interactable = canSelect;

        button.onClick.RemoveAllListeners();
        if (canSelect)
            button.onClick.AddListener(() =>
            {
                // 애니메이션 전 클릭 시점의 카드 중심 월드 좌표 캡처
                Vector2 worldCenter = _rect.TransformPoint(_rect.rect.center);
                PlaySelectAnimation(() => onClick?.Invoke(worldCenter));
            });
    }

    public void PrepareForAnimation()
    {
        DOTween.Kill(_rect);
        DOTween.Kill(transform);
        DOTween.Kill(canvasGroup);
        DOTween.Kill(countBadge.transform);
        transform.localScale            = Vector3.one;
        // 이월 카드는 배지가 이미 보여야 하므로 즉시 표시, 새 카드는 팝인 대기
        countBadge.transform.localScale = (_count > 1 && _isNew) ? Vector3.zero : Vector3.one;
    }

    public void PlayDealAnimation(float delay, Vector2 deckLocalPos)
    {
        _restPos = _rect.anchoredPosition;

        if (!_isNew)
        {
            // 이월 카드: 제자리에서 페이드인만
            canvasGroup.alpha = 0f;
            DOTween.Sequence()
                .Append(canvasGroup.DOFade(_canSelect ? 1f : 0.4f, 0.25f));
            return;
        }

        // 새 카드: 덱 파일 위치에서 날아옴
        _rect.anchoredPosition = deckLocalPos;
        transform.localScale   = new Vector3(0.3f, 0.3f, 1f);
        canvasGroup.alpha      = 0f;

        var seq = DOTween.Sequence()
            .AppendInterval(delay)
            .Append(_rect.DOAnchorPos(_restPos, 0.4f).SetEase(Ease.OutCubic))
            .Join(transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
            .Join(canvasGroup.DOFade(_canSelect ? 1f : 0.4f, 0.2f));

        if (_count > 1)
            seq.Append(countBadge.transform.DOScale(1.25f, 0.15f).SetEase(Ease.OutBack))
               .Append(countBadge.transform.DOScale(1f, 0.08f));
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (!_canSelect) return;
        _rect.DOAnchorPosY(_restPos.y + 24f, 0.15f).SetEase(Ease.OutQuad);
        transform.DOScale(1.08f, 0.15f);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (!_canSelect) return;
        _rect.DOAnchorPosY(_restPos.y, 0.15f).SetEase(Ease.OutQuad);
        transform.DOScale(1f, 0.15f);
    }

    private void PlaySelectAnimation(Action onComplete)
    {
        canvasGroup.interactable = false;

        DOTween.Sequence()
            .Append(transform.DOScale(1.15f, 0.08f).SetEase(Ease.OutQuad))
            .Append(_rect.DOAnchorPosY(_restPos.y + 220f, 0.28f).SetEase(Ease.InCubic))
            .Join(canvasGroup.DOFade(0f, 0.22f))
            .OnComplete(() =>
            {
                _rect.anchoredPosition = _restPos;
                onComplete?.Invoke();
            });
    }

    private string BuildDescText(CardData card, int count, GameState state = null)
    {
        var sb = new StringBuilder();

        bool freeTurn  = state?.freeTurnActive ?? false;
        int escalation = state?.GetCostEscalation(card.cardName) ?? 0;
        int penalty    = state?.costPenalty ?? 0;
        int discount   = freeTurn ? 1 : 0;
        foreach (var cost in card.costs)
            sb.AppendLine(R($"{ResourceIcons.Tag(cost.resource)} -{Mathf.Max(0, cost.amount + escalation + penalty - discount)}"));

        foreach (var cond in card.conditions)
            sb.AppendLine(Bl($"Req: {cond.stat} {cond.minAmount}+"));

        var shownLsKeys = new System.Collections.Generic.HashSet<(int, int, int)>();
        foreach (var effect in card.effects)
        {
            if (effect is ResourceEffectSO re)
            {
                string icon = ResourceIcons.Tag(re.resource);
                if (re.isRandom)
                    sb.AppendLine(B(count > 1
                        ? $"{icon} +{re.randomMin * count}~{re.randomMax * count} (×{count})"
                        : $"{icon} +{re.randomMin}~{re.randomMax}"));
                else
                {
                    int total = re.amount * count;
                    string line = count > 1
                        ? $"{icon} {(total >= 0 ? "+" : "")}{total} (×{count})"
                        : $"{icon} {(re.amount >= 0 ? "+" : "")}{re.amount}";
                    sb.AppendLine(re.amount >= 0 ? B(line) : R(line));
                }
            }
            else if (effect is StatEffectSO se)
            {
                int total = se.amount * count;
                string line = count > 1
                    ? $"{se.stat} {(total >= 0 ? "+" : "")}{total} (×{count})"
                    : $"{se.stat} {(se.amount >= 0 ? "+" : "")}{se.amount}";
                sb.AppendLine(se.amount >= 0 ? B(line) : R(line));
            }
            else if (effect is EscapeChanceEffectSO ec)
                sb.AppendLine(B(count > 1
                    ? $"Escape +{ec.amount * count * 100f:0}% (×{count})"
                    : $"Escape +{ec.amount * 100f:0}%"));
            else if (effect is BuildingProgressEffectSO bpe && bpe.buildingData != null)
            {
                var bd = bpe.buildingData;
                sb.AppendLine(B($"{bd.buildingName} +{bd.progressPerUse}%"));
                if (state != null)
                    sb.AppendLine(B(state.IsBuildingComplete(bd.buildingName)
                        ? "Complete!"
                        : $"(Currently {state.GetBuildingProgress(bd.buildingName)}%)"));
                if (bd.passiveResource != ResourceType.None)
                    sb.AppendLine(B($"  > {ResourceIcons.Tag(bd.passiveResource)} +{bd.passiveAmount}/turn"));
                if (bd.completionStatGain != StatType.None)
                    sb.AppendLine(B($"  > {bd.completionStatGain} +{bd.completionStatAmount} on complete"));
            }
            else if (effect is RaidEffectSO raid)
            {
                sb.AppendLine(R($"{ResourceIcons.Tag(ResourceType.HP)} -{raid.minDrain}~{raid.maxDrain}"));
                sb.AppendLine(B($"{ResourceIcons.Tag(ResourceType.Stone)} {ResourceIcons.Tag(ResourceType.Wood)} +(equal to drained)"));
            }
            else if (effect is LastStandEffectSO ls)
            {
                // 같은 threshold+amount를 공유하는 효과들을 한 줄로 묶기
                if (shownLsKeys.Contains((ls.hpThreshold, ls.bigAmount, ls.smallAmount))) continue;
                shownLsKeys.Add((ls.hpThreshold, ls.bigAmount, ls.smallAmount));

                var icons = new StringBuilder();
                foreach (var e2 in card.effects)
                    if (e2 is LastStandEffectSO ls2
                        && ls2.hpThreshold == ls.hpThreshold
                        && ls2.bigAmount   == ls.bigAmount
                        && ls2.smallAmount == ls.smallAmount)
                        icons.Append(ResourceIcons.Tag(ls2.resource));

                string hpIcon = ResourceIcons.Tag(ResourceType.HP);
                sb.AppendLine(R($"{hpIcon} ≤ {ls.hpThreshold}  →  {icons} +{ls.bigAmount}"));
                sb.AppendLine(B($"{hpIcon} > {ls.hpThreshold}  →  {icons} +{ls.smallAmount}"));
            }
            else if (effect is RemoveTrialEffectSO rt)
                sb.AppendLine(B(rt.source == RemoveTrialEffectSO.TrialSource.Deck
                    ? "Remove random Trial from deck"
                    : "Dismiss a Trial in hand"));
            else if (effect is TriggerCardEffectSO tc)
                sb.AppendLine(B(tc.source == TriggerCardEffectSO.CardSource.Deck
                    ? "Trigger random card from deck"
                    : "Recall random used card"));
            else if (effect is FreeTurnEffectSO)
                sb.AppendLine(B("Next day: all costs -1"));
            else if (effect is BlueprintEffectSO bp)
                sb.AppendLine(B($"Random building: +{bp.progressMin}~{bp.progressMax}%"));
            else if (effect is AllBuildingProgressEffectSO abp)
                sb.AppendLine(B($"All buildings: +{abp.progressAmount}%"));
        }

        if (card.escalatesOnDraw)
        {
            int esc = state?.GetCostEscalation(card.cardName) ?? 0;
            sb.AppendLine(R(esc > 0 ? $"Cost escalates each draw (+{esc})" : "Cost escalates each draw"));
        }
        if (card.cardType == CardType.Trial)
            sb.AppendLine(R("Ignored: HP -1 per card played"));

        return sb.ToString().TrimEnd();
    }

    private static string R(string s)  => $"<color=#CC2222>{s}</color>";
    private static string B(string s)  => $"<color=#111111>{s}</color>";
    private static string Bl(string s) => $"<color=#2255BB>{s}</color>";
}

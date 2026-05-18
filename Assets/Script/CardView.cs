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
    [Required] [SerializeField] private TextMeshProUGUI costText;
    [Required] [SerializeField] private TextMeshProUGUI conditionText;
    [Required] [SerializeField] private TextMeshProUGUI effectText;
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

    public void Setup(CardData card, int count, bool canSelect, bool isNew, Action onClick, GameState state = null)
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

        costText.text      = BuildCostText(card, count);
        conditionText.text = BuildConditionText(card);
        effectText.text    = BuildEffectText(card, count, state);

        costText.gameObject.SetActive(card.costs.Count > 0);
        conditionText.gameObject.SetActive(card.conditions.Count > 0);

        stackShadow.gameObject.SetActive(count > 1);
        countBadge.gameObject.SetActive(count > 1);
        if (count > 1) countBadge.text = $"×{count}";

        _canSelect               = canSelect;
        canvasGroup.alpha        = canSelect ? 1f : 0.4f;
        canvasGroup.interactable = canSelect;

        button.onClick.RemoveAllListeners();
        if (canSelect)
            button.onClick.AddListener(() => PlaySelectAnimation(onClick));
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

    private string BuildCostText(CardData card, int count)
    {
        var sb = new StringBuilder();
        foreach (var cost in card.costs)
            sb.AppendLine(count > 1
                ? $"{cost.resource} -{cost.amount * count} (×{count})"
                : $"{cost.resource} -{cost.amount}");
        return sb.ToString().TrimEnd();
    }

    private string BuildConditionText(CardData card)
    {
        var sb = new StringBuilder();
        foreach (var cond in card.conditions)
            sb.AppendLine($"Req: {cond.stat} {cond.minAmount}+");
        return sb.ToString().TrimEnd();
    }

    private string BuildEffectText(CardData card, int count, GameState state = null)
    {
        var sb = new StringBuilder();
        foreach (var effect in card.effects)
        {
            if (effect is ResourceEffectSO re)
                sb.AppendLine(count > 1
                    ? $"{re.resource} {(re.amount * count >= 0 ? "+" : "")}{re.amount * count} (×{count})"
                    : $"{re.resource} {(re.amount >= 0 ? "+" : "")}{re.amount}");
            else if (effect is StatEffectSO se)
                sb.AppendLine(count > 1
                    ? $"{se.stat} {(se.amount * count >= 0 ? "+" : "")}{se.amount * count} (×{count})"
                    : $"{se.stat} {(se.amount >= 0 ? "+" : "")}{se.amount}");
            else if (effect is EscapeChanceEffectSO ec)
                sb.AppendLine(count > 1
                    ? $"Escape +{ec.amount * count * 100f:0}% (×{count})"
                    : $"Escape +{ec.amount * 100f:0}%");
            else if (effect is BuildingProgressEffectSO bpe && bpe.buildingData != null)
            {
                var bd = bpe.buildingData;
                sb.AppendLine($"{bd.buildingName} +{bd.progressPerUse}%");
                if (state != null)
                    sb.AppendLine(state.IsBuildingComplete(bd.buildingName)
                        ? "Complete!"
                        : $"(Currently {state.GetBuildingProgress(bd.buildingName)}%)");
                if (bd.passiveResource != ResourceType.None)
                    sb.AppendLine($"  > {bd.passiveResource} +{bd.passiveAmount}/turn");
                if (bd.completionStatGain != StatType.None)
                    sb.AppendLine($"  > {bd.completionStatGain} +{bd.completionStatAmount} on complete");
            }
        }
        if (card.cardType == CardType.Trial)
            sb.AppendLine("Ignored: HP -1 per card played");
        return sb.ToString().TrimEnd();
    }
}

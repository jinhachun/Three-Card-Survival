using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class BuildingCompletePopup : MonoBehaviour
{
    [Required] [SerializeField] private CanvasGroup canvasGroup;
    [Required] [SerializeField] private TMP_Text    buildingNameText;
    [Required] [SerializeField] private TMP_Text    descriptionText;

    void Awake() => gameObject.SetActive(false);

    public void Show(BuildingData building)
    {
        buildingNameText.text = building.buildingName;
        descriptionText.text  = BuildDesc(building);

        DOTween.Kill(transform);
        DOTween.Kill(canvasGroup);

        gameObject.SetActive(true);
        canvasGroup.alpha    = 0f;
        transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad))
            .Join(transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack))
            .AppendInterval(2.2f)
            .Append(canvasGroup.DOFade(0f, 0.5f))
            .OnComplete(() => gameObject.SetActive(false));
    }

    private static string BuildDesc(BuildingData b)
    {
        if (b.passiveResource != ResourceType.None)
            return $"{b.passiveResource} +{b.passiveAmount} each turn";
        if (b.completionStatGain != StatType.None)
            return $"{b.completionStatGain} +{b.completionStatAmount}";
        return "";
    }
}

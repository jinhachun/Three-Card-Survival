using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class PassiveNotifier : MonoBehaviour
{
    // 플로팅 텍스트가 시작될 위치 — HUD 자원 표시 근처에 배치
    [Required] [SerializeField] private RectTransform spawnAnchor;

    private static readonly Color ColWater = new(0.35f, 0.80f, 1.00f);
    private static readonly Color ColFood  = new(0.90f, 0.78f, 0.20f);
    private static readonly Color ColHp    = new(1.00f, 0.35f, 0.35f);
    private static readonly Color ColMisc  = new(0.80f, 0.80f, 0.80f);

    private int _activeCount;

    public void Notify(BuildingData building)
    {
        if (building.passiveResource == ResourceType.None) return;

        string text  = $"+{building.passiveAmount} {building.passiveResource}  ({building.buildingName})";
        Color  color = building.passiveResource switch
        {
            ResourceType.Water => ColWater,
            ResourceType.Food  => ColFood,
            ResourceType.HP    => ColHp,
            _                  => ColMisc,
        };

        SpawnText(text, color);
    }

    private void SpawnText(string text, Color color)
    {
        var go = new GameObject("PassiveFloat");
        go.transform.SetParent(spawnAnchor.parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = spawnAnchor.anchoredPosition + Vector2.up * (_activeCount * 38f);
        rt.sizeDelta        = new Vector2(380f, 40f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 20;
        tmp.color     = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Left;

        var cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        _activeCount++;
        DOTween.Sequence()
            .Append(cg.DOFade(1f, 0.2f))
            .AppendInterval(0.5f)
            .Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 80f, 0.9f).SetEase(Ease.OutCubic))
            .Join(cg.DOFade(0f, 0.8f).SetDelay(0.1f))
            .OnComplete(() =>
            {
                _activeCount = Mathf.Max(0, _activeCount - 1);
                Destroy(go);
            });
    }
}

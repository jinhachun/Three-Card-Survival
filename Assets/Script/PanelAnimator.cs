using DG.Tweening;
using UnityEngine;

/// <summary>
/// 패널 오브젝트에 붙여서 Open/Close 애니메이션을 제공.
/// CanvasGroup(알파 페이드) + RectTransform(스케일 바운스) 사용.
/// GameManager의 GameObject 패널 필드에 이 컴포넌트를 추가하기만 하면 됨.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PanelAnimator : MonoBehaviour
{
    private CanvasGroup   _cg;
    private RectTransform _rect;

    void Awake()
    {
        _cg   = GetComponent<CanvasGroup>();
        _rect = GetComponent<RectTransform>();
    }

    public void Open()
    {
        DOTween.Kill(gameObject);
        gameObject.SetActive(true);
        _cg.alpha        = 0f;
        _rect.localScale = new Vector3(0.88f, 0.88f, 1f);

        DOTween.Sequence().SetTarget(gameObject)
            .Join(_cg.DOFade(1f, 0.22f))
            .Join(_rect.DOScale(1f, 0.28f).SetEase(Ease.OutBack));
    }

    public void Close(System.Action onComplete = null)
    {
        DOTween.Kill(gameObject);
        DOTween.Sequence().SetTarget(gameObject)
            .Join(_cg.DOFade(0f, 0.16f))
            .Join(_rect.DOScale(0.88f, 0.16f).SetEase(Ease.InBack))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _rect.localScale = Vector3.one;
                _cg.alpha        = 1f;
                onComplete?.Invoke();
            });
    }
}

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckPileView : MonoBehaviour
{
    [BoxGroup("기본")]
    [Required] [SerializeField] private TextMeshProUGUI countText;

    [BoxGroup("카드 추가 알림")]
    [Required] [SerializeField] private DeckManager deckManager;

    private int _prevCount = -1;
    private readonly Queue<CardData> _toastQueue = new();
    private bool _processingQueue;

    void OnEnable()  => deckManager.OnCardAddedToDeck += EnqueueToast;
    void OnDisable()
    {
        deckManager.OnCardAddedToDeck -= EnqueueToast;
        _toastQueue.Clear();
        _processingQueue = false;
    }

    public void UpdateCount(int count)
    {
        countText.text = count.ToString();

        if (_prevCount == count || _prevCount < 0)
        {
            _prevCount = count;
            return;
        }

        bool decreased = count < _prevCount;
        _prevCount = count;

        DOTween.Kill(countText.transform);
        countText.transform.DOPunchScale(Vector3.one * (decreased ? 0.22f : 0.35f), 0.32f, 5, 0.5f);
    }

    // ── 토스트 큐 ─────────────────────────────────────────────────────

    private void EnqueueToast(CardData card)
    {
        _toastQueue.Enqueue(card);
        if (!_processingQueue)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _processingQueue = true;
        while (_toastQueue.Count > 0)
        {
            yield return StartCoroutine(ShowToast(_toastQueue.Dequeue()));
            yield return new WaitForSeconds(0.08f);
        }
        _processingQueue = false;
    }

    private IEnumerator ShowToast(CardData card)
    {
        bool done = false;

        var rootCanvas = GetRootCanvas();
        Vector2 pilePos  = GetCanvasLocalPos(rootCanvas);
        Vector2 startPos = pilePos + new Vector2(0f, 110f);

        // ── 토스트 빌드 ───────────────────────────────────────────────
        var go = new GameObject("CardToast", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        go.transform.SetParent(rootCanvas.transform, false);
        go.transform.SetAsLastSibling();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(190f, 70f);
        rt.anchoredPosition = startPos;

        var img = go.AddComponent<Image>();
        img.color = TypeColor(card.cardType);

        var cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 카드명 (상단 60%)
        AddLabel(go, card.cardName,          15f, new Vector2(0f, 0.38f), Vector2.one,          Color.white);
        // 타입 서브텍스트 (하단 40%)
        AddLabel(go, TypeLabel(card.cardType), 11f, Vector2.zero,          new Vector2(1f, 0.40f), new Color(1f, 1f, 1f, 0.68f));

        rt.localScale = Vector3.one * 0.7f;

        // ── 애니메이션: 팝업 → 유지 → 덱파일로 날아감 ────────────────
        DOTween.Sequence()
            .Append(cg.DOFade(1f, 0.18f))
            .Join(rt.DOScale(1f, 0.18f).SetEase(Ease.OutBack))
            .AppendInterval(0.65f)
            .Append(rt.DOAnchorPos(pilePos, 0.30f).SetEase(Ease.InCubic))
            .Join(rt.DOScale(0.3f, 0.30f).SetEase(Ease.InCubic))
            .Join(cg.DOFade(0f, 0.30f))
            .OnComplete(() => { Destroy(go); done = true; });

        yield return new WaitUntil(() => done);
    }

    // ── 유틸 ──────────────────────────────────────────────────────────

    private Canvas GetRootCanvas()
    {
        var canvas = GetComponentInParent<Canvas>();
        Canvas root = canvas;
        var t = canvas.transform.parent;
        while (t != null)
        {
            var c = t.GetComponent<Canvas>();
            if (c != null) root = c;
            t = t.parent;
        }
        return root;
    }

    private Vector2 GetCanvasLocalPos(Canvas rootCanvas)
    {
        var myRect    = GetComponent<RectTransform>();
        var canvasRect = rootCanvas.GetComponent<RectTransform>();
        Camera cam    = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, myRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 local);
        return local;
    }

    private static void AddLabel(GameObject parent, string text, float size, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject("L", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text  = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.fontSize = size;
        tmp.enableWordWrapping = false;
        tmp.fontStyle = FontStyles.Bold;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static Color TypeColor(CardType t) => t switch
    {
        CardType.Trial => new Color(0.82f, 0.18f, 0.18f),
        CardType.SOS   => new Color(0.18f, 0.48f, 0.90f),
        CardType.Stat  => new Color(0.55f, 0.28f, 0.85f),
        _              => new Color(0.22f, 0.58f, 0.28f),
    };

    private static string TypeLabel(CardType t) => t switch
    {
        CardType.Trial => "+ Trial",
        CardType.SOS   => "+ SOS",
        CardType.Stat  => "+ Stat",
        _              => "+ Reward",
    };
}

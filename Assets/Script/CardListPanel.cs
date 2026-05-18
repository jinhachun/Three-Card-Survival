using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 카드 목록을 보여주고 N장 선택받는 공용 패널.
// 카드 버튼 프리팹: Image(배경) + Button + TMP_Text(자식) 구성 필요.
public class CardListPanel : MonoBehaviour
{
    [Required] [SerializeField] private Transform   content;
    [Required] [SerializeField] private GameObject  cardButtonPrefab;
    [Required] [SerializeField] private Button      confirmButton;
    [Required] [SerializeField] private TMP_Text    titleText;

    private readonly List<int>        _selected = new();
    private readonly List<GameObject> _buttons  = new();
    private int              _maxSelect;
    private Action<List<int>> _onConfirm;

    // 오픈/클로즈 애니메이션용 — 없으면 즉시 SetActive
    private CanvasGroup _cg;
    private Transform   _panelT;

    void Awake()
    {
        _cg     = GetComponent<CanvasGroup>();
        _panelT = transform.Find("Panel");
    }

    public void Show(string title, List<CardData> cards, int maxSelect, Action<List<int>> onConfirm)
    {
        titleText.text = title;
        _maxSelect     = maxSelect;
        _onConfirm     = onConfirm;
        _selected.Clear();

        foreach (var b in _buttons) Destroy(b);
        _buttons.Clear();

        for (int i = 0; i < cards.Count; i++)
        {
            int captured = i;
            var go = Instantiate(cardButtonPrefab, content);
            _buttons.Add(go);

            var cv = go.GetComponent<CardView>();
            if (cv != null)
            {
                // canSelect=false로 호버/딜 애니메이션 비활성화 후 alpha/interactable 직접 복원
                // HorizontalLayoutGroup과 DOTween 충돌 방지
                cv.Setup(cards[i], 1, false, false, null);
                var cg = go.GetComponent<CanvasGroup>();
                cg.alpha        = 1f;
                cg.interactable = true;
                go.GetComponent<Button>().onClick.AddListener(() => SelectImmediate(captured));
            }
            else
            {
                go.GetComponentInChildren<TMP_Text>().text = cards[i].cardName;
                go.GetComponent<Button>().onClick.AddListener(() => ToggleSelect(captured, go));
            }
        }

        confirmButton.gameObject.SetActive(maxSelect > 1);
        confirmButton.interactable = false;
        gameObject.SetActive(true);
        PlayOpen();
    }

    private void PlayOpen()
    {
        if (_cg == null || _panelT == null) return;

        DOTween.Kill(_cg);
        DOTween.Kill(_panelT);

        _cg.alpha            = 0f;
        _panelT.localScale   = new Vector3(0.88f, 0.88f, 1f);

        DOTween.Sequence()
            .Join(_cg.DOFade(1f, 0.22f))
            .Join(_panelT.DOScale(1f, 0.28f).SetEase(Ease.OutBack));
    }

    private void PlayClose(Action onComplete)
    {
        if (_cg == null || _panelT == null)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
            return;
        }

        DOTween.Kill(_cg);
        DOTween.Kill(_panelT);

        DOTween.Sequence()
            .Join(_cg.DOFade(0f, 0.18f))
            .Join(_panelT.DOScale(0.88f, 0.18f).SetEase(Ease.InBack))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _panelT.localScale = Vector3.one;
                onComplete?.Invoke();
            });
    }

    private void SelectImmediate(int index)
    {
        var captured = new List<GameObject>(_buttons);
        _buttons.Clear();

        PlayClose(() =>
        {
            foreach (var b in captured) Destroy(b);
            _onConfirm?.Invoke(new List<int> { index });
        });
    }

    private void ToggleSelect(int index, GameObject btn)
    {
        var img = btn.GetComponent<Image>();

        if (_selected.Contains(index))
        {
            _selected.Remove(index);
            img.color = Color.white;
        }
        else if (_selected.Count < _maxSelect)
        {
            _selected.Add(index);
            img.color = Color.yellow;
        }

        confirmButton.interactable = (_selected.Count == _maxSelect);
    }

    // 확인 버튼 OnClick에 연결
    public void OnConfirmClicked()
    {
        var captured = new List<GameObject>(_buttons);
        var selected = new List<int>(_selected);
        _buttons.Clear();
        _selected.Clear();

        PlayClose(() =>
        {
            foreach (var b in captured) Destroy(b);
            _onConfirm?.Invoke(selected);
        });
    }
}

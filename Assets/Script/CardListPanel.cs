using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 카드 목록을 보여주고 N장 선택받는 공용 패널.
// 카드 버튼 프리팹: Image(배경) + Button + TMP_Text(자식) 구성 필요.
public class CardListPanel : MonoBehaviour
{
    [Required] [SerializeField] private Transform content;
    [Required] [SerializeField] private GameObject cardButtonPrefab;
    [Required] [SerializeField] private Button confirmButton;
    [Required] [SerializeField] private TMP_Text titleText;

    private readonly List<int> _selected = new();
    private readonly List<GameObject> _buttons = new();
    private int _maxSelect;
    private Action<List<int>> _onConfirm;

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
            var btn = Instantiate(cardButtonPrefab, content);
            btn.GetComponentInChildren<TMP_Text>().text = cards[i].cardName;
            btn.GetComponent<Button>().onClick.AddListener(() => ToggleSelect(captured, btn));
            _buttons.Add(btn);
        }

        confirmButton.interactable = false;
        gameObject.SetActive(true);
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
        gameObject.SetActive(false);
        _onConfirm?.Invoke(new List<int>(_selected));
    }
}

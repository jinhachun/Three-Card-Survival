using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Required] [SerializeField] private TextMeshProUGUI nameText;
    [Required] [SerializeField] private Button button;
    [Required] [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(CardData card, bool canSelect, Action onClick)
    {
        nameText.text = card.cardName;
        canvasGroup.alpha = canSelect ? 1f : 0.4f;
        canvasGroup.interactable = canSelect;

        button.onClick.RemoveAllListeners();
        if (canSelect)
            button.onClick.AddListener(() => onClick());
    }
}

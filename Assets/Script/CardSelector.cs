using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    [Required] [SerializeField] private CardView[] cardViews; // 3개

    public event Action<int> OnCardSelected;
    public event Action OnAllCardsUnselectable;

    public void ShowCards(IReadOnlyList<CardData> cards, GameState state)
    {
        bool anySelectable = false;

        for (int i = 0; i < cardViews.Length; i++)
        {
            if (i < cards.Count)
            {
                bool canSelect = CanSelect(cards[i], state);
                if (canSelect) anySelectable = true;

                int captured = i;
                cardViews[i].gameObject.SetActive(true);
                cardViews[i].Setup(cards[i], canSelect, () => OnCardSelected?.Invoke(captured));
            }
            else
            {
                cardViews[i].gameObject.SetActive(false);
            }
        }

        if (!anySelectable)
            OnAllCardsUnselectable?.Invoke();
    }

    public bool CanSelect(CardData card, GameState state)
    {
        foreach (var cost in card.costs)
            if (state.GetResource(cost.resource) < cost.amount + state.costPenalty)
                return false;

        foreach (var cond in card.conditions)
            if (state.GetStat(cond.stat) < cond.minAmount)
                return false;

        return true;
    }
}

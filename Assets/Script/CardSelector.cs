using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CardSelector : MonoBehaviour
{
    [Required]  [SerializeField] private CardView[]    cardViews;
    // 덱 파일의 RectTransform — 미연결 시 기존 애니메이션으로 폴백
    [SerializeField] private RectTransform deckPileRect;

    public event Action<CardData, int> OnCardSelected;
    public event Action OnAllCardsUnselectable;

    private bool _animating;

    // carriedOverCount: DeckManager.LastCarriedOverCount를 GameManager가 전달
    public void ShowCards(IReadOnlyList<CardData> cards, GameState state, int carriedOverCount)
    {
        _animating = false;
        bool anySelectable = false;

        var slots = GroupByName(cards, carriedOverCount);

        for (int i = 0; i < cardViews.Length; i++)
        {
            if (i < slots.Count)
            {
                var (card, count, isNew) = slots[i];
                bool canSelect = CanSelect(card, count, state);
                if (canSelect) anySelectable = true;

                var capturedCard  = card;
                var capturedCount = count;
                cardViews[i].gameObject.SetActive(true);
                cardViews[i].Setup(card, count, canSelect, isNew, () =>
                {
                    if (_animating) return;
                    _animating = true;
                    OnCardSelected?.Invoke(capturedCard, capturedCount);
                }, state);
            }
            else
            {
                cardViews[i].gameObject.SetActive(false);
            }
        }

        foreach (var cv in cardViews)
            if (cv.gameObject.activeSelf)
                cv.PrepareForAnimation();

        Canvas.ForceUpdateCanvases();
        var cardContainer = cardViews[0].transform.parent as RectTransform;
        if (cardContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer);

        Vector2 deckLocalPos = ComputeDeckLocalPos(cardContainer);

        int newCardIdx = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            bool  isNew = slots[i].isNew;
            float delay = isNew ? newCardIdx++ * 0.12f : 0f;
            cardViews[i].PlayDealAnimation(delay, deckLocalPos);
        }

        if (!anySelectable)
            OnAllCardsUnselectable?.Invoke();
    }

    public bool CanSelect(CardData card, int count, GameState state)
    {
        foreach (var cost in card.costs)
            if (state.GetResource(cost.resource) < cost.amount * count + state.costPenalty)
                return false;

        foreach (var cond in card.conditions)
            if (state.GetStat(cond.stat) < cond.minAmount)
                return false;

        return true;
    }

    private static List<(CardData card, int count, bool isNew)> GroupByName(
        IReadOnlyList<CardData> cards, int carriedOverCount)
    {
        var result = new List<(CardData, int, bool)>();
        var seen   = new Dictionary<string, int>();

        for (int i = 0; i < cards.Count; i++)
        {
            var  card      = cards[i];
            bool cardIsNew = i >= carriedOverCount;

            if (seen.TryGetValue(card.cardName, out int idx))
            {
                var (c, n, wasNew) = result[idx];
                result[idx] = (c, n + 1, wasNew || cardIsNew);
            }
            else
            {
                seen[card.cardName] = result.Count;
                result.Add((card, 1, cardIsNew));
            }
        }
        return result;
    }

    // 덱 파일 위치를 이 컨테이너의 로컬 좌표로 변환
    // deckPileRect 미연결 시 컨테이너 왼쪽 바깥 위치로 폴백
    private Vector2 ComputeDeckLocalPos(RectTransform container)
    {
        if (container == null || deckPileRect == null)
            return container != null ? new Vector2(-container.rect.width * 0.7f, 0f) : Vector2.zero;

        var canvas = deckPileRect.GetComponentInParent<Canvas>();
        if (canvas == null)
            return new Vector2(-container.rect.width * 0.7f, 0f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            container,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, deckPileRect.position),
            canvas.worldCamera,
            out Vector2 localPos);

        return localPos;
    }

}

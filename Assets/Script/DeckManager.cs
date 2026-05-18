using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    private GameState _state;
    private List<CardData> _currentThree = new();

    public void Init(GameState state) => _state = state;

    public void Shuffle()
    {
        var deck = _state.deck;
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    // 마지막 GetThreeCards 호출 당시 이월 카드 수 (CardSelector가 참조)
    public int LastCarriedOverCount { get; private set; }

    // carriedOver + 덱 상단 보충 → 항상 unique 카드 이름 3종 확보
    // 중복 카드(x2 등)가 생기면 덱에서 추가 보충, 덱 소진 시 가능한 만큼만
    public IReadOnlyList<CardData> GetThreeCards()
    {
        _currentThree.Clear();
        LastCarriedOverCount = _state.carriedOver.Count;
        _currentThree.AddRange(_state.carriedOver);
        _state.carriedOver.Clear();

        while (UniqueNameCount(_currentThree) < 3 && _state.deck.Count > 0)
        {
            _currentThree.Add(_state.deck[0]);
            _state.deck.RemoveAt(0);
        }

        return _currentThree;
    }

    private static int UniqueNameCount(List<CardData> cards)
    {
        var seen = new System.Collections.Generic.HashSet<string>();
        foreach (var c in cards) seen.Add(c.cardName);
        return seen.Count;
    }

    // 선택된 카드 count장 → usedCards, 나머지 → carriedOver
    public CardData SelectCard(CardData selected, int count)
    {
        int removed = 0;
        for (int i = _currentThree.Count - 1; i >= 0 && removed < count; i--)
        {
            if (_currentThree[i].cardName == selected.cardName)
            {
                _state.usedCards.Add(_currentThree[i]);
                _currentThree.RemoveAt(i);
                removed++;
            }
        }

        foreach (var c in _currentThree)
            _state.carriedOver.Add(c);
        _currentThree.Clear();
        return selected;
    }

    public event System.Action<CardData> OnCardAddedToDeck;

    public void AddCardToDeck(CardData card)
    {
        _state.deck.Add(card);
        OnCardAddedToDeck?.Invoke(card);
    }

    public void RemoveCardFromDeck(CardData card) => _state.deck.Remove(card);
}

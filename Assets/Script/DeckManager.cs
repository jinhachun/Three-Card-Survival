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

    // carriedOver + 덱 상단 보충 → 최대 3장 반환
    // 덱 소진 후에도 carriedOver만으로 계속 진행
    public IReadOnlyList<CardData> GetThreeCards()
    {
        _currentThree.Clear();
        _currentThree.AddRange(_state.carriedOver);
        _state.carriedOver.Clear();

        while (_currentThree.Count < 3 && _state.deck.Count > 0)
        {
            _currentThree.Add(_state.deck[0]);
            _state.deck.RemoveAt(0);
        }

        return _currentThree;
    }

    // 선택된 카드 → usedCards, 나머지 → carriedOver
    public CardData SelectCard(int index)
    {
        var selected = _currentThree[index];
        _state.usedCards.Add(selected);

        for (int i = 0; i < _currentThree.Count; i++)
            if (i != index)
                _state.carriedOver.Add(_currentThree[i]);

        _currentThree.Clear();
        return selected;
    }

    public void AddCardToDeck(CardData card) => _state.deck.Add(card);

    public void RemoveCardFromDeck(CardData card) => _state.deck.Remove(card);
}

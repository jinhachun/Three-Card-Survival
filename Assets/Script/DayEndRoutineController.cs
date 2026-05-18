using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DayEndRoutineController : MonoBehaviour
{
    [Required] [SerializeField] private CardRegistry cardRegistry;

    // UI가 구독해서 선택 UI를 띄우고, 완료 시 콜백을 호출
    public event Action<List<CardData>, Action<int>> OnRequestRewardChoice;

    public void Run(GameState state, DeckManager deckManager, Action onComplete)
    {
        StartCoroutine(RunSequence(state, deckManager, onComplete));
    }

    private IEnumerator RunSequence(GameState state, DeckManager deckManager, Action onComplete)
    {
        // 0. 이번 날 사용한 카드 전부 덱에 반환
        state.deck.AddRange(state.usedCards);
        state.usedCards.Clear();
        state.deck.AddRange(state.carriedOver);
        state.carriedOver.Clear();

        // 1. 카드 보상: 보상 풀에서 3장 제시 → 1장 선택 → 덱에 추가
        var candidates = GetShuffledCandidates(state.day, state);
        CardData chosen = null;
        if (OnRequestRewardChoice != null)
        {
            OnRequestRewardChoice.Invoke(candidates, idx => chosen = candidates[idx]);
            yield return new WaitUntil(() => chosen != null);
        }
        else
        {
            chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
        deckManager.AddCardToDeck(CardUtils.Clone(chosen));

        // 2. 랜덤 시련 카드 1장 덱에 추가
        var pool = cardRegistry.trialPool;
        deckManager.AddCardToDeck(CardUtils.Clone(pool[UnityEngine.Random.Range(0, pool.Length)]));

        // 5. 마일스톤: 5일차에 SOS 카드 추가
        if (state.day == 5)
            deckManager.AddCardToDeck(CardUtils.Clone(cardRegistry.sosCard));

        // 6. HP 전량 회복 후 날짜 증가 및 덱 셔플
        state.hp = state.maxHp;
        state.day += 1;
        deckManager.Shuffle();

        onComplete?.Invoke();
    }

    private List<CardData> GetShuffledCandidates(int currentDay, GameState state)
    {
        var pool = new List<CardData>();
        foreach (var card in cardRegistry.rewardPool)
        {
            if (card == null) continue;
            if (card.minDay > currentDay) continue;
            if (!string.IsNullOrEmpty(card.requiredBuilding) && !state.IsBuildingComplete(card.requiredBuilding)) continue;
            if (card.cardType == CardType.Building && IsCompletedBuildingCard(card, state)) continue;
            pool.Add(card);
        }

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool.GetRange(0, Mathf.Min(3, pool.Count));
    }

    private static bool IsCompletedBuildingCard(CardData card, GameState state)
    {
        foreach (var effect in card.effects)
            if (effect is BuildingProgressEffectSO bpe && state.IsBuildingComplete(bpe.buildingData.buildingName))
                return true;
        return false;
    }

    private void MergeCards(GameState state, DeckManager deckManager, int idxA, int idxB)
    {
        var a = state.deck[idxA];
        var b = state.deck[idxB];

        var merged = ScriptableObject.CreateInstance<CardData>();
        merged.cardName           = $"{a.cardName}+{b.cardName}";
        merged.cardType           = a.cardType;
        merged.costs              = new List<CardCost>(a.costs);
        merged.conditions         = new List<CardCondition>(a.conditions);
        merged.onRefuseCopyToDeck = a.onRefuseCopyToDeck;
        merged.effects            = new List<CardEffectSO>();
        foreach (var e in a.effects) merged.effects.Add(Instantiate(e));
        foreach (var e in b.effects) merged.effects.Add(Instantiate(e));

        deckManager.RemoveCardFromDeck(a);
        deckManager.RemoveCardFromDeck(b);
        deckManager.AddCardToDeck(merged);
    }

}

using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class EffectResolver : MonoBehaviour
{
    [Required] [SerializeField] private BuildingRegistry buildingRegistry;

    private DeckManager _deckManager;

    public event Action<BuildingData> OnBuildingCompleted;
    public event Action<BuildingData> OnPassiveApplied;

    public void Init(DeckManager deckManager) => _deckManager = deckManager;

    public void ApplyCard(CardData card, int count, GameState state)
    {
        foreach (var cost in card.costs)
            state.AddResource(cost.resource, -(cost.amount * count) - state.costPenalty);

        // 건물 카드라면 Apply 전 완성 여부 스냅샷
        BuildingProgressEffectSO buildingEffect = null;
        bool wasComplete = false;
        foreach (var effect in card.effects)
        {
            if (effect is BuildingProgressEffectSO bpe)
            {
                buildingEffect = bpe;
                wasComplete    = state.IsBuildingComplete(bpe.buildingData.buildingName);
                break;
            }
        }

        for (int i = 0; i < count; i++)
            foreach (var effect in card.effects)
                effect.Apply(state);

        // 방금 완성된 경우에만 처리 (이미 완성 상태였으면 스킵)
        if (buildingEffect != null && !wasComplete
            && state.IsBuildingComplete(buildingEffect.buildingData.buildingName))
        {
            state.deck.RemoveAll(c => c.cardName == card.cardName);
            state.carriedOver.RemoveAll(c => c.cardName == card.cardName);
            state.usedCards.RemoveAll(c => c.cardName == card.cardName);
            OnBuildingCompleted?.Invoke(buildingEffect.buildingData);
        }

        // 완성된 건물 패시브 적용 (매 카드 선택 시)
        if (buildingRegistry != null)
            foreach (var b in buildingRegistry.buildings)
                if (state.IsBuildingComplete(b.buildingName) && b.passiveResource != ResourceType.None)
                {
                    state.AddResource(b.passiveResource, b.passiveAmount);
                    OnPassiveApplied?.Invoke(b);
                }
    }

    public void OnRefuseTrial(CardData card, GameState state)
    {
        state.AddResource(ResourceType.HP, -1);
    }

    public void ApplyEscapeFailurePenalty(GameState state) => state.costPenalty += 1;
}

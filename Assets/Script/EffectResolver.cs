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
        int escalation = state.GetCostEscalation(card.cardName);
        int discount   = state.freeTurnActive ? 1 : 0;
        foreach (var cost in card.costs)
            state.AddResource(cost.resource, -Mathf.Max(0, cost.amount + escalation + state.costPenalty - discount));

        // 어떤 효과든 건물을 완성할 수 있으므로 스냅샷으로 감지
        var completedBefore = new System.Collections.Generic.HashSet<string>(state.completedBuildings);

        for (int i = 0; i < count; i++)
            foreach (var effect in card.effects)
                effect.Apply(state);

        // 새로 완성된 건물 처리
        foreach (var buildingName in state.completedBuildings)
        {
            if (completedBefore.Contains(buildingName)) continue;

            string buildCardName = $"Build {buildingName}";
            state.deck.RemoveAll(c => c.cardName == buildCardName);
            state.carriedOver.RemoveAll(c => c.cardName == buildCardName);
            state.usedCards.RemoveAll(c => c.cardName == buildCardName);

            var bd = System.Array.Find(buildingRegistry.buildings, b => b.buildingName == buildingName);
            if (bd != null) OnBuildingCompleted?.Invoke(bd);
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

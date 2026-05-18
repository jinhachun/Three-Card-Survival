using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ThreeCardSurvival/Effects/BuildingProgress")]
public class BuildingProgressEffectSO : CardEffectSO
{
    [Required] public BuildingData buildingData;

    public override void Apply(GameState state)
    {
        string name = buildingData.buildingName;
        if (state.IsBuildingComplete(name)) return;

        int newProgress = state.GetBuildingProgress(name) + buildingData.progressPerUse;
        state.buildingProgress[name] = Mathf.Min(100, newProgress);

        if (newProgress >= 100)
        {
            state.completedBuildings.Add(name);
            if (buildingData.completionStatGain != StatType.None)
                state.AddStat(buildingData.completionStatGain, buildingData.completionStatAmount);
        }
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "ThreeCardSurvival/BuildingRegistry")]
public class BuildingRegistry : ScriptableObject
{
    public BuildingData[] buildings = System.Array.Empty<BuildingData>();
}

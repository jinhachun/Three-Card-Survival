using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ThreeCardSurvival/Effects/Resource")]
public class ResourceEffectSO : CardEffectSO
{
    [EnumToggleButtons] public ResourceType resource;
    public int amount;

    public override void Apply(GameState state) => state.AddResource(resource, amount);
}

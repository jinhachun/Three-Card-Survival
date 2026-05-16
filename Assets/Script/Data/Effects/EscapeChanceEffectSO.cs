using UnityEngine;

[CreateAssetMenu(menuName = "ThreeCardSurvival/Effects/EscapeChance")]
public class EscapeChanceEffectSO : CardEffectSO
{
    public float amount;

    public override void Apply(GameState state) => state.escapeChance += amount;
}

using UnityEngine;

[CreateAssetMenu(menuName = "ThreeCardSurvival/Effects/FreeTurn")]
public class FreeTurnEffectSO : CardEffectSO
{
    public override void Apply(GameState state)
    {
        state.freeTurnPending = true;
    }
}

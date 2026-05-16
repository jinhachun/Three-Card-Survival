using UnityEngine;

public abstract class CardEffectSO : ScriptableObject
{
    public abstract void Apply(GameState state);
}

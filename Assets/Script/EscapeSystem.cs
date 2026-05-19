using UnityEngine;

public class EscapeSystem : MonoBehaviour
{
    public bool TryEscape(GameState state) => Random.value < state.escapeChance;

    public float PreviewEscapeChance(GameState state) => state.escapeChance;
}

using UnityEngine;

public class EscapeSystem : MonoBehaviour
{
    // 탈출 시도 전에 확률을 먼저 누적한 뒤 판정
    public bool TryEscape(GameState state)
    {
        state.escapeChance += (state.strength + state.agility + state.intelligence) * 0.01f;
        state.escapeChance += (state.food + state.water) * 0.005f;

        return Random.value < state.escapeChance;
    }
}

using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ThreeCardSurvival/Effects/Stat")]
public class StatEffectSO : CardEffectSO
{
    [EnumToggleButtons] public StatType stat;
    public int amount;

    public override void Apply(GameState state) => state.AddStat(stat, amount);
}

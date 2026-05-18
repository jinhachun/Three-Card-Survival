using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ThreeCardSurvival/BuildingData")]
public class BuildingData : ScriptableObject
{
    [BoxGroup("기본")] public string buildingName;
    [BoxGroup("기본")] public int minDay = 1;

    [BoxGroup("건설"), ListDrawerSettings(ShowIndexLabels = false)]
    public List<CardCost> buildCosts = new();

    [BoxGroup("건설")] public int progressPerUse = 25;

    [BoxGroup("완성 효과"), EnumToggleButtons]
    public StatType completionStatGain = StatType.None;

    [BoxGroup("완성 효과"), ShowIf("@completionStatGain != StatType.None")]
    public int completionStatAmount = 1;

    [BoxGroup("패시브"), EnumToggleButtons]
    public ResourceType passiveResource = ResourceType.None;

    [BoxGroup("패시브"), ShowIf("@passiveResource != ResourceType.None")]
    public int passiveAmount = 1;

    [BoxGroup("카드 언락")]
    public string[] unlocksCards = Array.Empty<string>();
}

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public struct CardCost
{
    [EnumToggleButtons] public ResourceType resource;
    public int amount;
}

[Serializable]
public struct CardCondition
{
    [EnumToggleButtons] public StatType stat;
    public int minAmount;
}

[CreateAssetMenu(menuName = "ThreeCardSurvival/CardData")]
public class CardData : ScriptableObject
{
    [BoxGroup("기본")]
    public string cardName;

    [BoxGroup("기본"), EnumToggleButtons]
    public CardType cardType;

    [BoxGroup("선택 비용"), ListDrawerSettings(ShowIndexLabels = false)]
    public List<CardCost> costs = new();

    [BoxGroup("조건"), ListDrawerSettings(ShowIndexLabels = false)]
    public List<CardCondition> conditions = new();

    [BoxGroup("효과"), InlineEditor, ListDrawerSettings(ShowIndexLabels = false)]
    public List<CardEffectSO> effects = new();

    [BoxGroup("시련"), ShowIf("@cardType == CardType.Trial")]
    public bool onRefuseCopyToDeck;
}

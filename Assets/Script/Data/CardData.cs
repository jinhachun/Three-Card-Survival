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

    [BoxGroup("기본")]
    public int minDay = 1;

    // 비어 있으면 조건 없음. 해당 buildingName이 완성되어야 보상 풀에 등장
    [BoxGroup("기본")]
    public string requiredBuilding = "";

    [BoxGroup("기본"), PreviewField(55)]
    public Sprite cardSprite;

    [BoxGroup("선택 비용"), ListDrawerSettings(ShowIndexLabels = false)]
    public List<CardCost> costs = new();

    [BoxGroup("조건"), ListDrawerSettings(ShowIndexLabels = false)]
    public List<CardCondition> conditions = new();

    [BoxGroup("효과"), InlineEditor, ListDrawerSettings(ShowIndexLabels = false)]
    public List<CardEffectSO> effects = new();

    [BoxGroup("시련"), ShowIf("@cardType == CardType.Trial")]
    public bool onRefuseCopyToDeck;

    // 손패에 등장할 때마다 비용 영구 +1
    [BoxGroup("시련"), ShowIf("@cardType == CardType.Trial")]
    public bool escalatesOnDraw;
}

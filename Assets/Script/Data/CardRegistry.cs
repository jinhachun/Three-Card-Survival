using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public struct StarterCardEntry
{
    public CardData card;
    public int count;
}

[CreateAssetMenu(menuName = "ThreeCardSurvival/CardRegistry")]
public class CardRegistry : ScriptableObject
{
    [BoxGroup("스타터 덱")]
    public StarterCardEntry[] starterDeck;

    [BoxGroup("보상 풀")]
    public CardData[] rewardPool;

    [BoxGroup("시련 풀")]
    public CardData[] trialPool;

    [BoxGroup("특수")]
    public CardData sosCard;
}

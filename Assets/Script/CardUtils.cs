using System.Collections.Generic;
using UnityEngine;

public static class CardUtils
{
    // SO 원본을 보호하기 위해 런타임 복사본 생성
    public static CardData Clone(CardData source)
    {
        var clone = ScriptableObject.CreateInstance<CardData>();
        clone.cardName           = source.cardName;
        clone.cardType           = source.cardType;
        clone.costs              = new List<CardCost>(source.costs);
        clone.conditions         = new List<CardCondition>(source.conditions);
        clone.onRefuseCopyToDeck = source.onRefuseCopyToDeck;
        clone.effects            = new List<CardEffectSO>();
        foreach (var e in source.effects)
            clone.effects.Add(Object.Instantiate(e));
        return clone;
    }
}

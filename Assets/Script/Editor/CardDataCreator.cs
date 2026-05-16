#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CardDataCreator
{
    private const string SavePath = "Assets/Data/Cards";

    [MenuItem("ThreeCardSurvival/카드 에셋 일괄 생성")]
    public static void CreateAllCards()
    {
        // Starter deck
        Create("Forage",    CardType.Resource, effects: new() { Res(ResourceType.Food,  2) });
        Create("Collect Water", CardType.Resource, effects: new() { Res(ResourceType.Water, 2) });
        Create("Quarry",    CardType.Resource, effects: new() { Res(ResourceType.Stone, 2) });
        Create("Logging",   CardType.Resource, effects: new() { Res(ResourceType.Wood,  2) });
        Create("End of Day", CardType.DayEnd);

        // Trial cards
        Create("Hunger",  CardType.Trial, costs: new() { new() { resource = ResourceType.Food,  amount = 3 } }, onRefuseCopyToDeck: true);
        Create("Thirst",  CardType.Trial, costs: new() { new() { resource = ResourceType.Water, amount = 3 } }, onRefuseCopyToDeck: true);
        Create("Injury",  CardType.Trial, costs: new() { new() { resource = ResourceType.HP,    amount = 3 } }, onRefuseCopyToDeck: true);

        // SOS: Stone -2, Wood -2 → escapeChance +0.1
        Create("SOS", CardType.SOS,
            costs: new() {
                new() { resource = ResourceType.Stone, amount = 2 },
                new() { resource = ResourceType.Wood,  amount = 2 }
            },
            effects: new() { Escape(0.1f) });

        // Reward cards
        Create("Hunting", CardType.Resource,
            conditions: new() { new() { stat = StatType.Strength, minAmount = 2 } },
            effects: new() { Res(ResourceType.Food, 4) });
        Create("Fishing",       CardType.Resource, effects: new() { Res(ResourceType.Food, 3), Res(ResourceType.Water, 1) });
        Create("Rain Collect",  CardType.Resource, effects: new() { Res(ResourceType.Water, 4) });
        Create("Str Training",  CardType.Stat, effects: new() { Stat(StatType.Strength,     1) });
        Create("Agi Training",  CardType.Stat, effects: new() { Stat(StatType.Agility,      1) });
        Create("Observe",       CardType.Stat, effects: new() { Stat(StatType.Intelligence, 1) });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CardDataCreator] 카드 에셋 15장 생성 완료 → {SavePath}");
    }

    // --- 이펙트 팩토리 ---

    private static ResourceEffectSO Res(ResourceType r, int amount)
    {
        var e = ScriptableObject.CreateInstance<ResourceEffectSO>();
        e.resource = r; e.amount = amount;
        return e;
    }

    private static StatEffectSO Stat(StatType s, int amount)
    {
        var e = ScriptableObject.CreateInstance<StatEffectSO>();
        e.stat = s; e.amount = amount;
        return e;
    }

    private static EscapeChanceEffectSO Escape(float amount)
    {
        var e = ScriptableObject.CreateInstance<EscapeChanceEffectSO>();
        e.amount = amount;
        return e;
    }

    // --- 에셋 생성 ---

    private static void Create(
        string cardName,
        CardType cardType,
        List<CardCost>         costs      = null,
        List<CardCondition>    conditions = null,
        List<CardEffectSO>     effects    = null,
        bool onRefuseCopyToDeck           = false)
    {
        var asset = ScriptableObject.CreateInstance<CardData>();
        asset.cardName           = cardName;
        asset.cardType           = cardType;
        asset.costs              = costs      ?? new();
        asset.conditions         = conditions ?? new();
        asset.effects            = effects    ?? new();
        asset.onRefuseCopyToDeck = onRefuseCopyToDeck;

        string path = $"{SavePath}/{cardName}.asset";
        AssetDatabase.CreateAsset(asset, path);

        // 이펙트 SO를 CardData 에셋 안에 서브에셋으로 embed
        if (effects != null)
            foreach (var e in effects)
            {
                e.name = e.GetType().Name;
                AssetDatabase.AddObjectToAsset(e, asset);
            }
    }
}
#endif

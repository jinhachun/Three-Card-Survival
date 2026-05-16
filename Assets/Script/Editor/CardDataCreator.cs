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
        Create("Forage",        CardType.Resource, costs: new() { new() { resource = ResourceType.Water, amount = 2 } }, effects: new() { Res(ResourceType.Food,  2) });
        Create("Collect Water", CardType.Resource, costs: new() { new() { resource = ResourceType.Food,  amount = 2 } }, effects: new() { Res(ResourceType.Water, 2) });
        Create("Mine",          CardType.Resource, costs: new() { new() { resource = ResourceType.HP,    amount = 2 } }, effects: new() { Res(ResourceType.Stone, 3) });
        Create("Log",           CardType.Resource, costs: new() { new() { resource = ResourceType.HP,    amount = 2 } }, effects: new() { Res(ResourceType.Wood,  3) });
        Create("End of Day",    CardType.DayEnd);

        // Trial cards
        Create("Hunger", CardType.Trial, costs: new() { new() { resource = ResourceType.Food,  amount = 3 } }, onRefuseCopyToDeck: true);
        Create("Thirst", CardType.Trial, costs: new() { new() { resource = ResourceType.Water, amount = 3 } }, onRefuseCopyToDeck: true);
        Create("Injury", CardType.Trial, costs: new() { new() { resource = ResourceType.HP,    amount = 3 } }, onRefuseCopyToDeck: true);

        // SOS: Stone -2, Wood -2 → escapeChance +0.1
        Create("SOS", CardType.SOS,
            costs: new() {
                new() { resource = ResourceType.Stone, amount = 2 },
                new() { resource = ResourceType.Wood,  amount = 2 }
            },
            effects: new() { Escape(0.1f) });

        // Reward cards
        Create("Craft Food",  CardType.Resource, costs: new() { new() { resource = ResourceType.Stone, amount = 2 } }, effects: new() { Res(ResourceType.Food,  5) });
        Create("Dig Well",    CardType.Resource, costs: new() { new() { resource = ResourceType.Wood,  amount = 2 } }, effects: new() { Res(ResourceType.Water, 5) });
        Create("Hunting",     CardType.Resource,
            costs: new() { new() { resource = ResourceType.HP, amount = 3 } },
            conditions: new() { new() { stat = StatType.Strength, minAmount = 2 } },
            effects: new() { Res(ResourceType.Food, 6) });
        Create("Rest",        CardType.Resource,
            costs: new() { new() { resource = ResourceType.Food, amount = 2 }, new() { resource = ResourceType.Water, amount = 1 } },
            effects: new() { Res(ResourceType.HP, 5) });
        Create("Str Training", CardType.Stat, costs: new() { new() { resource = ResourceType.Food,  amount = 2 } }, effects: new() { Stat(StatType.Strength, 1) });
        Create("Agi Training", CardType.Stat, costs: new() { new() { resource = ResourceType.Water, amount = 2 } }, effects: new() { Stat(StatType.Agility,  1) });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PopulateRegistry();
        Debug.Log($"[CardDataCreator] 카드 에셋 15장 생성 + 레지스트리 자동 등록 완료 → {SavePath}");
    }

    // --- 레지스트리 자동 등록 ---

    private static void PopulateRegistry()
    {
        const string registryPath = "Assets/Data/CardRegistry.asset";
        var registry = AssetDatabase.LoadAssetAtPath<CardRegistry>(registryPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<CardRegistry>();
            AssetDatabase.CreateAsset(registry, registryPath);
        }

        registry.starterDeck = new StarterCardEntry[]
        {
            new() { card = Load("Forage"),        count = 2 },
            new() { card = Load("Collect Water"), count = 2 },
            new() { card = Load("Mine"),          count = 2 },
            new() { card = Load("Log"),           count = 2 },
            new() { card = Load("End of Day"),    count = 1 },
        };

        registry.rewardPool = new CardData[]
        {
            Load("Craft Food"), Load("Dig Well"), Load("Hunting"),
            Load("Rest"), Load("Str Training"), Load("Agi Training"),
        };

        registry.trialPool = new CardData[] { Load("Hunger"), Load("Thirst"), Load("Injury") };
        registry.sosCard   = Load("SOS");

        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
    }

    private static CardData Load(string cardName)
        => AssetDatabase.LoadAssetAtPath<CardData>($"{SavePath}/{cardName}.asset");

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

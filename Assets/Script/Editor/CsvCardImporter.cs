#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CsvCardImporter
{
    private const string CsvPath  = "Assets/Data/Cards.csv";
    private const string SavePath = "Assets/Data/Cards";

    // 스타터 덱 구성: CSV에 없는 정보이므로 여기에 고정
    private static readonly Dictionary<string, int> StarterCounts = new()
    {
        { "Forage",        2 },
        { "Collect Water", 2 },
        { "Mine",          2 },
        { "Log",           2 },
        { "End of Day",    1 },
    };

    [MenuItem("ThreeCardSurvival/CSV에서 카드 임포트")]
    public static void Import()
    {
        string fullPath = Path.Combine(Application.dataPath, "..", CsvPath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[CsvCardImporter] 파일 없음: {fullPath}");
            return;
        }

        var lines   = File.ReadAllLines(fullPath);
        var created = new List<CardData>();

        for (int i = 1; i < lines.Length; i++) // 헤더 skip
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var card = ParseRow(SplitCsv(line));
            if (card != null) created.Add(card);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PopulateRegistry(created);
        Debug.Log($"[CsvCardImporter] {created.Count}장 임포트 완료 → {SavePath}");
    }

    // ── 행 파싱 ───────────────────────────────────────────────────────

    // 열 순서: Name(0) CardType(1) MinDay(2) Costs(3) Conditions(4) Effects(5) OnRefuseCopyToDeck(6) RequiredBuilding(7) EscalatesOnDraw(8, optional)
    private static CardData ParseRow(string[] f)
    {
        if (f.Length < 7) return null;

        string cardName      = f[0].Trim();
        string cardTypeStr   = f[1].Trim();
        string minDayStr     = f[2].Trim();
        string costsStr      = f[3].Trim();
        string conditionsStr = f[4].Trim();
        string effectsStr    = f[5].Trim();
        string refuseStr     = f[6].Trim();

        if (!Enum.TryParse<CardType>(cardTypeStr, out var cardType))
        {
            Debug.LogWarning($"[CsvCardImporter] 알 수 없는 CardType '{cardTypeStr}' ({cardName})");
            return null;
        }

        var effects = ParseEffects(effectsStr);

        int.TryParse(minDayStr, out int minDay);
        if (minDay < 1) minDay = 1;

        string requiredBuilding = f.Length > 7 ? f[7].Trim() : "";
        bool   escalatesOnDraw  = f.Length > 8 && f[8].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);

        var asset = ScriptableObject.CreateInstance<CardData>();
        asset.cardName            = cardName;
        asset.cardType            = cardType;
        asset.minDay              = minDay;
        asset.requiredBuilding    = requiredBuilding;
        asset.costs              = ParseCosts(costsStr);
        asset.conditions         = ParseConditions(conditionsStr);
        asset.effects            = effects;
        asset.onRefuseCopyToDeck = refuseStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        asset.escalatesOnDraw    = escalatesOnDraw;

        string assetPath = $"{SavePath}/{cardName}.asset";
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(asset, assetPath);

        foreach (var e in effects)
        {
            e.name = e.GetType().Name;
            AssetDatabase.AddObjectToAsset(e, asset);
        }

        return asset;
    }

    // ── 필드 파서 ─────────────────────────────────────────────────────

    private static List<CardCost> ParseCosts(string s)
    {
        var result = new List<CardCost>();
        if (string.IsNullOrEmpty(s)) return result;
        foreach (var part in s.Split('|'))
        {
            var t = part.Trim().Split(':');
            if (t.Length != 2) continue;
            if (!Enum.TryParse<ResourceType>(t[0], out var res)) continue;
            if (!int.TryParse(t[1], out int amount)) continue;
            result.Add(new CardCost { resource = res, amount = amount });
        }
        return result;
    }

    private static List<CardCondition> ParseConditions(string s)
    {
        var result = new List<CardCondition>();
        if (string.IsNullOrEmpty(s)) return result;
        foreach (var part in s.Split('|'))
        {
            var t = part.Trim().Split(':');
            if (t.Length != 2) continue;
            if (!Enum.TryParse<StatType>(t[0], out var stat)) continue;
            if (!int.TryParse(t[1], out int minAmount)) continue;
            result.Add(new CardCondition { stat = stat, minAmount = minAmount });
        }
        return result;
    }

    private static List<CardEffectSO> ParseEffects(string s)
    {
        var result = new List<CardEffectSO>();
        if (string.IsNullOrEmpty(s)) return result;
        foreach (var part in s.Split('|'))
        {
            var effect = ParseEffect(part.Trim());
            if (effect != null) result.Add(effect);
        }
        return result;
    }

    private static CardEffectSO ParseEffect(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        var t = s.Split(':');

        switch (t[0])
        {
            case "Resource":
                if (t.Length == 3
                    && Enum.TryParse<ResourceType>(t[1], out var res)
                    && int.TryParse(t[2], out int resAmt))
                {
                    var e = ScriptableObject.CreateInstance<ResourceEffectSO>();
                    e.resource = res; e.amount = resAmt;
                    return e;
                }
                break;

            case "Stat":
                if (t.Length == 3
                    && Enum.TryParse<StatType>(t[1], out var stat)
                    && int.TryParse(t[2], out int statAmt))
                {
                    var e = ScriptableObject.CreateInstance<StatEffectSO>();
                    e.stat = stat; e.amount = statAmt;
                    return e;
                }
                break;

            case "EscapeChance":
                if (t.Length == 2
                    && float.TryParse(t[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float ec))
                {
                    var e = ScriptableObject.CreateInstance<EscapeChanceEffectSO>();
                    e.amount = ec;
                    return e;
                }
                break;

        }

        Debug.LogWarning($"[CsvCardImporter] 파싱 실패한 효과: '{s}'");
        return null;
    }

    // ── 레지스트리 갱신 ───────────────────────────────────────────────

    private static void PopulateRegistry(List<CardData> all)
    {
        const string registryPath = "Assets/Data/CardRegistry.asset";
        var registry = AssetDatabase.LoadAssetAtPath<CardRegistry>(registryPath)
                       ?? CreateRegistry(registryPath);

        // 스타터 덱
        var starterEntries = new List<StarterCardEntry>();
        foreach (var kv in StarterCounts)
        {
            var card = all.Find(c => c.cardName == kv.Key);
            if (card != null)
                starterEntries.Add(new StarterCardEntry { card = card, count = kv.Value });
            else
                Debug.LogWarning($"[CsvCardImporter] 스타터 카드 없음: '{kv.Key}'");
        }
        registry.starterDeck = starterEntries.ToArray();

        // 시련 풀 / SOS / 보상 풀
        registry.trialPool  = all.FindAll(c => c.cardType == CardType.Trial).ToArray();
        registry.sosCard    = all.Find(c => c.cardType == CardType.SOS);

        var starterNames = new HashSet<string>(StarterCounts.Keys);
        var rewardList = all.FindAll(c =>
            c.cardType != CardType.Trial &&
            c.cardType != CardType.SOS   &&
            c.cardType != CardType.DayEnd &&
            !starterNames.Contains(c.cardName));

        // BuildingRegistry에서 Build X 카드 추가
        var buildingRegistry = AssetDatabase.LoadAssetAtPath<BuildingRegistry>("Assets/Data/BuildingRegistry.asset");
        if (buildingRegistry != null)
            foreach (var b in buildingRegistry.buildings)
            {
                var bc = AssetDatabase.LoadAssetAtPath<CardData>($"Assets/Data/Cards/Build {b.buildingName}.asset");
                if (bc != null) rewardList.Add(bc);
            }

        registry.rewardPool = rewardList.ToArray();

        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
    }

    private static CardRegistry CreateRegistry(string path)
    {
        var r = ScriptableObject.CreateInstance<CardRegistry>();
        AssetDatabase.CreateAsset(r, path);
        return r;
    }

    // ── CSV 파서 (따옴표 내 쉼표 대응) ───────────────────────────────

    private static string[] SplitCsv(string line)
    {
        var result   = new List<string>();
        var current  = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
                inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
                current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
#endif

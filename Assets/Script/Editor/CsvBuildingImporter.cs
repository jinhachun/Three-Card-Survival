#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CsvBuildingImporter
{
    private const string CsvPath          = "Assets/Data/Buildings.csv";
    private const string BuildingSavePath = "Assets/Data/Buildings";
    private const string CardSavePath     = "Assets/Data/Cards";
    private const string RegistryPath     = "Assets/Data/BuildingRegistry.asset";

    [MenuItem("ThreeCardSurvival/Buildings CSV 임포트")]
    public static void Import()
    {
        string fullPath = Path.Combine(Application.dataPath, "..", CsvPath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[CsvBuildingImporter] 파일 없음: {fullPath}");
            return;
        }

        if (!AssetDatabase.IsValidFolder(BuildingSavePath))
            AssetDatabase.CreateFolder("Assets/Data", "Buildings");

        var lines    = File.ReadAllLines(fullPath);
        var buildings = new List<BuildingData>();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var building = ParseBuildingRow(SplitCsv(line));
            if (building != null) buildings.Add(building);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Build X 카드 생성
        var buildCards = new List<CardData>();
        foreach (var b in buildings)
        {
            var card = CreateBuildCard(b);
            if (card != null) buildCards.Add(card);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        PopulateBuildingRegistry(buildings);
        AppendToCardRegistry(buildCards);

        Debug.Log($"[CsvBuildingImporter] 건물 {buildings.Count}개, 카드 {buildCards.Count}장 임포트 완료");
    }

    [MenuItem("ThreeCardSurvival/전체 임포트 (카드 + 건물)")]
    public static void ImportAll()
    {
        Import();
        CsvCardImporter.Import();
        Debug.Log("[CsvBuildingImporter] 전체 임포트 완료");
    }

    // ── 행 파싱 ───────────────────────────────────────────────────────

    // 열 순서: Name(0) MinDay(1) ProgressPerUse(2) BuildCosts(3) CompletionEffect(4) PassiveEffect(5) UnlocksCards(6, optional)
    private static BuildingData ParseBuildingRow(string[] f)
    {
        if (f.Length < 6) return null;

        string name = f[0].Trim();
        if (string.IsNullOrEmpty(name)) return null;

        int.TryParse(f[1].Trim(), out int minDay);
        if (minDay < 1) minDay = 1;

        int.TryParse(f[2].Trim(), out int progressPerUse);
        if (progressPerUse < 1) progressPerUse = 25;

        string assetPath = $"{BuildingSavePath}/{name}.asset";
        AssetDatabase.DeleteAsset(assetPath);
        var asset = ScriptableObject.CreateInstance<BuildingData>();
        AssetDatabase.CreateAsset(asset, assetPath);

        asset.buildingName   = name;
        asset.minDay         = minDay;
        asset.progressPerUse = progressPerUse;
        asset.buildCosts     = ParseCosts(f[3].Trim());

        ParseCompletionEffect(f[4].Trim(), asset);
        ParsePassiveEffect(f[5].Trim(), asset);

        asset.unlocksCards = f.Length > 6 ? ParseUnlocks(f[6].Trim()) : Array.Empty<string>();

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void ParseCompletionEffect(string s, BuildingData asset)
    {
        if (string.IsNullOrEmpty(s)) return;
        var t = s.Split(':');
        if (t.Length == 3 && t[0] == "Stat"
            && Enum.TryParse<StatType>(t[1], out var stat)
            && int.TryParse(t[2], out int amt))
        {
            asset.completionStatGain   = stat;
            asset.completionStatAmount = amt;
        }
    }

    private static void ParsePassiveEffect(string s, BuildingData asset)
    {
        if (string.IsNullOrEmpty(s)) return;
        var t = s.Split(':');
        if (t.Length == 3 && t[0] == "Resource"
            && Enum.TryParse<ResourceType>(t[1], out var res)
            && int.TryParse(t[2], out int amt))
        {
            asset.passiveResource = res;
            asset.passiveAmount   = amt;
        }
    }

    private static string[] ParseUnlocks(string s)
    {
        if (string.IsNullOrEmpty(s)) return Array.Empty<string>();
        var parts = s.Split('|');
        var result = new List<string>();
        foreach (var p in parts)
        {
            string trimmed = p.Trim();
            if (!string.IsNullOrEmpty(trimmed)) result.Add(trimmed);
        }
        return result.ToArray();
    }

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

    // ── Build X 카드 생성 ─────────────────────────────────────────────

    private static CardData CreateBuildCard(BuildingData building)
    {
        string cardName  = $"Build {building.buildingName}";
        string assetPath = $"{CardSavePath}/{cardName}.asset";
        AssetDatabase.DeleteAsset(assetPath);

        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName  = cardName;
        card.cardType  = CardType.Building;
        card.minDay    = building.minDay;
        card.costs     = new List<CardCost>(building.buildCosts);

        AssetDatabase.CreateAsset(card, assetPath);

        var effect = ScriptableObject.CreateInstance<BuildingProgressEffectSO>();
        effect.buildingData = building;
        effect.name         = nameof(BuildingProgressEffectSO);
        AssetDatabase.AddObjectToAsset(effect, card);

        card.effects = new List<CardEffectSO> { effect };
        EditorUtility.SetDirty(card);
        return card;
    }

    // ── 레지스트리 갱신 ───────────────────────────────────────────────

    private static void PopulateBuildingRegistry(List<BuildingData> buildings)
    {
        var registry = AssetDatabase.LoadAssetAtPath<BuildingRegistry>(RegistryPath)
                       ?? CreateBuildingRegistry();
        registry.buildings = buildings.ToArray();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
    }

    private static BuildingRegistry CreateBuildingRegistry()
    {
        var r = ScriptableObject.CreateInstance<BuildingRegistry>();
        AssetDatabase.CreateAsset(r, RegistryPath);
        return r;
    }

    private static void AppendToCardRegistry(List<CardData> buildCards)
    {
        const string cardRegistryPath = "Assets/Data/CardRegistry.asset";
        var cardRegistry = AssetDatabase.LoadAssetAtPath<CardRegistry>(cardRegistryPath);
        if (cardRegistry == null) return;

        var pool = new List<CardData>(cardRegistry.rewardPool ?? Array.Empty<CardData>());
        // 기존 Building 타입 카드 제거 후 재추가 (중복 방지)
        pool.RemoveAll(c => c != null && c.cardType == CardType.Building);
        pool.AddRange(buildCards);
        cardRegistry.rewardPool = pool.ToArray();

        EditorUtility.SetDirty(cardRegistry);
        AssetDatabase.SaveAssets();
    }

    // ── CSV 파서 ─────────────────────────────────────────────────────

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

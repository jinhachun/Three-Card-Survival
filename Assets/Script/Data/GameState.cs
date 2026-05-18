using System.Collections.Generic;

public class GameState
{
    // 자원
    public int maxHp     = 10;
    public int hp        = 10;
    public int food      = 5;
    public int water     = 5;
    public int stone     = 0;
    public int wood      = 0;

    // 스탯
    public int strength     = 1;
    public int agility      = 1;
    public int intelligence = 1;

    // 덱
    public List<CardData> deck        = new();
    public List<CardData> usedCards   = new();
    public List<CardData> carriedOver = new();

    // 건물
    public Dictionary<string, int> buildingProgress   = new();
    public HashSet<string>         completedBuildings  = new();

    public int  GetBuildingProgress(string name) => buildingProgress.TryGetValue(name, out int v) ? v : 0;
    public bool IsBuildingComplete(string name)  => completedBuildings.Contains(name);

    // 진행
    public int   day          = 1;
    public float escapeChance = 0f;
    public int   costPenalty  = 0;  // 탈출 실패 누적 패널티
    public bool  isGameOver   = false;
    public bool  isClear      = false;

    public bool HasCardInCollection(string cardName)
    {
        foreach (var c in deck)        if (c.cardName == cardName) return true;
        foreach (var c in usedCards)   if (c.cardName == cardName) return true;
        foreach (var c in carriedOver) if (c.cardName == cardName) return true;
        return false;
    }

    public int GetResource(ResourceType type) => type switch
    {
        ResourceType.HP    => hp,
        ResourceType.Food  => food,
        ResourceType.Water => water,
        ResourceType.Stone => stone,
        ResourceType.Wood  => wood,
        _                  => 0
    };

    public void AddResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.HP:    hp    += amount; break;
            case ResourceType.Food:  food  += amount; break;
            case ResourceType.Water: water += amount; break;
            case ResourceType.Stone: stone += amount; break;
            case ResourceType.Wood:  wood  += amount; break;
        }
    }

    public int GetStat(StatType type) => type switch
    {
        StatType.Strength     => strength,
        StatType.Agility      => agility,
        StatType.Intelligence => intelligence,
        _                     => 0
    };

    public void AddStat(StatType type, int amount)
    {
        switch (type)
        {
            case StatType.Strength:     strength     += amount; break;
            case StatType.Agility:      agility      += amount; break;
            case StatType.Intelligence: intelligence += amount; break;
        }
    }
}

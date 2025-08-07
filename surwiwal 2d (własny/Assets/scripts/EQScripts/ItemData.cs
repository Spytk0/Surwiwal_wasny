using UnityEngine;

// Typy przedmiotów
public enum ItemType
{
    Weapon,
    Armor,
    Medymedicines,
    Consumable,
    Uncategorized,
    Tools,
    Resource
}

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public ItemType type;
    public int width = 1;
    public int height = 1;
    public Sprite itemIcon;
    public int stackSize = 1;

    [Header("Type-Specific Stats")]
    [ShowIf("type", ItemType.Weapon)] public WeaponStats weaponStats;
    [ShowIf("type", ItemType.Armor)] public ArmorStats armorStats;
    [ShowIf("type", ItemType.Medymedicines)] public MedymedicinesStats medymedicinesStats;
    [ShowIf("type", ItemType.Consumable)] public ConsumableStats consumableStats;
    [ShowIf("type", ItemType.Uncategorized)] public UncategorizedStats uncategorizedStats;
    [ShowIf("type", ItemType.Tools)] public ToolsStats toolsStats;
    [ShowIf("type", ItemType.Resource)] public ResourceStats resourceStats;
}

// Struktury dla ró¿nych typów przedmiotów
[System.Serializable]
public struct WeaponStats
{
    public int damage;
    public float attackSpeed;
    public float range;
    public int durability;
}

[System.Serializable]
public struct ArmorStats
{
    public int defense;
    public int coldDefense;
    public int hotDefense;
    public int durability;
}

[System.Serializable]
public struct ConsumableStats
{
    public int healthRestore;
    public int foodRestore;
    public int waterRestore;
}

[System.Serializable]
public struct MedymedicinesStats
{
    public int healthRestore;
}
[System.Serializable]
public struct ToolsStats
{
    public int durability;
    public int mineSpeed;
}

    [System.Serializable]
public struct ResourceStats
{
    public ResourceType resourceType;
    public int gatherTime;
}

[System.Serializable]
public struct UncategorizedStats
{
    public ResourceType resourceType;
    
}

public enum ResourceType { Wood, Stone, Metal }

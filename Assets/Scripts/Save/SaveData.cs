using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public PlayerData player = new PlayerData();
    public List<QuestSaveData> quests = new List<QuestSaveData>();
    public List<RegionSaveData> regions = new List<RegionSaveData>();
}

[Serializable]
public class PlayerData
{
    public long gold;
    public int smallPotions;
    public int mediumPotions;
    public int largePotions;
    public List<ItemEntry> items = new List<ItemEntry>();        // µñ¼Å³Ê¸® ´ëÃ¼
    public List<EquipmentEntry> weapons = new List<EquipmentEntry>();
    public List<EquipmentEntry> armors = new List<EquipmentEntry>();
    public EquipmentEntry equippedWeapon;
    public EquipmentEntry equippedArmor;
}

[Serializable]
public class ItemEntry
{
    public string name;
    public int count;
}

[Serializable]
public class EquipmentEntry
{
    public string itemName;
    public int type;        // ShopUI.ItemType ¸¦ int·Î º¸°ü
    public int statBonus;
}

[Serializable]
public class QuestSaveData
{
    public string questId;
    public bool isAccepted;
    public bool isCompleted;
    public int currentProgress;
}

[Serializable]
public class RegionSaveData
{
    public string regionId;
    public List<StageSaveData> stages = new List<StageSaveData>();
}

[Serializable]
public class StageSaveData
{
    public string stageId;
    public bool isUnlocked;
}

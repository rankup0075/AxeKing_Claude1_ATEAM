using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    string path;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
        path = Path.Combine(Application.persistentDataPath, "save.json");
    }

    // ===== SAVE =====
    public void SaveGame()
    {
        var data = new SaveData();

        // Player / Inventory
        var gm = GameManager.Instance;
        var inv = FindFirstObjectByType<PlayerInventory>();

        if (gm != null) data.player.gold = gm.gold;
        if (inv != null)
        {
            data.player.smallPotions = inv.smallPotions;
            data.player.mediumPotions = inv.mediumPotions;
            data.player.largePotions = inv.largePotions;

            // items 딕셔너리 -> 리스트
            foreach (var kv in inv.items)
                data.player.items.Add(new ItemEntry { name = kv.Key, count = kv.Value });

            // 장비 스토리지 최소 정보만 저장
            foreach (var w in inv.weaponStorage)
                data.player.weapons.Add(new EquipmentEntry
                {
                    itemName = w.EquipmentitemName,
                    type = (int)w.Equipmenttype,
                    statBonus = w.EquipmentstatBonus
                });
            foreach (var a in inv.armorStorage)
                data.player.armors.Add(new EquipmentEntry
                {
                    itemName = a.EquipmentitemName,
                    type = (int)a.Equipmenttype,
                    statBonus = a.EquipmentstatBonus
                });

            // 장착중
            if (inv.currentWeapon != null)
                data.player.equippedWeapon = new EquipmentEntry
                {
                    itemName = inv.currentWeapon.EquipmentitemName,
                    type = (int)inv.currentWeapon.Equipmenttype,
                    statBonus = inv.currentWeapon.EquipmentstatBonus
                };
            if (inv.currentArmor != null)
                data.player.equippedArmor = new EquipmentEntry
                {
                    itemName = inv.currentArmor.EquipmentitemName,
                    type = (int)inv.currentArmor.Equipmenttype,
                    statBonus = inv.currentArmor.EquipmentstatBonus
                };
        }

        // Quests
        var qm = QuestManager.Instance;
        if (qm != null)
        {
            foreach (var q in qm.allQuests)
                data.quests.Add(new QuestSaveData
                {
                    questId = q.questId,
                    isAccepted = q.isAccepted,
                    isCompleted = q.isCompleted,
                    currentProgress = q.currentProgress
                });
        }

        // Regions / Stages
        var sm = StageManager.Instance;
        if (sm != null)
        {
            foreach (var r in sm.regions)
            {
                var rs = new RegionSaveData { regionId = r.regionId };
                foreach (var s in r.stages)
                    rs.stages.Add(new StageSaveData { stageId = s.stageId, isUnlocked = s.isUnlocked });
                data.regions.Add(rs);
            }
        }

        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"[Save] {path}");
    }

    // ===== LOAD =====
    public void LoadGame()
    {
        if (!File.Exists(path)) { Debug.Log("[Load] no file"); return; }

        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveData>(json);

        var gm = GameManager.Instance;
        var inv = FindFirstObjectByType<PlayerInventory>();
        var qm = QuestManager.Instance;
        var sm = StageManager.Instance;

        // Player / Inventory
        if (gm != null)
        {
            gm.gold = data.player.gold;
            UIManager.Instance?.UpdateGoldDisplay(gm.gold);
            UIManager.Instance?.UpdateHUDGold(gm.gold);
        }
        if (inv != null)
        {
            inv.smallPotions = data.player.smallPotions;
            inv.mediumPotions = data.player.mediumPotions;
            inv.largePotions = data.player.largePotions;

            // 리스트 -> 딕셔너리 복원
            inv.items = new Dictionary<string, int>();
            foreach (var e in data.player.items)
                inv.items[e.name] = e.count;

            // 스토리지 복원
            inv.weaponStorage = new List<ItemEquipment>();
            foreach (var e in data.player.weapons)
                inv.weaponStorage.Add(new ItemEquipment
                {
                    EquipmentitemName = e.itemName,
                    Equipmenttype = (ShopUI.ItemType)e.type,
                    EquipmentstatBonus = e.statBonus
                });

            inv.armorStorage = new List<ItemEquipment>();
            foreach (var e in data.player.armors)
                inv.armorStorage.Add(new ItemEquipment
                {
                    EquipmentitemName = e.itemName,
                    Equipmenttype = (ShopUI.ItemType)e.type,
                    EquipmentstatBonus = e.statBonus
                });

            // 장착 복원
            inv.currentWeapon = (data.player.equippedWeapon != null && !string.IsNullOrEmpty(data.player.equippedWeapon.itemName))
                ? new ItemEquipment
                {
                    EquipmentitemName = data.player.equippedWeapon.itemName,
                    Equipmenttype = (ShopUI.ItemType)data.player.equippedWeapon.type,
                    EquipmentstatBonus = data.player.equippedWeapon.statBonus
                } : null;

            inv.currentArmor = (data.player.equippedArmor != null && !string.IsNullOrEmpty(data.player.equippedArmor.itemName))
                ? new ItemEquipment
                {
                    EquipmentitemName = data.player.equippedArmor.itemName,
                    Equipmenttype = (ShopUI.ItemType)data.player.equippedArmor.type,
                    EquipmentstatBonus = data.player.equippedArmor.statBonus
                } : null;

            // HUD/기존 포션 UI 갱신
            UIManager.Instance?.UpdatePotionCount(inv.smallPotions, inv.mediumPotions, inv.largePotions);
            UIManager.Instance?.UpdateHUDPotions(inv.smallPotions, inv.mediumPotions, inv.largePotions);
        }

        // Quests
        if (qm != null)
        {
            foreach (var qs in data.quests)
            {
                var q = qm.allQuests.Find(x => x.questId == qs.questId);
                if (q == null) continue;
                q.isAccepted = qs.isAccepted;
                q.isCompleted = qs.isCompleted;
                q.currentProgress = qs.currentProgress;
            }
            QuestBoardUI.Instance?.RefreshUI();
        }

        // Regions / Stages
        if (sm != null)
        {
            foreach (var rs in data.regions)
            {
                var r = sm.regions.Find(x => x.regionId == rs.regionId);
                if (r == null) continue;
                foreach (var ss in rs.stages)
                {
                    var s = r.stages.Find(x => x.stageId == ss.stageId);
                    if (s != null) s.isUnlocked = ss.isUnlocked;
                }
            }
        }

        Debug.Log("[Load] done");
    }
}

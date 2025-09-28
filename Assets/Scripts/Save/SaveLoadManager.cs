using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        var gm = GameManager.Instance;
        var inv = FindFirstObjectByType<PlayerInventory>();
        var hp = FindFirstObjectByType<PlayerHealth>();

        // 골드
        if (gm != null) data.player.gold = gm.gold;

        // 포션 + 인벤토리
        if (inv != null)
        {
            data.player.smallPotions = inv.smallPotions;
            data.player.mediumPotions = inv.mediumPotions;
            data.player.largePotions = inv.largePotions;

            foreach (var kv in inv.items)
                data.player.items.Add(new ItemEntry { name = kv.Key, count = kv.Value });

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

        // 현재 체력
        if (hp != null)
            data.player.currentHealth = hp.currentHealth;

        // 퀘스트
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

        // 지역/스테이지
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

        // 파일 기록
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
        var hp = FindFirstObjectByType<PlayerHealth>();
        var qm = QuestManager.Instance;
        var sm = StageManager.Instance;

        // 골드
        if (gm != null)
        {
            gm.gold = data.player.gold;
            UIManager.Instance?.UpdateGoldDisplay(gm.gold);
            UIManager.Instance?.UpdateHUDGold(gm.gold);
        }

        // 인벤토리
        if (inv != null)
        {
            inv.smallPotions = data.player.smallPotions;
            inv.mediumPotions = data.player.mediumPotions;
            inv.largePotions = data.player.largePotions;

            inv.items = new Dictionary<string, int>();
            foreach (var e in data.player.items)
                inv.items[e.name] = e.count;

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

            UIManager.Instance?.UpdatePotionCount(inv.smallPotions, inv.mediumPotions, inv.largePotions);
            UIManager.Instance?.UpdateHUDPotions(inv.smallPotions, inv.mediumPotions, inv.largePotions);
        }

        // 체력 복원
        if (hp != null)
        {
            hp.currentHealth = Mathf.Clamp(data.player.currentHealth, 0, hp.maxHealth);
            UIManager.Instance?.UpdateHealthBar(hp.currentHealth, hp.maxHealth);
            UIManager.Instance?.UpdateHUDHealth(hp.currentHealth, hp.maxHealth);
        }

        // 퀘스트
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

        // 지역/스테이지
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

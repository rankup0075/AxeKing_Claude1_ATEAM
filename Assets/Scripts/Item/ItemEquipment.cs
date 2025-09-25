//using Unity.VisualScripting;
//using UnityEngine;

//[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
//public class ItemEquipment 
//{
//    [Header("Equipment Stats")]
//    public EquipmentSlot equipmentSlot = EquipmentSlot.Weapon;
//    public int attackPower = 0;
//    public int healthBonus = 0;

//    [Header("Requirements")]
//    public int levelRequired = 1;




//    public string EquipmentitemName;
//    public ShopUI.ItemType Equipmenttype;
//    public int EquipmentstatBonus;
//    public Sprite icon;

//    public enum EquipmentSlot
//    {
//        Weapon,      // 무기
//        Armor       // 방어구
//    }

//    void Awake()
//    {


//    }

//    // 장비 사용 (착용)
//    //public override bool UseItem(GameObject user)
//    //{
//    //    PlayerInventory inventory = user.GetComponent<PlayerInventory>();
//    //    if (inventory != null)
//    //    {
//    //        return inventory.EquipItem(this);
//    //    }
//    //    return false;
//    //}

//    // 장비 정보 텍스트 (오버라이드)

//    //string GetSlotString()
//    //{
//    //    switch (equipmentSlot)
//    //    {
//    //        case EquipmentSlot.Weapon: return "무기";
//    //        case EquipmentSlot.Armor: return "방어구";
//    //        default: return "알 수 없음";
//    //    }
//    //}

//    //// 장비 파손 처리
//    //void OnEquipmentBroken()
//    //{
//    //    Debug.Log($"{itemName}이(가) 파손되었습니다!");
//    //    // 파손 시 효과 감소나 다른 처리 로직
//    //}


//    // 장비 능력치 적용
//    public void ApplyStats(PlayerController player, PlayerHealth health)
//    {
//        if (Equipmenttype == ShopUI.ItemType.Weapon)
//        {
//            player.attackDamage += EquipmentstatBonus;
//        }
//        else if (Equipmenttype == ShopUI.ItemType.Armor)
//        {
//            health.IncreaseMaxHealth(EquipmentstatBonus);
//        }
//    }

//    // 해제 시 능력치 제거
//    public void RemoveStats(PlayerController player, PlayerHealth health)
//    {
//        if (Equipmenttype == ShopUI.ItemType.Weapon)
//        {
//            player.attackDamage -= EquipmentstatBonus;
//        }
//        else if (Equipmenttype == ShopUI.ItemType.Armor)
//        {
//            health.IncreaseMaxHealth(-EquipmentstatBonus);
//        }
//    }

//    //void ApplySpecialEffects(PlayerController player)
//    //{
//    //    if (specialEffects == null) return;

//    //    foreach (var effect in specialEffects)
//    //    {
//    //        // 특수 효과 적용 로직 (추후 확장)
//    //        switch (effect.effectType)
//    //        {
//    //            case EquipmentEffect.EffectType.AttackSpeedBonus:
//    //                // 공격속도 증가 로직
//    //                break;
//    //            case EquipmentEffect.EffectType.MovementSpeedBonus:
//    //                // 이동속도 증가 로직
//    //                break;
//    //                // 다른 효과들도 추가 가능
//    //        }
//    //    }
//    //}

//    //void RemoveSpecialEffects(PlayerController player)
//    //{
//    //    // ApplySpecialEffects의 역순으로 효과 제거
//    //}
//}

using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class ItemEquipment
{
    public string EquipmentitemName;
    public ShopUI.ItemType Equipmenttype;
    public int EquipmentstatBonus;
    public Sprite icon;

    public enum EquipmentSlot { Weapon, Armor }

    public void ApplyStats(PlayerController player, PlayerHealth health)
    {
        if (Equipmenttype == ShopUI.ItemType.Weapon)
        {
            player.attackDamage += EquipmentstatBonus;
        }
        else if (Equipmenttype == ShopUI.ItemType.Armor)
        {
            health.IncreaseMaxHealth(EquipmentstatBonus);
        }
    }

    public void RemoveStats(PlayerController player, PlayerHealth health)
    {
        if (Equipmenttype == ShopUI.ItemType.Weapon)
        {
            player.attackDamage -= EquipmentstatBonus;
        }
        else if (Equipmenttype == ShopUI.ItemType.Armor)
        {
            health.IncreaseMaxHealth(-EquipmentstatBonus);
        }
    }
}


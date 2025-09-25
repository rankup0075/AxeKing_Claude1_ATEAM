using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Transform itemContainer;
    public GameObject itemPrefab;
    public TextMeshProUGUI goldText;

    private ShopItem[] shopItems;

    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int price;
        public string description;
        public ItemType type;
        public int statBonus;
    }

    public enum ItemType { Weapon, Armor }

    void Start()
    {
        InitializeShop();
    }

    void InitializeShop()
    {
        shopItems = new ShopItem[]
        {
            // 무기
            new ShopItem { itemName = "일반 도끼", price = 1000, description = "공격력 +1", type = ItemType.Weapon, statBonus = 1 },
            new ShopItem { itemName = "살짝 날카로운 도끼", price = 3000, description = "공격력 +2", type = ItemType.Weapon, statBonus = 2 },
            // ... 더 많은 아이템들
            
            // 방어구  
            new ShopItem { itemName = "천갑옷", price = 1000, description = "체력 +30", type = ItemType.Armor, statBonus = 30 },
            new ShopItem { itemName = "가죽 갑옷", price = 3000, description = "체력 +50", type = ItemType.Armor, statBonus = 50 },
            // ... 더 많은 방어구들
        };

        CreateShopItems();
    }

    void CreateShopItems()
    {
        foreach (var item in shopItems)
        {
            GameObject itemUI = Instantiate(itemPrefab, itemContainer);

            // UI 설정
            TextMeshProUGUI nameText = itemUI.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = itemUI.transform.Find("Price").GetComponent<TextMeshProUGUI>();
            Button buyButton = itemUI.transform.Find("BuyButton").GetComponent<Button>();

            nameText.text = item.itemName;
            priceText.text = $"{item.price}원";

            // 구매 버튼 이벤트
            buyButton.onClick.AddListener(() => BuyItem(item));
        }
    }

    public void BuyItem(ShopItem item)
    {
        if (GameManager.Instance.Gold >= item.price)
        {
            GameManager.Instance.SpendGold(item.price);

            // 플레이어 인벤토리에 아이템 추가
            PlayerInventory inventory = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
            inventory.AddEquipment(item.itemName, item.type, item.statBonus);

            UpdateGoldDisplay();
        }
    }

    void UpdateGoldDisplay()
    {
        goldText.text = $"골드: {GameManager.Instance.Gold}원";
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        UpdateGoldDisplay();
        Time.timeScale = 0f; // 게임 일시정지
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        Time.timeScale = 1f; // 게임 재시작
    }
}
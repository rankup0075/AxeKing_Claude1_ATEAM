using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public string targetScene;
    public string portalID;                  // [NEW] ��Ż ���� ID (Town�� ���������� ������ �ǹ̡��� �����ּ���)
    public PortalType portalType;
    public bool isUnlocked = true;

    [Header("Shop Portal Settings")]
    public PotionShopUI potionShopUI;
    public EquipmentShopUI equipmentShopUI;
    public WarehouseUI warehouseUI;

    // �߰�: ���� ����Ʈ
    [Header("Spawn Point")]
    public Transform spawnPoint;
    
    public enum PortalType
    {
        SceneTransition,
        StageSelect,
        Shop,
        QuestBoard,
        PotionShop,
        EquipmentShop,
        WareHouseChest
    }

    private bool playerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionUI(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowInteractionUI(false);
        }
    }

    public void Interact()
    {
        if (!playerInRange || !isUnlocked) return;

        // [NEW] � Ÿ���̵� ���� ��Ż�� ���������� ����ߴ١��� ����� ���� ����
        if (GameManager.Instance != null)
            GameManager.Instance.SetLastPortalID(portalID);

        switch (portalType)
        {
            case PortalType.SceneTransition:
                TransitionToScene();
                break;

            case PortalType.StageSelect:
                UIManager.Instance?.OpenStageSelectUI();
                break;

            case PortalType.QuestBoard:
                UIManager.Instance?.OpenQuestBoardUI();
                break;

            case PortalType.PotionShop:
                if (potionShopUI != null) potionShopUI.OpenShop();
                else Debug.LogWarning("[Portal] PotionShopUI �̿���");
                break;

            case PortalType.EquipmentShop:
                if (equipmentShopUI != null) equipmentShopUI.OpenShop();
                else Debug.LogWarning("[Portal] EquipmentShopUI �̿���");
                break;

            case PortalType.WareHouseChest:
                if (warehouseUI != null) warehouseUI.OpenWarehouse();
                else Debug.LogWarning("[Portal] warehouseUI �̿���");
                break;

            //case PortalType.Shop:
            //    UIManager.Instance?.OpenShopUI();
            //    break;
        }
    }

    void TransitionToScene()
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[Portal] targetScene�� �������");
            return;
        }

        // [CHANGED] �� ��ȯ ���� ����(�� Interact������ �̹� ���������� �����ϰ� �� �� �� ����)
        GameManager.Instance?.SetLastPortalID(portalID);

        // ���Ͻø� ����
        GameManager.Instance?.SavePlayerData();

        // ��ȯ
        Time.timeScale = 1f; // Ȥ�� �����ִٸ� ����
        SceneManager.LoadScene(targetScene);
    }

    void ShowInteractionUI(bool show)
    {
        string promptText = GetInteractionPrompt();
        UIManager.Instance?.ShowInteractionPrompt(show, promptText);
    }

    string GetInteractionPrompt()
    {
        switch (portalType)
        {
            case PortalType.PotionShop: return "�� Ű�� ���� ���� ���� ����";
            case PortalType.EquipmentShop: return "�� Ű�� ���� ��� ���� ����";
            case PortalType.QuestBoard: return "�� Ű�� ���� ����Ʈ Ȯ��";
            case PortalType.StageSelect: return "�� Ű�� ���� �������� ����";
            default: return "�� Ű�� ���� ��ȣ�ۿ�";
        }
    }

}

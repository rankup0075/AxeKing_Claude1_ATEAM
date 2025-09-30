using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public string targetScene;
    public string portalID;                  // [NEW] 포탈 고유 ID (Town과 상점씬에서 “같은 의미”로 맞춰주세요)
    public PortalType portalType;
    public bool isUnlocked = true;
    public PortalDirection portalDirection = PortalDirection.Forward; // [NEW]

    [Header("Shop Portal Settings")]
    public PotionShopUI potionShopUI;
    public EquipmentShopUI equipmentShopUI;
    public WarehouseUI warehouseUI;

    [Header("Stage Clear Options")]
    [SerializeField] private bool completeStageOnUse = false;  // 마지막 라운드에서만 true
    [SerializeField] private string stageIdToComplete = "";    // 스테이지1의 마지막 라운드 포탈이라면 "1-1" 입력.

    [Header("Portal Visuals")]
    [SerializeField] private ParticleSystem portalParticles;
    [SerializeField] private Light portalLight;


    [Header("Spawn Point")]
    public Transform spawnPoint;   // 새 씬에서 플레이어 위치 지정용


    public enum PortalDirection
    {
        Forward,
        Backward
    }

    public enum PortalType
    {
        SceneTransition,
        StageSelect,
        Shop,
        QuestBoard,
        PotionShop,
        EquipmentShop,
        WareHouseChest,
        RoundTransition,
        StageClear
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

    public void SetActiveState(bool active)
    {
        isUnlocked = active; // 상호작용 여부

        if (portalParticles != null)
        {
            if (active && !portalParticles.isPlaying) portalParticles.Play();
            else if (!active && portalParticles.isPlaying) portalParticles.Stop();
        }

        if (portalLight != null)
        {
            portalLight.enabled = active;
        }
    }

    public void Interact()
    {
        if (!playerInRange || !isUnlocked) return;

        // [NEW] 어떤 타입이든 “이 포탈을 마지막으로 사용했다”는 사실을 먼저 저장
        if (GameManager.Instance != null)
            GameManager.Instance.SetLastPortalID(portalID);

        switch (portalType)
        {
            case PortalType.SceneTransition:
                TransitionToScene();
                break;

            case PortalType.StageSelect:
                // [추가] Town이든 Stage든 구분 없이 포탈 위치 저장
                if (GameManager.Instance != null)
                    GameManager.Instance.SetLastPortalID(portalID);

                UIManager.Instance?.OpenStageSelectPanel();
                break;

            case PortalType.QuestBoard:
                UIManager.Instance?.OpenQuestBoardUI();
                break;

            case PortalType.PotionShop:
                if (potionShopUI != null) potionShopUI.OpenShop();
                else Debug.LogWarning("[Portal] PotionShopUI 미연결");
                break;

            case PortalType.EquipmentShop:
                if (equipmentShopUI != null) equipmentShopUI.OpenShop();
                else Debug.LogWarning("[Portal] EquipmentShopUI 미연결");
                break;

            case PortalType.WareHouseChest:
                if (warehouseUI != null) warehouseUI.OpenWarehouse();
                else Debug.LogWarning("[Portal] warehouseUI 미연결");
                break;

            case PortalType.RoundTransition:
                SceneManager.LoadScene(targetScene); // 라운드 간 이동
                break;

            case PortalType.StageClear:
                // 보스 라운드 종료 포탈
                if (completeStageOnUse && StageManager.Instance != null && !string.IsNullOrEmpty(stageIdToComplete))
                {
                    StageManager.Instance.CompleteStage(stageIdToComplete);
                    GameManager.Instance?.SavePlayerData();
                }

                // 스테이지 선택창 열기
                UIManager.Instance?.OpenStageSelectPanel();
                break;
        }
    }

    void TransitionToScene()
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[Portal] targetScene이 비어있음");
            return;
        }

        // [CHANGED] 씬 전환 직전 저장(위 Interact에서도 이미 저장하지만 안전하게 한 번 더 보장)
        GameManager.Instance?.SetLastPortalID(portalID);

        // 원하시면 저장
        GameManager.Instance?.SavePlayerData();

        // 전환
        Time.timeScale = 1f; // 혹시 멈춰있다면 복구
        SceneManager.LoadScene(targetScene);

        if (completeStageOnUse && StageManager.Instance != null && !string.IsNullOrEmpty(stageIdToComplete))
        {
            StageManager.Instance.CompleteStage(stageIdToComplete);
        }

        GameManager.Instance?.SavePlayerData();
        Time.timeScale = 1f;
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
            case PortalType.PotionShop: return "↑ 키를 눌러 포션 상점 열기";
            case PortalType.EquipmentShop: return "↑ 키를 눌러 장비 상점 열기";
            case PortalType.QuestBoard: return "↑ 키를 눌러 퀘스트 확인";
            case PortalType.StageSelect: return "↑ 키를 눌러 스테이지 선택";
            default: return "↑ 키를 눌러 상호작용";
        }
    }

}

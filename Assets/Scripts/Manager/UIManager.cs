using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Health UI")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    [Header("Boss Health UI")]
    public GameObject bossHealthPanel;
    public Slider bossHealthBar;
    public TextMeshProUGUI bossNameText;

    [Header("Potion UI")]
    public TextMeshProUGUI smallPotionCount;
    public TextMeshProUGUI mediumPotionCount;
    public TextMeshProUGUI largePotionCount;

    [Header("Gold UI")]
    public TextMeshProUGUI goldText;

    [Header("Interaction")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI interactionText;

    [Header("Panels")]
    public GameObject potionShopPanel;
    public GameObject equipmentShopPanel;
    public GameObject questBoardPanel;
    public GameObject inventoryPanel;
    public GameObject stageSelectPanel;
    public GameObject gameOverPanel;
    public GameObject stageCompletePanel;
    public GameObject settingsPanel;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC 감지됨");
            // 포션 상점 먼저 닫기
            if (potionShopPanel != null && potionShopPanel.activeSelf)
            {
                ClosePotionShopUI();
                return;
            }

            // 장비 상점 닫기
            if (equipmentShopPanel != null && equipmentShopPanel.activeSelf)
            {
                CloseEquipmentShopUI();
                return;
            }

            // 퀘스트보드 닫기
            if (questBoardPanel != null && questBoardPanel.activeSelf)
            {
                CloseQuestBoardUI();
                return;
            }

            // 인벤토리 닫기
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                CloseWareHouseUI();
                return;
            }

            // 스테이지 선택 닫기
            if (stageSelectPanel != null && stageSelectPanel.activeSelf)
            {
                CloseStageSelectUI();
                return;
            }

            // 아무 UI도 열려있지 않을 때만 설정창 토글
            if (settingsPanel != null)
            {
                var cg = settingsPanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    if (cg.alpha > 0.5f)
                        CloseSettings();
                    else
                        OpenSettings();
                }
            }
        }

    }

    //카메라 초기화를 위한 메소드
    private void ResetCameraTarget()
    {
        var cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null)
            cam.ResetTarget(); // CameraFollow가 플레이어와 기본 오프셋으로 되돌림
    }

    //ESC로 UI를 꺼도 canMove=true로 돌리기 위한 메소드
    private void RestorePlayerControl()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.canMove = true;
        }
    }

    // ======================
    // 설정창
    // ======================
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            var cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            Time.timeScale = 0f;
            isPaused = true;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            var cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    // ======================
    // Shop UI 제어
    // ======================
    public void OpenPotionShopUI()
    {
        if (potionShopPanel != null)
        {
            potionShopPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ClosePotionShopUI()
    {
        if (potionShopPanel != null)
        {
            potionShopPanel.SetActive(false);
            Time.timeScale = 1f;
        }
        // 카메라 리셋
        ResetCameraTarget();
        //ESC로 꺼도 canMove가 true로 돌아올 수 있도록
        RestorePlayerControl();
    }

    public void OpenEquipmentShopUI()
    {
        if (equipmentShopPanel != null)
        {
            equipmentShopPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseEquipmentShopUI()
    {
        if (equipmentShopPanel != null)
        {
            equipmentShopPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        ResetCameraTarget();
        //ESC로 꺼도 canMove가 true로 돌아올 수 있도록
        RestorePlayerControl();
    }
    public void OpenWareHouseUI()
    {
        if (inventoryPanel != null)
        {
            equipmentShopPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseWareHouseUI()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        ResetCameraTarget();
        //ESC로 꺼도 canMove가 true로 돌아올 수 있도록
        RestorePlayerControl();
    }


    // Portal.cs에서 참조하는 용도로 남겨둠
    //public void OpenShopUI()
    //{
    //    OpenPotionShopUI();
    //}

    // ======================
    // Quest / Stage
    // ======================
    public void OpenQuestBoardUI()
    {
        if (questBoardPanel != null)
        {
            questBoardPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseQuestBoardUI()
    {
        if (questBoardPanel != null)
        {
            questBoardPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        ResetCameraTarget();
        //ESC로 꺼도 canMove가 true로 돌아올 수 있도록
        RestorePlayerControl();
    }

    public void OpenStageSelectUI()
    {
        if (stageSelectPanel != null)
        {
            stageSelectPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseStageSelectUI()
    {
        if (stageSelectPanel != null)
        {
            stageSelectPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        ResetCameraTarget();
        //ESC로 꺼도 canMove가 true로 돌아올 수 있도록
        RestorePlayerControl();
    }

    // ======================
    // Health / Gold / Potion UI
    // ======================
    public void UpdateHealthBar(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.value = (float)current / max;
            if (healthText != null)
                healthText.text = $"{current} / {max}";
        }
    }

    public void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }

    public void UpdatePotionCount(int small, int medium, int large)
    {
        if (smallPotionCount != null) smallPotionCount.text = small.ToString();
        if (mediumPotionCount != null) mediumPotionCount.text = medium.ToString();
        if (largePotionCount != null) largePotionCount.text = large.ToString();
    }

    // ======================
    // Interaction Prompt
    // ======================
    public void ShowInteractionPrompt(bool show, string text = "")
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
            if (show && interactionText != null)
                interactionText.text = text;
        }
    }

    // ======================
    // Boss Health
    // ======================

    // 스테이지 구현 후 추가
    //public void ShowBossHealthBar(EnemyHealth bossHealth)
    //{
    //    if (bossHealthPanel != null)
    //    {
    //        bossHealthPanel.SetActive(true);
    //        bossNameText.text = bossHealth.gameObject.name;

    //        // 보스 체력 업데이트 이벤트 연결
    //        bossHealth.OnHealthChanged.AddListener(UpdateBossHealthBar);
    //    }
    //}

    public void UpdateBossHealthBar(int current, int max)
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.value = (float)current / max;
        }

        if (current <= 0)
        {
            bossHealthPanel.SetActive(false);
        }
    }

    // ======================
    // Game Over / Stage Complete
    // ======================
    public void ShowGameOverUI()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowStageCompleteUI()
    {
        if (stageCompletePanel != null)
        {
            stageCompletePanel.SetActive(true);
            Invoke(nameof(HideStageCompleteUI), 2f);
        }
    }

    void HideStageCompleteUI()
    {
        if (stageCompletePanel != null)
            stageCompletePanel.SetActive(false);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬이 로드되면 패널 자동 재연결
        ReassignPanels();
    }

    void ReassignPanels()
    {
        potionShopPanel = GameObject.Find("PotionShopPanel");
        if(potionShopPanel == null)
        {
            Debug.Log("PotionShopPanel을 찾지 못했습니다");
        }
        else
        {
            Debug.Log("PotionShopPanel을 찾았습니다.");
        }

        equipmentShopPanel = GameObject.Find("EquipmentShopPanel");
        if (equipmentShopPanel == null)
        {
            Debug.Log("equipmentShopPanel 찾지 못했습니다");
        }
        else
        {
            Debug.Log("equipmentShopPanel 찾았습니다.");
        }

        questBoardPanel = GameObject.Find("QuestBoardPanel");

        inventoryPanel = GameObject.Find("InventoryPanel");
        if (inventoryPanel == null)
        {
            Debug.Log("InventoryPanel 찾지 못했습니다");
        }
        else
        {
            Debug.Log("InventoryPanel 찾았습니다.");
        }

        stageSelectPanel = GameObject.Find("StageSelectPanel");
        gameOverPanel = GameObject.Find("GameOverPanel");
        stageCompletePanel = GameObject.Find("StageCompletePanel");

        settingsPanel = GameObject.Find("SettingsPanel");

        if (settingsPanel != null)
        {
            Debug.Log("SettingsPanel을 찾았습니다.");
            var cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }
        

        interactionPrompt = GameObject.Find("InteractionPrompt");
        if (interactionPrompt != null)
            interactionText = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();

        GameObject goldObj = GameObject.Find("GoldText");
        if (goldObj != null)
            goldText = goldObj.GetComponent<TextMeshProUGUI>();

        // 필요한 컴포넌트들 계속 등록
    }
}

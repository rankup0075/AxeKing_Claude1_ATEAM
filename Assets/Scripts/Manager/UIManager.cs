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

    [Header("PlayerHUD")]
    public GameObject playerHUD; // PlayerHUD 패널 (프리팹으로 생성되거나 씬에 존재)
    public TextMeshProUGUI hudGoldText;
    public Slider hudHealthBar;
    public TextMeshProUGUI hudHealthText;
    public TextMeshProUGUI hudSmallPotionText;
    public TextMeshProUGUI hudMediumPotionText;
    public TextMeshProUGUI hudLargePotionText;


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

    private bool isPaused = false;

    [Header("Settings (In-Game Only)")]
    public GameObject settingsPanel_InGame; // 인스펙터에 MainMenu의 SettingsPanel 연결
    public Button closeButton;
    public Button mainMenuButton;
    public Button quitButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (settingsPanel_InGame != null)
                DontDestroyOnLoad(settingsPanel_InGame); // [NEW] SettingsPanel도 함께 유지

        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("ESC 감지됨");

            if (SceneManager.GetActiveScene().name == "MainMenu")
                return;

            if (potionShopPanel != null && potionShopPanel.activeSelf)
            {
                ClosePotionShopUI();
                return;
            }
            if (equipmentShopPanel != null && equipmentShopPanel.activeSelf)
            {
                CloseEquipmentShopUI();
                return;
            }
            if (questBoardPanel != null && questBoardPanel.activeSelf)
            {
                CloseQuestBoardUI();
                return;
            }
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                CloseWareHouseUI();
                return;
            }
            if (stageSelectPanel != null && stageSelectPanel.activeSelf)
            {
                CloseStageSelectUI();
                return;
            }

            if (settingsPanel_InGame != null)
            {
                var cg = settingsPanel_InGame.GetComponent<CanvasGroup>();
                if (cg == null) return;

                if (cg.alpha > 0.5f) CloseSettings();
                else OpenSettings();
            }

        }
    }

    //카메라 초기화를 위한 메소드
    private void ResetCameraTarget()
    {
        var cam = Camera.main ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cam != null)
            cam.ResetTarget();
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
        if (settingsPanel_InGame == null) return;

        var cg = settingsPanel_InGame.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        Time.timeScale = 0f;
        isPaused = true;

        // 플레이어 제어 정지
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.canControl = false; // 입력 정지
            var anim = player.GetComponent<Animator>();
            if (anim != null) anim.speed = 0f; // 애니메이션 정지
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel_InGame == null) return;

        var cg = settingsPanel_InGame.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
        isPaused = false;

        // 플레이어 제어 복구
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.canControl = true;
            var anim = player.GetComponent<Animator>();
            if (anim != null) anim.speed = 1f;
        }
    }
    public void ReturnToMainMenu()
    {
        // 자동 저장
        if (GameManager.Instance != null)
            GameManager.Instance.SavePlayerData();

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Destroy(player.transform.root.gameObject);
            PlayerController.Instance = null; // 싱글톤 초기화
        }

        // 시간 재개 후 메인메뉴 로드
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        // 자동 저장
        if (GameManager.Instance != null)
            GameManager.Instance.SavePlayerData();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
        ResetCameraTarget();
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
        RestorePlayerControl();
    }

    public void OpenWareHouseUI()
    {
        if (inventoryPanel != null)
        {
            // BUGFIX: 장비상점이 아니라 인벤토리 패널을 연다.
            inventoryPanel.SetActive(true); // [FIX]
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
        RestorePlayerControl();
    }

    // ======================
    // Quest / Stage
    // ======================
    public void OpenQuestBoardUI()
    {
        if (questBoardPanel != null)
        {
            questBoardPanel.SetActive(true);
            var qb = questBoardPanel.GetComponent<QuestBoardUI>();
            if (qb != null) qb.RefreshUI();
            Time.timeScale = 0f;
            if (playerHUD != null)
                playerHUD.SetActive(false); // HUD 끄기

            Debug.Log("[UIManager] QuestBoard 열림");
        }
        else
        {
            Debug.LogError("[UIManager] QuestBoardPanel이 null임");
        }
    }

    public void CloseQuestBoardUI()
    {
        if (questBoardPanel != null)
        {
            questBoardPanel.SetActive(false);
            Time.timeScale = 1f;

            if (playerHUD != null)
                playerHUD.SetActive(true); // HUD 다시 켜기
        }
        ResetCameraTarget();
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

    public void UpdateGoldDisplay(long gold)
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold:N0}";
    }

    public void UpdatePotionCount(int small, int medium, int large)
    {
        if (smallPotionCount != null) smallPotionCount.text = small.ToString();
        if (mediumPotionCount != null) mediumPotionCount.text = medium.ToString();
        if (largePotionCount != null) largePotionCount.text = large.ToString();
    }
    // ======================
    // [NEW] PlayerHUD 전용 UI
    // ======================
    public void InitPlayerHUD()
    {
        if (playerHUD == null) return;
        UpdateHUDGold(GameManager.Instance?.Gold ?? 0);
        UpdateHUDHealth(100, 100);
        UpdateHUDPotions(0, 0, 0);
    }

    public void UpdateHUDGold(long gold)
    {
        if (hudGoldText != null)
            hudGoldText.text = $"Gold: {gold:N0}G";
    }

    public void UpdateHUDHealth(int current, int max)
    {
        if (hudHealthBar != null)
            hudHealthBar.value = max > 0 ? (float)current / max : 0f;

        if (hudHealthText != null)
            hudHealthText.text = $"{current}/{max}";
    }

    public void UpdateHUDPotions(int small, int medium, int large)
    {
        if (hudSmallPotionText != null)
            hudSmallPotionText.text = $"{small:N0}개";
        if (hudMediumPotionText != null)
            hudMediumPotionText.text = $"{medium:N0}개";
        if (hudLargePotionText != null)
            hudLargePotionText.text = $"{large:N0}개";
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
        ReassignPanels();

        HookupSettingsButtons();

        // [NEW] 매 씬 로드시 SettingsPanel을 해당 씬의 Canvas 밑으로 이동
        MoveSettingsPanelToActiveCanvas();

        // [NEW] 씬 로드 시 HUD도 자동 재연결
        ReassignPlayerHUD(scene);
    }

    void ReassignPanels()
    {
        // 이미 연결되어 있으면 유지, 없을 때만 Find
        if (potionShopPanel == null) potionShopPanel = GameObject.Find("PotionShopPanel");
        if (equipmentShopPanel == null) equipmentShopPanel = GameObject.Find("EquipmentShopPanel");
        if (questBoardPanel == null) questBoardPanel = GameObject.Find("QuestBoardPanel");
        if (inventoryPanel == null) inventoryPanel = GameObject.Find("InventoryPanel");
        if (stageSelectPanel == null) stageSelectPanel = GameObject.Find("StageSelectPanel");
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");
        if (stageCompletePanel == null) stageCompletePanel = GameObject.Find("StageCompletePanel");

        if (settingsPanel_InGame == null) settingsPanel_InGame = GameObject.Find("SettingsPanel_InGame");
        

        // SettingsPanel 기본 비활성 상태(CanvasGroup 기반)로 초기화
        if (settingsPanel_InGame != null)
        {
            var cg = settingsPanel_InGame.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanel_InGame.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        if (interactionPrompt == null) interactionPrompt = GameObject.Find("InteractionPrompt");
        if (interactionPrompt != null && interactionText == null)
            interactionText = interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        if (goldText == null)
        {
            GameObject goldObj = GameObject.Find("GoldText");
            if (goldObj != null) goldText = goldObj.GetComponent<TMPro.TextMeshProUGUI>();
        }

        // [추가] QuestBoardPanel 처음 잡을 때는 꺼두기
        if (questBoardPanel != null && questBoardPanel.activeSelf)
        {
            questBoardPanel.SetActive(false);
        }
    }


    // [NEW] 현재 씬의 Canvas 밑으로 SettingsPanel을 재부착 (해상도 스케일 일치)
    public void MoveSettingsPanelToActiveCanvas()
    {
        if (settingsPanel_InGame == null)
        {
            var obj = GameObject.Find("SettingsPanel_InGame"); // 씬에 있는 인게임 세팅 패널 이름
            if (obj != null)
            {
                settingsPanel_InGame = obj;
                Debug.Log("[UIManager] SettingsPanel_InGame 자동 연결됨");
            }
        }


        if (settingsPanel_InGame == null)
        {
            Debug.LogWarning("[UIManager] SettingsPanel_InGame 없음");
            return;
        }

        // 씬 안의 Canvas들 중에서 "이름이 그냥 Canvas인 것"만 선택
        var targetCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .FirstOrDefault(c => c.name == "Canvas"); // "일반 Canvas"만 선택

        if (targetCanvas == null)
        {
            Debug.LogWarning("[UIManager] 활성 Canvas 없음 → SettingsPanel 이동 생략");
        }
        else
        {
            settingsPanel_InGame.transform.SetParent(targetCanvas.transform, false);

            var rt = settingsPanel_InGame.transform as RectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            Debug.Log($"[UIManager] SettingsPanel_InGame → '{targetCanvas.name}' 로 이동 완료");
        }

        // CanvasGroup 초기화
        var cg = settingsPanel_InGame.GetComponent<CanvasGroup>();
        if (cg == null) cg = settingsPanel_InGame.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

    }

    void HookupSettingsButtons()
    {
        if (settingsPanel_InGame != null) return;

        // Resources에서 프리팹 로드
        GameObject prefab = Resources.Load<GameObject>("UI/SettingsPanel_InGame");
        if (prefab == null)
        {
            Debug.LogWarning("[UIManager] Resources/UI/SettingsPanel_InGame.prefab 없음");
            return;
        }

        // 인스턴스 생성
        var panelInstance = Instantiate(prefab);
        panelInstance.name = "SettingsPanel_InGame";
        settingsPanel_InGame = panelInstance;

        // 현재 씬에서 가장 위 Canvas 찾기
        var targetCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .Where(c => c.isActiveAndEnabled && c.gameObject.activeInHierarchy)
            .OrderByDescending(c => c.sortingOrder)
            .FirstOrDefault();

        if (targetCanvas != null)
        {
            settingsPanel_InGame.transform.SetParent(targetCanvas.transform, false);
            Debug.Log($"[UIManager] SettingsPanel_InGame '{targetCanvas.name}' 밑으로 이동");
        }
        else
        {
            Debug.LogWarning("[UIManager] 활성 Canvas 없음 → SettingsPanel_InGame 고아 상태");
        }

        closeButton = settingsPanel_InGame.transform
        .Find("ContentContainer/CloseButton")?.GetComponent<Button>();

        mainMenuButton = settingsPanel_InGame.transform
            .Find("ContentContainer/ButtonContainer/ToMainMenuButton")?.GetComponent<Button>();

        quitButton = settingsPanel_InGame.transform
            .Find("ContentContainer/ButtonContainer/QuitGameButton")?.GetComponent<Button>();

        // 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseSettings);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        Debug.Log("[UIManager] SettingsPanel 버튼 연결 완료");


        // CanvasGroup 기본값 초기화
        var cg = settingsPanel_InGame.GetComponent<CanvasGroup>();
        if (cg == null) cg = settingsPanel_InGame.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;


    }


    // [NEW] PlayerHUD 재연결
    void ReassignPlayerHUD(Scene scene)
    {
        // MainMenu에서는 HUD 필요 없음
        if (scene.name == "MainMenu")
        {
            playerHUD = null;
            return;
        }

        if (playerHUD == null)
        {
            // 씬에 없으면 프리팹 인스턴스화
            var prefab = Resources.Load<GameObject>("UI/PlayerHUDCanvas");
            if (prefab != null)
            {
                var instance = Instantiate(prefab);
                playerHUD = instance.transform.Find("PlayerHUD")?.gameObject;
                DontDestroyOnLoad(instance); // 씬 전환 시 파괴되지 않게
                Debug.Log("[UIManager] PlayerHUD 프리팹 인스턴스화 완료");
            }
            else
            {
                Debug.LogError("[UIManager] Resources/UI/PlayerHUDCanvas 프리팹을 찾을 수 없음");
                return;
            }
        }

        if (playerHUD == null)
            playerHUD = GameObject.Find("PlayerHUDCanvas/PlayerHUD");

        if (playerHUD != null)
        {
            // 자식 오브젝트 찾아서 연결
            hudGoldText = playerHUD.transform.Find("UIContainer/GoldText")?.GetComponent<TextMeshProUGUI>();

            hudHealthBar = playerHUD.transform.Find("UIContainer/HP/Fill")?.GetComponent<Slider>();
            hudHealthText = playerHUD.transform.Find("UIContainer/HP/HealthText")?.GetComponent<TextMeshProUGUI>();

            hudSmallPotionText = playerHUD.transform.Find("UIContainer/SmallPotionCount/SmallPotionCountText")?.GetComponent<TextMeshProUGUI>();
            hudMediumPotionText = playerHUD.transform.Find("UIContainer/MiddlePotionCount/MiddlePotionCountText")?.GetComponent<TextMeshProUGUI>();
            hudLargePotionText = playerHUD.transform.Find("UIContainer/LargePotionCount/LargePotionCountText")?.GetComponent<TextMeshProUGUI>();


            Debug.Log("[UIManager] PlayerHUD 재연결 완료");
        }
        else
        {
            Debug.LogWarning("[UIManager] PlayerHUD를 찾을 수 없음");
        }
    }
}

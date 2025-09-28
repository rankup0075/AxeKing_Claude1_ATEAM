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
    public GameObject playerHUD; // PlayerHUD �г� (���������� �����ǰų� ���� ����)
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
    public GameObject settingsPanel_InGame; // �ν����Ϳ� MainMenu�� SettingsPanel ����
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
                DontDestroyOnLoad(settingsPanel_InGame); // [NEW] SettingsPanel�� �Բ� ����

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
            //Debug.Log("ESC ������");

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

    //ī�޶� �ʱ�ȭ�� ���� �޼ҵ�
    private void ResetCameraTarget()
    {
        var cam = Camera.main ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cam != null)
            cam.ResetTarget();
    }

    //ESC�� UI�� ���� canMove=true�� ������ ���� �޼ҵ�
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
    // ����â
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

        // �÷��̾� ���� ����
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.canControl = false; // �Է� ����
            var anim = player.GetComponent<Animator>();
            if (anim != null) anim.speed = 0f; // �ִϸ��̼� ����
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

        // �÷��̾� ���� ����
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
        // �ڵ� ����
        if (GameManager.Instance != null)
            GameManager.Instance.SavePlayerData();

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Destroy(player.transform.root.gameObject);
            PlayerController.Instance = null; // �̱��� �ʱ�ȭ
        }

        // �ð� �簳 �� ���θ޴� �ε�
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        // �ڵ� ����
        if (GameManager.Instance != null)
            GameManager.Instance.SavePlayerData();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    // ======================
    // Shop UI ����
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
            // BUGFIX: �������� �ƴ϶� �κ��丮 �г��� ����.
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
                playerHUD.SetActive(false); // HUD ����

            Debug.Log("[UIManager] QuestBoard ����");
        }
        else
        {
            Debug.LogError("[UIManager] QuestBoardPanel�� null��");
        }
    }

    public void CloseQuestBoardUI()
    {
        if (questBoardPanel != null)
        {
            questBoardPanel.SetActive(false);
            Time.timeScale = 1f;

            if (playerHUD != null)
                playerHUD.SetActive(true); // HUD �ٽ� �ѱ�
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
    // [NEW] PlayerHUD ���� UI
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
            hudSmallPotionText.text = $"{small:N0}��";
        if (hudMediumPotionText != null)
            hudMediumPotionText.text = $"{medium:N0}��";
        if (hudLargePotionText != null)
            hudLargePotionText.text = $"{large:N0}��";
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

        // [NEW] �� �� �ε�� SettingsPanel�� �ش� ���� Canvas ������ �̵�
        MoveSettingsPanelToActiveCanvas();

        // [NEW] �� �ε� �� HUD�� �ڵ� �翬��
        ReassignPlayerHUD(scene);
    }

    void ReassignPanels()
    {
        // �̹� ����Ǿ� ������ ����, ���� ���� Find
        if (potionShopPanel == null) potionShopPanel = GameObject.Find("PotionShopPanel");
        if (equipmentShopPanel == null) equipmentShopPanel = GameObject.Find("EquipmentShopPanel");
        if (questBoardPanel == null) questBoardPanel = GameObject.Find("QuestBoardPanel");
        if (inventoryPanel == null) inventoryPanel = GameObject.Find("InventoryPanel");
        if (stageSelectPanel == null) stageSelectPanel = GameObject.Find("StageSelectPanel");
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");
        if (stageCompletePanel == null) stageCompletePanel = GameObject.Find("StageCompletePanel");

        if (settingsPanel_InGame == null) settingsPanel_InGame = GameObject.Find("SettingsPanel_InGame");
        

        // SettingsPanel �⺻ ��Ȱ�� ����(CanvasGroup ���)�� �ʱ�ȭ
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

        // [�߰�] QuestBoardPanel ó�� ���� ���� ���α�
        if (questBoardPanel != null && questBoardPanel.activeSelf)
        {
            questBoardPanel.SetActive(false);
        }
    }


    // [NEW] ���� ���� Canvas ������ SettingsPanel�� ����� (�ػ� ������ ��ġ)
    public void MoveSettingsPanelToActiveCanvas()
    {
        if (settingsPanel_InGame == null)
        {
            var obj = GameObject.Find("SettingsPanel_InGame"); // ���� �ִ� �ΰ��� ���� �г� �̸�
            if (obj != null)
            {
                settingsPanel_InGame = obj;
                Debug.Log("[UIManager] SettingsPanel_InGame �ڵ� �����");
            }
        }


        if (settingsPanel_InGame == null)
        {
            Debug.LogWarning("[UIManager] SettingsPanel_InGame ����");
            return;
        }

        // �� ���� Canvas�� �߿��� "�̸��� �׳� Canvas�� ��"�� ����
        var targetCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .FirstOrDefault(c => c.name == "Canvas"); // "�Ϲ� Canvas"�� ����

        if (targetCanvas == null)
        {
            Debug.LogWarning("[UIManager] Ȱ�� Canvas ���� �� SettingsPanel �̵� ����");
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

            Debug.Log($"[UIManager] SettingsPanel_InGame �� '{targetCanvas.name}' �� �̵� �Ϸ�");
        }

        // CanvasGroup �ʱ�ȭ
        var cg = settingsPanel_InGame.GetComponent<CanvasGroup>();
        if (cg == null) cg = settingsPanel_InGame.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

    }

    void HookupSettingsButtons()
    {
        if (settingsPanel_InGame != null) return;

        // Resources���� ������ �ε�
        GameObject prefab = Resources.Load<GameObject>("UI/SettingsPanel_InGame");
        if (prefab == null)
        {
            Debug.LogWarning("[UIManager] Resources/UI/SettingsPanel_InGame.prefab ����");
            return;
        }

        // �ν��Ͻ� ����
        var panelInstance = Instantiate(prefab);
        panelInstance.name = "SettingsPanel_InGame";
        settingsPanel_InGame = panelInstance;

        // ���� ������ ���� �� Canvas ã��
        var targetCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .Where(c => c.isActiveAndEnabled && c.gameObject.activeInHierarchy)
            .OrderByDescending(c => c.sortingOrder)
            .FirstOrDefault();

        if (targetCanvas != null)
        {
            settingsPanel_InGame.transform.SetParent(targetCanvas.transform, false);
            Debug.Log($"[UIManager] SettingsPanel_InGame '{targetCanvas.name}' ������ �̵�");
        }
        else
        {
            Debug.LogWarning("[UIManager] Ȱ�� Canvas ���� �� SettingsPanel_InGame ��� ����");
        }

        closeButton = settingsPanel_InGame.transform
        .Find("ContentContainer/CloseButton")?.GetComponent<Button>();

        mainMenuButton = settingsPanel_InGame.transform
            .Find("ContentContainer/ButtonContainer/ToMainMenuButton")?.GetComponent<Button>();

        quitButton = settingsPanel_InGame.transform
            .Find("ContentContainer/ButtonContainer/QuitGameButton")?.GetComponent<Button>();

        // �̺�Ʈ ����
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

        Debug.Log("[UIManager] SettingsPanel ��ư ���� �Ϸ�");


        // CanvasGroup �⺻�� �ʱ�ȭ
        var cg = settingsPanel_InGame.GetComponent<CanvasGroup>();
        if (cg == null) cg = settingsPanel_InGame.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;


    }


    // [NEW] PlayerHUD �翬��
    void ReassignPlayerHUD(Scene scene)
    {
        // MainMenu������ HUD �ʿ� ����
        if (scene.name == "MainMenu")
        {
            playerHUD = null;
            return;
        }

        if (playerHUD == null)
        {
            // ���� ������ ������ �ν��Ͻ�ȭ
            var prefab = Resources.Load<GameObject>("UI/PlayerHUDCanvas");
            if (prefab != null)
            {
                var instance = Instantiate(prefab);
                playerHUD = instance.transform.Find("PlayerHUD")?.gameObject;
                DontDestroyOnLoad(instance); // �� ��ȯ �� �ı����� �ʰ�
                Debug.Log("[UIManager] PlayerHUD ������ �ν��Ͻ�ȭ �Ϸ�");
            }
            else
            {
                Debug.LogError("[UIManager] Resources/UI/PlayerHUDCanvas �������� ã�� �� ����");
                return;
            }
        }

        if (playerHUD == null)
            playerHUD = GameObject.Find("PlayerHUDCanvas/PlayerHUD");

        if (playerHUD != null)
        {
            // �ڽ� ������Ʈ ã�Ƽ� ����
            hudGoldText = playerHUD.transform.Find("UIContainer/GoldText")?.GetComponent<TextMeshProUGUI>();

            hudHealthBar = playerHUD.transform.Find("UIContainer/HP/Fill")?.GetComponent<Slider>();
            hudHealthText = playerHUD.transform.Find("UIContainer/HP/HealthText")?.GetComponent<TextMeshProUGUI>();

            hudSmallPotionText = playerHUD.transform.Find("UIContainer/SmallPotionCount/SmallPotionCountText")?.GetComponent<TextMeshProUGUI>();
            hudMediumPotionText = playerHUD.transform.Find("UIContainer/MiddlePotionCount/MiddlePotionCountText")?.GetComponent<TextMeshProUGUI>();
            hudLargePotionText = playerHUD.transform.Find("UIContainer/LargePotionCount/LargePotionCountText")?.GetComponent<TextMeshProUGUI>();


            Debug.Log("[UIManager] PlayerHUD �翬�� �Ϸ�");
        }
        else
        {
            Debug.LogWarning("[UIManager] PlayerHUD�� ã�� �� ����");
        }
    }
}

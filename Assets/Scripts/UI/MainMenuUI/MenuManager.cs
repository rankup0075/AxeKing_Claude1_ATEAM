using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject loadingPanel;
    public GameObject confirmQuitPanel;
    public GameObject settingsPanel_MainMenu;

    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Loading UI")]
    public Slider loadingProgressBar;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI tipText;

    [Header("Game Settings")]
    public string firstSceneName = "Town";

    private string[] loadingTips = {
        "Tip: 포션을 활용하여 체력을 즉시 회복하세요!",
        "Tip: Shift를 눌러 달리세요!",
        "Tip: 적에게 맞게 된다면 잠시 경직이 되니 조심하세요!",
        "Tip: 돈을 빨리 벌고 싶다고요? 퀘스트를 하세요!",
        "Tip: 강한 장비는 구매해 강력한 힘을 얻으세요!"
    };

    void Start()
    {
        InitializeMenu();
        SetupButtonEvents();
        continueButton.interactable = HasSaveData();
    }

    void InitializeMenu()
    {
        // SettingsPanel은 UIManager에서 제어
        mainMenuPanel.SetActive(true);
        loadingPanel.SetActive(false);
        confirmQuitPanel.SetActive(false);
    }

    void SetupButtonEvents()
    {
        // 메인 메뉴 버튼들
        newGameButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(ShowQuitConfirm);

        // 종료 확인 패널 버튼
        GameObject confirmButton = confirmQuitPanel.transform.Find("ConfirmButton")?.gameObject;
        GameObject cancelButton = confirmQuitPanel.transform.Find("CancelButton")?.gameObject;

        if (confirmButton != null)
            confirmButton.GetComponent<Button>().onClick.AddListener(QuitGame);
        if (cancelButton != null)
            cancelButton.GetComponent<Button>().onClick.AddListener(CancelQuit);
    }

    public void StartNewGame()
    {
        // 새 게임 데이터 초기화
        PlayerPrefs.DeleteKey("Gold");
        PlayerPrefs.DeleteKey("CurrentTerritory");
        PlayerPrefs.DeleteKey("CurrentStage");
        PlayerPrefs.DeleteKey("CurrentRound");
        PlayerPrefs.Save();

        StartCoroutine(LoadSceneAsync(firstSceneName));
    }

    public void ContinueGame()
    {
        if (HasSaveData())
            StartCoroutine(LoadSceneAsync(firstSceneName));
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        if (settingsPanel_MainMenu != null)
            settingsPanel_MainMenu.SetActive(true);

        var cg = settingsPanel_MainMenu.GetComponent<CanvasGroup>();
        if (cg == null) cg = settingsPanel_MainMenu.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public void CloseSettings()
    {
        if (settingsPanel_MainMenu != null)
            settingsPanel_MainMenu.SetActive(false);
        mainMenuPanel.SetActive(true);

        var cg = settingsPanel_MainMenu.GetComponent<CanvasGroup>();
        if (cg == null) cg = settingsPanel_MainMenu.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void ShowQuitConfirm()
    {
        confirmQuitPanel.SetActive(true);
    }

    public void CancelQuit()
    {
        confirmQuitPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // 로딩 화면 표시
        mainMenuPanel.SetActive(false);
        loadingPanel.SetActive(true);

        // 랜덤 팁 표시
        if (tipText != null)
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];

        // 비동기 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float fakeLoadingTime = 0f;
        float totalFakeTime = 2f; // 최소 2초간 로딩 화면 표시

        while (!asyncLoad.isDone)
        {
            float realProgress = asyncLoad.progress;
            fakeLoadingTime += Time.deltaTime;
            float fakeProgress = fakeLoadingTime / totalFakeTime;
            float displayProgress = Mathf.Min(realProgress, fakeProgress);

            if (loadingProgressBar != null)
                loadingProgressBar.value = displayProgress;

            if (loadingText != null)
                loadingText.text = $"로딩중... {(displayProgress * 100):F0}%";

            if (realProgress >= 0.9f && fakeLoadingTime >= totalFakeTime)
                asyncLoad.allowSceneActivation = true;

            yield return null;
        }
    }

    bool HasSaveData()
    {
        return PlayerPrefs.HasKey("Gold") ||
               PlayerPrefs.HasKey("CurrentTerritory") ||
               PlayerPrefs.HasKey("CurrentStage");
    }

    void Update()
    {
        // ESC → 설정 닫기 or 종료창 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.Instance != null)
            {
                var cg = settingsPanel_MainMenu?.GetComponent<CanvasGroup>();
                if (cg != null && cg.alpha > 0.5f)
                {
                    CloseSettings();
                    mainMenuPanel.SetActive(true);
                    return;
                }
            }

            if (confirmQuitPanel.activeInHierarchy)
                CancelQuit();
        }
    }
}

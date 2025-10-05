using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.IO; // [추가] save.json 체크/삭제용

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
        "Tip: 강한 장비를 구매해 강력한 힘을 얻으세요!"
    };

    void Start()
    {
        InitializeMenu();
        SetupButtonEvents();
        continueButton.interactable = HasSaveData();
    }

    void InitializeMenu()
    {
        mainMenuPanel.SetActive(true);
        loadingPanel.SetActive(false);
        confirmQuitPanel.SetActive(false);
    }

    void SetupButtonEvents()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(ShowQuitConfirm);

        GameObject confirmButton = confirmQuitPanel.transform.Find("ConfirmButton")?.gameObject;
        GameObject cancelButton = confirmQuitPanel.transform.Find("CancelButton")?.gameObject;

        if (confirmButton != null)
            confirmButton.GetComponent<Button>().onClick.AddListener(QuitGame);
        if (cancelButton != null)
            cancelButton.GetComponent<Button>().onClick.AddListener(CancelQuit);
    }

    public void StartNewGame()
    {
        // ==== save.json 삭제 추가 ====
        string path = Path.Combine(Application.persistentDataPath, "save.json");
        if (File.Exists(path))
            File.Delete(path);

        StartCoroutine(LoadSceneAsync(firstSceneName));
    }

    public void ContinueGame()
    {
        if (HasSaveData())
        {
            StartCoroutine(LoadSceneAsync(firstSceneName));
            // 씬 로드 완료 후 세이브 데이터 적용
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveLoadManager.Instance?.LoadGame();
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        mainMenuPanel.SetActive(false);
        loadingPanel.SetActive(true);

        if (tipText != null)
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float fakeLoadingTime = 0f;
        float totalFakeTime = 2f;

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
        // ==== save.json 파일 존재 여부로 체크 ====
        string path = Path.Combine(Application.persistentDataPath, "save.json");
        return File.Exists(path);
    }

    void Update()
    {
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

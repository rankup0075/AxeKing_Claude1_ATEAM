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
        "Tip: ������ Ȱ���Ͽ� ü���� ��� ȸ���ϼ���!",
        "Tip: Shift�� ���� �޸�����!",
        "Tip: ������ �°� �ȴٸ� ��� ������ �Ǵ� �����ϼ���!",
        "Tip: ���� ���� ���� �ʹٰ��? ����Ʈ�� �ϼ���!",
        "Tip: ���� ���� ������ ������ ���� ��������!"
    };

    void Start()
    {
        InitializeMenu();
        SetupButtonEvents();
        continueButton.interactable = HasSaveData();
    }

    void InitializeMenu()
    {
        // SettingsPanel�� UIManager���� ����
        mainMenuPanel.SetActive(true);
        loadingPanel.SetActive(false);
        confirmQuitPanel.SetActive(false);
    }

    void SetupButtonEvents()
    {
        // ���� �޴� ��ư��
        newGameButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(ShowQuitConfirm);

        // ���� Ȯ�� �г� ��ư
        GameObject confirmButton = confirmQuitPanel.transform.Find("ConfirmButton")?.gameObject;
        GameObject cancelButton = confirmQuitPanel.transform.Find("CancelButton")?.gameObject;

        if (confirmButton != null)
            confirmButton.GetComponent<Button>().onClick.AddListener(QuitGame);
        if (cancelButton != null)
            cancelButton.GetComponent<Button>().onClick.AddListener(CancelQuit);
    }

    public void StartNewGame()
    {
        // �� ���� ������ �ʱ�ȭ
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
        // �ε� ȭ�� ǥ��
        mainMenuPanel.SetActive(false);
        loadingPanel.SetActive(true);

        // ���� �� ǥ��
        if (tipText != null)
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];

        // �񵿱� �� �ε�
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float fakeLoadingTime = 0f;
        float totalFakeTime = 2f; // �ּ� 2�ʰ� �ε� ȭ�� ǥ��

        while (!asyncLoad.isDone)
        {
            float realProgress = asyncLoad.progress;
            fakeLoadingTime += Time.deltaTime;
            float fakeProgress = fakeLoadingTime / totalFakeTime;
            float displayProgress = Mathf.Min(realProgress, fakeProgress);

            if (loadingProgressBar != null)
                loadingProgressBar.value = displayProgress;

            if (loadingText != null)
                loadingText.text = $"�ε���... {(displayProgress * 100):F0}%";

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
        // ESC �� ���� �ݱ� or ����â �ݱ�
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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject loadingPanel;
    public GameObject confirmQuitPanel;

    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings UI")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    //public Dropdown qualityDropdown;
    //public Toggle fullscreenToggle;

    [Header("Loading UI")]
    public Slider loadingProgressBar;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI tipText;

    //[Header("Audio")]
    //public AudioSource backgroundMusic;
    //public AudioClip buttonClickSound;
    //public AudioClip buttonHoverSound;

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
        LoadPlayerPrefs();

        // 버튼 이벤트 연결
        SetupButtonEvents();

        // 연속 게임 버튼 활성화 여부 확인
        continueButton.interactable = HasSaveData();

        //// 배경 음악 재생
        //if (backgroundMusic != null)
        //    backgroundMusic.Play();
    }

    void InitializeMenu()
    {
        // 모든 패널을 비활성화하고 메인 메뉴만 활성화
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
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

        // 버튼 사운드 효과 추가
        AddButtonSounds(newGameButton);
        AddButtonSounds(continueButton);
        AddButtonSounds(settingsButton);
        AddButtonSounds(quitButton);

        // 설정 패널 버튼들
        GameObject backButton = settingsPanel.transform.Find("BackButton")?.gameObject;
        if (backButton != null)
        {
            backButton.GetComponent<Button>().onClick.AddListener(CloseSettings);
        }

        // 종료 확인 패널 버튼들
        GameObject confirmButton = confirmQuitPanel.transform.Find("ConfirmButton")?.gameObject;
        GameObject cancelButton = confirmQuitPanel.transform.Find("CancelButton")?.gameObject;

        if (confirmButton != null)
            confirmButton.GetComponent<Button>().onClick.AddListener(QuitGame);
        if (cancelButton != null)
            cancelButton.GetComponent<Button>().onClick.AddListener(CancelQuit);
    }

    void AddButtonSounds(Button button)
    {
        // 버튼 클릭 사운드
        //button.onClick.AddListener(() => PlaySound(buttonClickSound));

        // 버튼 호버 사운드 (EventTrigger 사용)
        UnityEngine.EventSystems.EventTrigger trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        //entry.callback.AddListener((data) => PlaySound(buttonHoverSound));
        trigger.triggers.Add(entry);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
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
        {
            StartCoroutine(LoadSceneAsync(firstSceneName));
        }
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        LoadCurrentSettings();
    }

    public void CloseSettings()
    {
        SaveSettings();
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
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
            // 실제 로딩 진행도와 가짜 로딩 시간을 조합
            float realProgress = asyncLoad.progress;
            fakeLoadingTime += Time.deltaTime;
            float fakeProgress = fakeLoadingTime / totalFakeTime;

            float displayProgress = Mathf.Min(realProgress, fakeProgress);

            // UI 업데이트
            if (loadingProgressBar != null)
                loadingProgressBar.value = displayProgress;

            if (loadingText != null)
                loadingText.text = $"로딩중... {(displayProgress * 100):F0}%";

            // 로딩 완료 조건
            if (realProgress >= 0.9f && fakeLoadingTime >= totalFakeTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    void LoadPlayerPrefs()
    {
        // 오디오 설정
        //if (masterVolumeSlider != null)
        //{
        //    float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        //    masterVolumeSlider.value = masterVolume;
        //    AudioListener.volume = masterVolume;
        //}

        //if (musicVolumeSlider != null && backgroundMusic != null)
        //{
        //    float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        //    musicVolumeSlider.value = musicVolume;
        //    backgroundMusic.volume = musicVolume;
        //}

        //// 그래픽 설정
        //if (qualityDropdown != null)
        //{
        //    int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        //    qualityDropdown.value = quality;
        //    QualitySettings.SetQualityLevel(quality);
        //}

        //if (fullscreenToggle != null)
        //{
        //    bool fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        //    fullscreenToggle.isOn = fullscreen;
        //    Screen.fullScreen = fullscreen;
        //}
    }

    void LoadCurrentSettings()
    {
        // 현재 설정값들을 UI에 반영
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = AudioListener.volume;

        //if (musicVolumeSlider != null && backgroundMusic != null)
        //    musicVolumeSlider.value = backgroundMusic.volume;

        //if (qualityDropdown != null)
        //    qualityDropdown.value = QualitySettings.GetQualityLevel();

        //if (fullscreenToggle != null)
        //    fullscreenToggle.isOn = Screen.fullScreen;

        //// 슬라이더 이벤트 연결
        //if (masterVolumeSlider != null)
        //    masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        //if (musicVolumeSlider != null)
        //    musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        //if (qualityDropdown != null)
        //    qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        //if (fullscreenToggle != null)
        //    fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    void SaveSettings()
    {
        // 설정값 저장
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);

        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);

        //if (qualityDropdown != null)
        //    PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);

        //if (fullscreenToggle != null)
        //    PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
    }

    // 설정 변경 이벤트들
    void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    void OnMusicVolumeChanged(float value)
    {
        //if (backgroundMusic != null)
        //    backgroundMusic.volume = value;
    }

    void OnQualityChanged(int value)
    {
        QualitySettings.SetQualityLevel(value);
    }

    void OnFullscreenChanged(bool value)
    {
        Screen.fullScreen = value;
    }

    bool HasSaveData()
    {
        return PlayerPrefs.HasKey("Gold") ||
               PlayerPrefs.HasKey("CurrentTerritory") ||
               PlayerPrefs.HasKey("CurrentStage");
    }

    void Update()
    {
        // ESC 키로 설정 패널 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeInHierarchy)
            {
                CloseSettings();
            }
            else if (confirmQuitPanel.activeInHierarchy)
            {
                CancelQuit();
            }
        }
    }

    public void CloseSettingPanel()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            Time.timeScale = 1f; // 게임 재개
        }

        // 메시지 패널도 닫기
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
    }
}
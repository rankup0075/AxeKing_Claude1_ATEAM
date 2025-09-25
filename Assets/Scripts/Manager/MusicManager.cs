using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer masterMixer; // MasterMixer 연결

    [Header("Audio Sources")]
    public AudioSource musicSource1;
    public AudioSource musicSource2;

    [Header("Menu Music")]
    public AudioClip mainMenuMusic;

    [Header("Town Area Music")]
    public AudioClip townMusic;
    public AudioClip equipmentShopMusic;
    public AudioClip alchemistShopMusic;
    public AudioClip hideoutMusic;

    [Header("Stage Select Music")]
    public AudioClip stageSelectMusic;

    [Header("Territory Music")]
    public AudioClip territory1Music;
    public AudioClip territory2Music;
    public AudioClip territory3Music;
    public AudioClip territory4Music;
    public AudioClip territory5Music;

    [Header("Boss Music")]
    public AudioClip bossBattleMusic;
    public AudioClip finalBossMusic;

    [Header("Settings")]
    public float crossfadeDuration = 2f;
    public float defaultVolume = 0.7f;

    private AudioSource currentAudioSource;
    private AudioSource nextAudioSource;
    private AudioClip currentClip;
    private bool isCrossfading = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioSources()
    {
        if (musicSource1 == null) musicSource1 = gameObject.AddComponent<AudioSource>();
        if (musicSource2 == null) musicSource2 = gameObject.AddComponent<AudioSource>();

        SetupAudioSource(musicSource1);
        SetupAudioSource(musicSource2);

        currentAudioSource = musicSource1;
        nextAudioSource = musicSource2;
    }

    void SetupAudioSource(AudioSource source)
    {
        source.loop = true;
        source.volume = defaultVolume;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        AudioClip targetClip = GetMusicForScene(sceneName);
        if (targetClip != null && targetClip != currentClip)
        {
            PlayMusic(targetClip);
        }
    }

    AudioClip GetMusicForScene(string sceneName)
    {
        switch (sceneName.ToLower())
        {
            case "mainmenu": return mainMenuMusic;
            case "town": return townMusic;
            case "equipmentshop": return equipmentShopMusic;
            case "alchemistshop": return alchemistShopMusic;
            case "hideout": return hideoutMusic;
            case "stageselect": return stageSelectMusic;
            case "stage_1_1":
            case "stage_1_2":
            case "stage_1_3":
            case "stage_1_4":
            case "stage_1_5":
                return territory1Music;
            case "stage_2_1":
            case "stage_2_2":
            case "stage_2_3":
            case "stage_2_4":
            case "stage_2_5":
                return territory2Music;
            case "stage_3_1":
            case "stage_3_2":
            case "stage_3_3":
            case "stage_3_4":
            case "stage_3_5":
                return territory3Music;
            case "stage_4_1":
            case "stage_4_2":
            case "stage_4_3":
            case "stage_4_4":
            case "stage_4_5":
                return territory4Music;
            case "stage_5_1":
            case "stage_5_2":
            case "stage_5_3":
            case "stage_5_4":
            case "stage_5_5":
                return territory5Music;
            default:
                return null;
        }
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (newClip == null || newClip == currentClip) return;
        if (isCrossfading) StopAllCoroutines();

        StartCoroutine(CrossfadeMusic(newClip));
    }

    System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        isCrossfading = true;

        nextAudioSource.clip = newClip;
        nextAudioSource.volume = 0f;
        nextAudioSource.Play();

        float elapsed = 0f;
        float startVolume = currentAudioSource.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / crossfadeDuration;

            currentAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            nextAudioSource.volume = Mathf.Lerp(0f, defaultVolume, t);

            yield return null;
        }

        currentAudioSource.Stop();
        currentAudioSource.volume = defaultVolume;
        nextAudioSource.volume = defaultVolume;

        AudioSource temp = currentAudioSource;
        currentAudioSource = nextAudioSource;
        nextAudioSource = temp;

        currentClip = newClip;
        isCrossfading = false;
    }

    // ===== 볼륨 제어 부분 =====
    public void SetMasterVolume(float value)
    {
        masterMixer.SetFloat("Master Volume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
    }

    public void SetBGMVolume(float value)
    {
        masterMixer.SetFloat("BGM Volume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
    }

    public void SetSFXVolume(float value)
    {
        masterMixer.SetFloat("SFX Volume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
    }
}

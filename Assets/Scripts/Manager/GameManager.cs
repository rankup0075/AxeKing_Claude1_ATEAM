using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PortalSpawnPoint
{
    public string portalID;   // 포탈 ID
    public Vector3 position;  // 이동할 좌표 (월드 좌표)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public long gold = 0;
    public int currentTerritory = 1;
    public int currentStage = 1;
    public int currentRound = 1;

    [Header("Game Settings")]
    public bool isPaused = false;

    [Header("Portal Spawn System")]
    public string lastPortalID = null;

    [Tooltip("씬 로드시 약간의 대기 후 배치 (오브젝트 생성 순서 문제 방지)")]
    [SerializeField] private float spawnPlaceDelay = 0.05f;

    [Header("Portal Spawn Table (Inspector에서 설정)")]
    public List<PortalSpawnPoint> spawnPoints = new List<PortalSpawnPoint>();

    public GameObject playerHUDPrefab;
    GameObject playerHUDInstance;

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // ==========================
    // Unity Events
    // ==========================
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GM] 씬 로드됨: {scene.name}, lastPortalID={lastPortalID}");
        StopAllCoroutines();
        StartCoroutine(PlacePlayerAtLastPortalAfterDelay(scene.name));

        bool show = scene.name != "MainMenu";
        if (playerHUDInstance == null && playerHUDPrefab != null)
        {
            playerHUDInstance = Instantiate(playerHUDPrefab);
            DontDestroyOnLoad(playerHUDInstance);

            // [NEW] HUD 초기화
            UIManager.Instance?.InitPlayerHUD();
        }
        if (playerHUDInstance != null)
            playerHUDInstance.SetActive(show);
    }

    // ==========================
    // Player / Gold 관리
    // ==========================
    public long Gold => gold;

    public void AddGold(long amount)
    {
        gold += amount;
        UIManager.Instance?.UpdateGoldDisplay(gold);
        UIManager.Instance?.UpdateHUDGold(gold); // [NEW] HUD 반영
        SavePlayerData();
    }

    public bool SpendGold(long amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UIManager.Instance?.UpdateGoldDisplay(gold);
            UIManager.Instance?.UpdateHUDGold(gold); // [NEW] HUD 반영
            SavePlayerData();
            return true;
        }
        return false;
    }

    // ==========================
    // Stage / Game Over
    // ==========================
    public void UnlockStage(int territoryId, int stageId, int roundId)
    {
        if (roundId > 3)
        {
            roundId = 1;
            stageId++;
            if (stageId > 5)
            {
                stageId = 1;
                territoryId++;
            }
        }

        if (territoryId > currentTerritory ||
            (territoryId == currentTerritory && stageId > currentStage) ||
            (territoryId == currentTerritory && stageId == currentStage && roundId > currentRound))
        {
            currentTerritory = territoryId;
            currentStage = stageId;
            currentRound = roundId;
            SavePlayerData();
        }
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        UIManager.Instance?.ShowGameOverUI();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Town");
    }

    // ==========================
    // Save / Load
    // ==========================
    public void SavePlayerData()
    {
        PlayerPrefs.SetString("Gold", gold.ToString("N0"));
        PlayerPrefs.SetInt("CurrentTerritory", currentTerritory);
        PlayerPrefs.SetInt("CurrentStage", currentStage);
        PlayerPrefs.SetInt("CurrentRound", currentRound);
        PlayerPrefs.SetString("LastPortalID", lastPortalID ?? "");
        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        string savedGold = PlayerPrefs.GetString("Gold", "0");
        long parsed;
        if (!long.TryParse(savedGold, out parsed)) parsed = 0;
        gold = parsed;

        currentTerritory = PlayerPrefs.GetInt("CurrentTerritory", 1);
        currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        currentRound = PlayerPrefs.GetInt("CurrentRound", 1);
        lastPortalID = PlayerPrefs.GetString("LastPortalID", "");
        UIManager.Instance?.UpdateGoldDisplay(gold);
        UIManager.Instance?.UpdateHUDGold(gold); // [NEW] HUD 반영
    }

    // ==========================
    // Portal Handling
    // ==========================
    public void SetLastPortalID(string portalId)
    {
        lastPortalID = portalId;
        Debug.Log($"[GM] 마지막 포탈 ID 저장: {lastPortalID}");
    }

    private IEnumerator PlacePlayerAtLastPortalAfterDelay(string sceneName)
    {
        yield return null;
        yield return new WaitForSeconds(spawnPlaceDelay);

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[GM] Player 오브젝트를 찾지 못함");
            yield break;
        }

        if (string.IsNullOrEmpty(lastPortalID))
        {
            Debug.Log("[GM] lastPortalID 없음 → 위치 보정 스킵");
            yield break;
        }

        // === Inspector에서 미리 정의한 좌표 찾기 ===
        PortalSpawnPoint match = spawnPoints.Find(p => p.portalID == lastPortalID);

        if (match != null)
        {
            // 좌표 고정
            player.transform.position = match.position;

            // Rigidbody 안정화
            if (player.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }

            // PlayerController 입력 잠시 차단
            if (player.TryGetComponent<PlayerController>(out var pc))
                StartCoroutine(FreezePlayerControl(pc, 0.1f));

            Debug.Log($"[GM] '{lastPortalID}' 고정 좌표 적용 완료 → {match.position}");
        }
        else
        {
            Debug.LogWarning($"[GM] '{lastPortalID}' 매핑된 좌표 없음 (씬: {sceneName})");
        }
    }

    private IEnumerator FreezePlayerControl(PlayerController pc, float duration)
    {
        pc.canControl = false;
        yield return new WaitForSeconds(duration);
        pc.canControl = true;
    }
}

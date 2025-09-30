using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Portal;

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

    // === [NEW] 펫 관련 ===
    [Header("Pet Settings")]
    public GameObject petPrefab;
    private GameObject petInstance;

    [Header("Player 관리")]
    public GameObject playerPrefab;   // PlayerRoot 프리팹
    private GameObject playerInstance;

    //보스 및 포탈 관리
    private HashSet<string> clearedRounds = new HashSet<string>();
    private HashSet<string> defeatedBosses = new HashSet<string>();

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

    /// <summary>
    /// 보스 및 포탈 관리 로직
    /// </summary>
    public void SetRoundCleared(string roundId)
    {
        clearedRounds.Add(roundId);
    }

    public bool IsRoundCleared(string roundId)
    {
        return clearedRounds.Contains(roundId);
    }

    public void SetBossDefeated(string bossId)
    {
        defeatedBosses.Add(bossId);
    }

    public bool IsBossDefeated(string bossId)
    {
        return defeatedBosses.Contains(bossId);
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GM] 씬 로드됨: {scene.name}, lastPortalID={lastPortalID}");


        if (playerInstance == null && playerPrefab != null && scene.name != "MainMenu")
        {
            Vector3 spawnPos = Vector3.zero;

            if (scene.name == "Town")
            {
                // Town 처음 진입 → 기본 스폰 좌표 강제 지정
                spawnPos = new Vector3(-23f, 0f, 0f); // 원하는 위치로 바꿔라
            }
            playerInstance = Instantiate(playerPrefab);
            DontDestroyOnLoad(playerInstance);
            Debug.Log($"[GM] Player 프리팹 생성 완료 → {playerInstance.name}");
        }

        StopAllCoroutines();
        StartCoroutine(PlacePlayerAtLastPortalAfterDelay(scene.name));

        bool show = scene.name != "MainMenu";

        // === HUD 생성 안전장치 추가 ===
        if (playerHUDInstance == null && playerHUDPrefab != null)
        {
            playerHUDInstance = Instantiate(playerHUDPrefab);
            DontDestroyOnLoad(playerHUDInstance);
            UIManager.Instance?.InitPlayerHUD();
        }
        else
        {
            // 이미 존재하면 중복 생성하지 않고, MainMenu 여부만 체크해서 활성/비활성
            if (playerHUDInstance != null)
                Debug.Log("[GM] 기존 HUD 인스턴스 재사용");
        }

        if (playerHUDInstance != null)
            playerHUDInstance.SetActive(show);

        if (scene.name != "MainMenu")
        {
            if (petPrefab != null && petInstance == null)
            {
                // 플레이어 기준 스폰
                Vector3 spawnPos = PlayerController.Instance != null
                    ? PlayerController.Instance.transform.position + new Vector3(1f, 0f, -1f)
                    : Vector3.zero;

                petInstance = Instantiate(petPrefab, spawnPos, Quaternion.identity);
                DontDestroyOnLoad(petInstance);
                Debug.Log($"[GM] 펫 생성 완료: {petPrefab.name}");
            }
            else if (petInstance != null && PlayerController.Instance != null)
            {
                // 씬이 바뀌면 항상 플레이어 옆으로 보정
                petInstance.transform.position = PlayerController.Instance.transform.position + new Vector3(1f, 0f, -1f);

                if (petInstance.TryGetComponent<Rigidbody>(out var petRb))
                {
                    petRb.linearVelocity = Vector3.zero;
                    petRb.angularVelocity = Vector3.zero;
                    petRb.Sleep();
                }

                Debug.Log("[GM] 씬 로드 후 펫 위치 보정 완료");
            }
        }
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

        // ===============================
        // [NEW] Town 처리
        // ===============================
        if (sceneName == "Town")
        {
            if (string.IsNullOrEmpty(lastPortalID))
            {
                // MainMenu → Town : TownSpawnPoint 사용
                var townSpawn = GameObject.Find("TownSpawnPoint");
                if (townSpawn != null)
                {
                    player.transform.position = townSpawn.transform.position;
                    Debug.Log($"[GM] TownSpawnPoint 위치로 스폰 → {townSpawn.transform.position}");
                    AdjustPetPosition(townSpawn.transform.position); // [NEW]
                }
            }
            else
            {
                // Stage → Town 귀환 : 마지막 포탈 기준
                var portals = GameObject.FindObjectsByType<Portal>(FindObjectsSortMode.None);
                var match = portals.FirstOrDefault(p => p.portalID == lastPortalID);
                if (match != null)
                {
                    player.transform.position = match.transform.position;
                    Debug.Log($"[GM] Town 귀환 '{lastPortalID}' 포탈 위치로 스폰");
                }
            }
            yield break;
        }

        // ===============================
        // [NEW] Stage 처리
        // ===============================
        if (sceneName.StartsWith("Stage"))
        {
            Portal lastPortal = null;
            if (!string.IsNullOrEmpty(lastPortalID))
            {
                var portals = GameObject.FindObjectsByType<Portal>(FindObjectsSortMode.None);
                lastPortal = portals.FirstOrDefault(p => p.portalID == lastPortalID);
            }

            if (lastPortal != null && lastPortal.portalDirection == Portal.PortalDirection.Backward)
            {
                // 역방향 → 마지막 포탈 위치 사용
                player.transform.position = lastPortal.transform.position;
                Debug.Log($"[GM] 역방향 입장 → '{lastPortalID}' 포탈 위치로 스폰");
                AdjustPetPosition(lastPortal.transform.position); // [NEW]

            }
            else
            {
                // 정방향 → PlayerSpawnPoint 우선
                var spawnPoint = GameObject.Find("PlayerSpawnPoint");
                if (spawnPoint != null)
                {
                    player.transform.position = spawnPoint.transform.position;
                    Debug.Log($"[GM] 정방향 입장 → PlayerSpawnPoint 위치로 스폰 → {spawnPoint.transform.position}");
                    AdjustPetPosition(spawnPoint.transform.position); // [NEW]
                }
                else if (lastPortal != null)
                {
                    // PlayerSpawnPoint 없을 경우 포탈 좌표 fallback
                    player.transform.position = lastPortal.transform.position;
                    Debug.Log($"[GM] PlayerSpawnPoint 없음 → '{lastPortalID}' 포탈 위치로 fallback");
                    AdjustPetPosition(spawnPoint.transform.position); // [NEW]
                }
            }
            yield break;
        }

        // ===============================
        // [기존] 기타 fallback
        // ===============================
        if (string.IsNullOrEmpty(lastPortalID))
        {
            Debug.Log("[GM] lastPortalID 없음 → 위치 보정 스킵");
            yield break;
        }

        var fallbackMatch = spawnPoints.Find(p => p.portalID == lastPortalID);
        if (fallbackMatch != null)
        {
            player.transform.position = fallbackMatch.position;
            Debug.Log($"[GM] fallback: '{lastPortalID}' 좌표 적용 → {fallbackMatch.position}");
            AdjustPetPosition(fallbackMatch.position); // [NEW]
        }
    }

    private void AdjustPetPosition(Vector3 basePos)
    {
        if (petInstance == null) return;

        Vector3 petPos = basePos + new Vector3(1f, 0f, -1f);
        petInstance.transform.position = petPos;

        if (petInstance.TryGetComponent<Rigidbody>(out var petRb))
        {
            petRb.linearVelocity = Vector3.zero;
            petRb.angularVelocity = Vector3.zero;
            petRb.Sleep();
        }

        Debug.Log($"[GM] 펫 위치 보정 완료 → {petPos}");
    }



    private IEnumerator FreezePlayerControl(PlayerController pc, float duration)
    {
        pc.canControl = false;
        yield return new WaitForSeconds(duration);
        pc.canControl = true;
    }
}

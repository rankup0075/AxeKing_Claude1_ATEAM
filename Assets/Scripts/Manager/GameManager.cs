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
    public string portalID;   // ��Ż ID
    public Vector3 position;  // �̵��� ��ǥ (���� ��ǥ)
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

    [Tooltip("�� �ε�� �ణ�� ��� �� ��ġ (������Ʈ ���� ���� ���� ����)")]
    [SerializeField] private float spawnPlaceDelay = 0.05f;

    [Header("Portal Spawn Table (Inspector���� ����)")]
    public List<PortalSpawnPoint> spawnPoints = new List<PortalSpawnPoint>();

    public GameObject playerHUDPrefab;
    GameObject playerHUDInstance;

    // === [NEW] �� ���� ===
    [Header("Pet Settings")]
    public GameObject petPrefab;
    private GameObject petInstance;

    [Header("Player ����")]
    public GameObject playerPrefab;   // PlayerRoot ������
    private GameObject playerInstance;

    //���� �� ��Ż ����
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
    /// ���� �� ��Ż ���� ����
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
        Debug.Log($"[GM] �� �ε��: {scene.name}, lastPortalID={lastPortalID}");


        if (playerInstance == null && playerPrefab != null && scene.name != "MainMenu")
        {
            Vector3 spawnPos = Vector3.zero;

            if (scene.name == "Town")
            {
                // Town ó�� ���� �� �⺻ ���� ��ǥ ���� ����
                spawnPos = new Vector3(-23f, 0f, 0f); // ���ϴ� ��ġ�� �ٲ��
            }
            playerInstance = Instantiate(playerPrefab);
            DontDestroyOnLoad(playerInstance);
            Debug.Log($"[GM] Player ������ ���� �Ϸ� �� {playerInstance.name}");
        }

        StopAllCoroutines();
        StartCoroutine(PlacePlayerAtLastPortalAfterDelay(scene.name));

        bool show = scene.name != "MainMenu";

        // === HUD ���� ������ġ �߰� ===
        if (playerHUDInstance == null && playerHUDPrefab != null)
        {
            playerHUDInstance = Instantiate(playerHUDPrefab);
            DontDestroyOnLoad(playerHUDInstance);
            UIManager.Instance?.InitPlayerHUD();
        }
        else
        {
            // �̹� �����ϸ� �ߺ� �������� �ʰ�, MainMenu ���θ� üũ�ؼ� Ȱ��/��Ȱ��
            if (playerHUDInstance != null)
                Debug.Log("[GM] ���� HUD �ν��Ͻ� ����");
        }

        if (playerHUDInstance != null)
            playerHUDInstance.SetActive(show);

        if (scene.name != "MainMenu")
        {
            if (petPrefab != null && petInstance == null)
            {
                // �÷��̾� ���� ����
                Vector3 spawnPos = PlayerController.Instance != null
                    ? PlayerController.Instance.transform.position + new Vector3(1f, 0f, -1f)
                    : Vector3.zero;

                petInstance = Instantiate(petPrefab, spawnPos, Quaternion.identity);
                DontDestroyOnLoad(petInstance);
                Debug.Log($"[GM] �� ���� �Ϸ�: {petPrefab.name}");
            }
            else if (petInstance != null && PlayerController.Instance != null)
            {
                // ���� �ٲ�� �׻� �÷��̾� ������ ����
                petInstance.transform.position = PlayerController.Instance.transform.position + new Vector3(1f, 0f, -1f);

                if (petInstance.TryGetComponent<Rigidbody>(out var petRb))
                {
                    petRb.linearVelocity = Vector3.zero;
                    petRb.angularVelocity = Vector3.zero;
                    petRb.Sleep();
                }

                Debug.Log("[GM] �� �ε� �� �� ��ġ ���� �Ϸ�");
            }
        }
    }

    // ==========================
    // Player / Gold ����
    // ==========================
    public long Gold => gold;

    public void AddGold(long amount)
    {
        gold += amount;
        UIManager.Instance?.UpdateGoldDisplay(gold);
        UIManager.Instance?.UpdateHUDGold(gold); // [NEW] HUD �ݿ�
        SavePlayerData();
    }

    public bool SpendGold(long amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UIManager.Instance?.UpdateGoldDisplay(gold);
            UIManager.Instance?.UpdateHUDGold(gold); // [NEW] HUD �ݿ�
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
        UIManager.Instance?.UpdateHUDGold(gold); // [NEW] HUD �ݿ�
    }

    // ==========================
    // Portal Handling
    // ==========================
    public void SetLastPortalID(string portalId)
    {
        lastPortalID = portalId;
        Debug.Log($"[GM] ������ ��Ż ID ����: {lastPortalID}");
    }


    private IEnumerator PlacePlayerAtLastPortalAfterDelay(string sceneName)
    {
        yield return null;
        yield return new WaitForSeconds(spawnPlaceDelay);

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[GM] Player ������Ʈ�� ã�� ����");
            yield break;
        }

        // ===============================
        // [NEW] Town ó��
        // ===============================
        if (sceneName == "Town")
        {
            if (string.IsNullOrEmpty(lastPortalID))
            {
                // MainMenu �� Town : TownSpawnPoint ���
                var townSpawn = GameObject.Find("TownSpawnPoint");
                if (townSpawn != null)
                {
                    player.transform.position = townSpawn.transform.position;
                    Debug.Log($"[GM] TownSpawnPoint ��ġ�� ���� �� {townSpawn.transform.position}");
                    AdjustPetPosition(townSpawn.transform.position); // [NEW]
                }
            }
            else
            {
                // Stage �� Town ��ȯ : ������ ��Ż ����
                var portals = GameObject.FindObjectsByType<Portal>(FindObjectsSortMode.None);
                var match = portals.FirstOrDefault(p => p.portalID == lastPortalID);
                if (match != null)
                {
                    player.transform.position = match.transform.position;
                    Debug.Log($"[GM] Town ��ȯ '{lastPortalID}' ��Ż ��ġ�� ����");
                }
            }
            yield break;
        }

        // ===============================
        // [NEW] Stage ó��
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
                // ������ �� ������ ��Ż ��ġ ���
                player.transform.position = lastPortal.transform.position;
                Debug.Log($"[GM] ������ ���� �� '{lastPortalID}' ��Ż ��ġ�� ����");
                AdjustPetPosition(lastPortal.transform.position); // [NEW]

            }
            else
            {
                // ������ �� PlayerSpawnPoint �켱
                var spawnPoint = GameObject.Find("PlayerSpawnPoint");
                if (spawnPoint != null)
                {
                    player.transform.position = spawnPoint.transform.position;
                    Debug.Log($"[GM] ������ ���� �� PlayerSpawnPoint ��ġ�� ���� �� {spawnPoint.transform.position}");
                    AdjustPetPosition(spawnPoint.transform.position); // [NEW]
                }
                else if (lastPortal != null)
                {
                    // PlayerSpawnPoint ���� ��� ��Ż ��ǥ fallback
                    player.transform.position = lastPortal.transform.position;
                    Debug.Log($"[GM] PlayerSpawnPoint ���� �� '{lastPortalID}' ��Ż ��ġ�� fallback");
                    AdjustPetPosition(spawnPoint.transform.position); // [NEW]
                }
            }
            yield break;
        }

        // ===============================
        // [����] ��Ÿ fallback
        // ===============================
        if (string.IsNullOrEmpty(lastPortalID))
        {
            Debug.Log("[GM] lastPortalID ���� �� ��ġ ���� ��ŵ");
            yield break;
        }

        var fallbackMatch = spawnPoints.Find(p => p.portalID == lastPortalID);
        if (fallbackMatch != null)
        {
            player.transform.position = fallbackMatch.position;
            Debug.Log($"[GM] fallback: '{lastPortalID}' ��ǥ ���� �� {fallbackMatch.position}");
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

        Debug.Log($"[GM] �� ��ġ ���� �Ϸ� �� {petPos}");
    }



    private IEnumerator FreezePlayerControl(PlayerController pc, float duration)
    {
        pc.canControl = false;
        yield return new WaitForSeconds(duration);
        pc.canControl = true;
    }
}

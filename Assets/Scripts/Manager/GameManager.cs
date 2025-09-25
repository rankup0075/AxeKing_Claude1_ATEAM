using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public int gold = 0;
    public int currentTerritory = 1;
    public int currentStage = 1;
    public int currentRound = 1;

    [Header("Game Settings")]
    public bool isPaused = false;

    // === 포탈 스폰 시스템 ===
    // [NEW] 마지막으로 사용한 포탈의 고유 ID (Town에서 같은 ID의 포탈 위치로 스폰)
    public string lastPortalID = null;

    // [NEW] 장면 로드시 배치가 씬 내 오브젝트 생성 순서보다 먼저될 때를 대비한 딜레이
    [SerializeField] private float spawnPlaceDelay = 0.02f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // [NEW] 씬 로드 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    void OnDestroy()
    {
        // [NEW] 씬 로드 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public int Gold => gold;

    public void AddGold(int amount)
    {
        gold += amount;
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateGoldDisplay(gold);
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateGoldDisplay(gold);
            return true;
        }
        return false;
    }

    public void UnlockStage(int territoryId, int stageId, int roundId)
    {
        // 다음 스테이지 잠금 해제 로직
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

        // 진행도 저장
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
        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOverUI();
        else
            Debug.LogWarning("[GameManager] UIManager가 없어 GameOverUI 표시 불가");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Town");
    }

    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("Gold", gold);
        PlayerPrefs.SetInt("CurrentTerritory", currentTerritory);
        PlayerPrefs.SetInt("CurrentStage", currentStage);
        PlayerPrefs.SetInt("CurrentRound", currentRound);

        // [NEW] 마지막 포탈 ID 저장
        PlayerPrefs.SetString("LastPortalID", lastPortalID ?? "");

        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        gold = PlayerPrefs.GetInt("Gold", 0);
        currentTerritory = PlayerPrefs.GetInt("CurrentTerritory", 1);
        currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        currentRound = PlayerPrefs.GetInt("CurrentRound", 1);

        // [NEW] 마지막 포탈 ID 로드
        lastPortalID = PlayerPrefs.GetString("LastPortalID", "");

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateGoldDisplay(gold);
    }

    // [NEW] 포탈에서 저장: 마지막 사용 포탈의 고유 ID
    public void SetLastPortalID(string portalId)
    {
        lastPortalID = portalId;
        Debug.Log($"[GameManager] 마지막 포탈 ID 저장: {lastPortalID}");
    }

    // [NEW] 씬 로드 → Town이면 같은 ID의 포탈을 찾아 그 위치에 플레이어 배치
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(lastPortalID))
        {
            Debug.Log("[GameManager] lastPortalID 비어있음 → 포지션 보정 없음");
            return;
        }
        StartCoroutine(PlacePlayerAtLastPortalAfterDelay());
    }


    // [NEW] 실제 배치 코루틴
    private System.Collections.IEnumerator PlacePlayerAtLastPortalAfterDelay()
    {
        yield return null;
        yield return new WaitForSeconds(spawnPlaceDelay);

        Portal[] portals = Object.FindObjectsByType<Portal>(FindObjectsSortMode.None);
        if (portals == null || portals.Length == 0)
        {
            Debug.LogWarning("[GameManager] 이 씬에서 Portal을 찾지 못함");
            yield break;
        }

        Portal target = null;
        foreach (var p in portals)
        {
            if (p != null && p.portalID == lastPortalID)
            {
                target = p;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"[GameManager] 같은 ID 포탈({lastPortalID}) 없음");
            yield break;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[GameManager] Player 태그 오브젝트를 찾지 못함");
            yield break;
        }

        // X, Y만 이동, Z는 유지
        Vector3 currentPos = player.transform.position;
        Vector3 spawnPos;

        if (target.spawnPoint != null)
        {
            spawnPos = new Vector3(
                target.spawnPoint.position.x,
                target.spawnPoint.position.y,
                currentPos.z                // ← 기존 Z값 유지
            );
        }
        else
        {
            spawnPos = new Vector3(
                target.transform.position.x,
                target.transform.position.y,
                currentPos.z                // ← 기존 Z값 유지
            );
        }

        // 회전 동기화 (선택)
        if (target.spawnPoint != null)
        {
            Vector3 euler = player.transform.eulerAngles;
            euler.y = target.spawnPoint.eulerAngles.y;
            player.transform.eulerAngles = euler;
        }

        player.transform.position = spawnPos;

        // Rigidbody 속도 제거
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        Debug.Log($"[GameManager] '{lastPortalID}' 스폰포인트 적용 (Z 고정): {player.transform.position}");
    }


}

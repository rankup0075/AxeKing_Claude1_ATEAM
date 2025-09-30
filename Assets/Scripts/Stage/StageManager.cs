using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Regions & Stages")]
    public List<RegionData> regions = new List<RegionData>();
    public List<StageData> stages = new List<StageData>(); // [NOTE] 전역 스테이지 리스트 (자동 동기화용)
    private Vector3 pendingSpawnPos = Vector3.zero; // [NEW] 다음 씬에서 쓸 위치


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // [NEW] 씬 전환에도 살아남음
        }
        else
        {
            Destroy(gameObject);
        }
   
    }

    /// <summary>
    /// [NEW] regions 안의 모든 stageData를 모아서 전역 stages 리스트를 갱신
    /// </summary>
    public void RefreshStageList()
    {
        stages.Clear();
        foreach (var region in regions)
        {
            if (region != null && region.stages != null)
            {
                stages.AddRange(region.stages);
            }
        }
        Debug.Log($"[StageManager] 전역 스테이지 리스트 갱신 완료. 총 {stages.Count}개 스테이지");
    }

    /// <summary>
    /// 특정 스테이지 클리어 처리
    /// </summary>
    public void CompleteStage(string stageId)
    {
        var (region, stageIndex) = FindStage(stageId);

        if (region == null || stageIndex < 0) return;

        var stage = region.stages[stageIndex];
        Debug.Log($"[StageManager] 스테이지 클리어: {stage.stageName}");

        // 다음 스테이지 해금
        if (stageIndex + 1 < region.stages.Count)
        {
            var nextStage = region.stages[stageIndex + 1];
            nextStage.isUnlocked = true;
            Debug.Log($"[StageManager] 다음 스테이지 해금: {nextStage.stageName}");
        }
        else
        {
            // 현재 영지의 마지막 스테이지를 클리어했을 때 → 다음 영지 해금
            var currentRegionIndex = regions.IndexOf(region);
            if (currentRegionIndex + 1 < regions.Count)
            {
                var nextRegion = regions[currentRegionIndex + 1];
                if (nextRegion.stages.Count > 0)
                {
                    nextRegion.stages[0].isUnlocked = true;
                    Debug.Log($"[StageManager] 다음 영지 해금: {nextRegion.regionName} - {nextRegion.stages[0].stageName}");
                }
            }
        }

        // [NEW] 클리어 후 리스트 갱신 (unlock 상태 반영)
        RefreshStageList();
    }

    /// <summary>
    /// 스테이지 ID로 스테이지와 소속 영지 찾기
    /// </summary>
    private (RegionData, int) FindStage(string stageId)
    {
        foreach (var region in regions)
        {
            for (int i = 0; i < region.stages.Count; i++)
            {
                if (region.stages[i].stageId == stageId)
                    return (region, i);
            }
        }
        return (null, -1);
    }

    /// <summary>
    /// 스테이지 입장 처리
    /// </summary>
    public void EnterStage(string stageId)
    {
        // [변경] 전역 리스트 갱신을 보장하고 검색
        if (stages == null || stages.Count == 0) RefreshStageList();

        StageData stage = stages.Find(s => s.stageId == stageId);
        if (stage == null)
        {
            Debug.LogError($"[StageManager] StageId '{stageId}'를 찾을 수 없음");
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.lastPortalID = null; // [추가] 포탈 위치 강제 비활성화
        pendingSpawnPos = stage.spawnPosition;    // 아까 추가했던 스폰 포인트 사용

        SceneManager.sceneLoaded += OnSceneLoaded_SetPlayerPos;
        SceneManager.LoadScene(stage.sceneName);

        Debug.Log($"[StageManager] EnterStage 호출됨 → {stage.stageName}");

        // 실제 씬 로드 (StageData에 sceneName 추가 필요)
        if (!string.IsNullOrEmpty(stage.sceneName))
        {
            SceneManager.LoadScene(stage.sceneName);
        }
        else
        {
            Debug.LogWarning($"[StageManager] {stage.stageName}에 sceneName이 설정되지 않음");
        }
    }

    private void OnSceneLoaded_SetPlayerPos(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded_SetPlayerPos;

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = pendingSpawnPos;
            Debug.Log($"[StageManager] Player 위치를 {pendingSpawnPos} 로 이동");

            // [NEW] 입력과 애니메이션 복구
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.canControl = true;
                controller.canMove = true;
            }
            var anim = player.GetComponent<Animator>();
            if (anim != null) anim.speed = 1f;
        }
    }
    public void ReturnToTown()
    {
        Debug.Log("[StageManager] 마을로 귀환");
        SceneManager.LoadScene("Town");
    }
}

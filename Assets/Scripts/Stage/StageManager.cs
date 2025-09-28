using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Regions & Stages")]
    public List<RegionData> regions = new List<RegionData>();
    public List<StageData> stages = new List<StageData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        StageData stage = stages.Find(s => s.stageId == stageId);
        if (stage == null)
        {
            Debug.LogError($"[StageManager] StageId '{stageId}'를 찾을 수 없음");
            return;
        }

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
}

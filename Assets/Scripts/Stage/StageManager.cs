using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Regions & Stages")]
    public List<RegionData> regions = new List<RegionData>();
    public List<StageData> stages = new List<StageData>();
    private Vector3 pendingSpawnPos = Vector3.zero;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void RefreshStageList()
    {
        stages.Clear();
        foreach (var region in regions)
        {
            if (region != null && region.stages != null)
                stages.AddRange(region.stages);
        }
        Debug.Log($"[StageManager] ���� �������� ����Ʈ ���� �Ϸ�. �� {stages.Count}�� ��������");
    }

    public void CompleteStage(string stageId)
    {
        var (region, stageIndex) = FindStage(stageId);
        if (region == null || stageIndex < 0) return;

        var stage = region.stages[stageIndex];
        Debug.Log($"[StageManager] �������� Ŭ����: {stage.stageName}");

        if (stageIndex + 1 < region.stages.Count)
        {
            var nextStage = region.stages[stageIndex + 1];
            nextStage.isUnlocked = true;
            Debug.Log($"[StageManager] ���� �������� �ر�: {nextStage.stageName}");
        }
        else
        {
            var currentRegionIndex = regions.IndexOf(region);
            if (currentRegionIndex + 1 < regions.Count)
            {
                var nextRegion = regions[currentRegionIndex + 1];
                if (nextRegion.stages.Count > 0)
                {
                    nextRegion.stages[0].isUnlocked = true;
                    Debug.Log($"[StageManager] ���� ���� �ر�: {nextRegion.regionName} - {nextRegion.stages[0].stageName}");
                }
            }
        }

        RefreshStageList();
    }

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

    public void EnterStage(string stageId)
    {
        if (stages == null || stages.Count == 0) RefreshStageList();

        StageData stage = stages.Find(s => s.stageId == stageId);
        if (stage == null)
        {
            Debug.LogError($"[StageManager] StageId '{stageId}'�� ã�� �� ����");
            return;
        }

        // StageSelect �� Stage ������ �׻� PlayerSpawnPoint ����
        GameManager.Instance.BeginTransition(TransitionKind.FromStageSelect, stage.sceneName, null, "PlayerSpawnPoint");

        pendingSpawnPos = stage.spawnPosition;

        SceneManager.LoadScene(stage.sceneName);
        Debug.Log($"[StageManager] EnterStage ȣ��� �� {stage.stageName}");
    }

    public void ReturnToTown()
    {
        Debug.Log("[StageManager] ������ ��ȯ");
        // ���� Ŭ���� �� ��ȯ�� �׻� Town�� ReturnPoint ����
        GameManager.Instance.BeginTransition(TransitionKind.ReturnToTown, "Town", null, "ReturnPoint");
        SceneManager.LoadScene("Town");
    }
}

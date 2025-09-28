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
    /// Ư�� �������� Ŭ���� ó��
    /// </summary>
    public void CompleteStage(string stageId)
    {
        var (region, stageIndex) = FindStage(stageId);

        if (region == null || stageIndex < 0) return;

        var stage = region.stages[stageIndex];
        Debug.Log($"[StageManager] �������� Ŭ����: {stage.stageName}");

        // ���� �������� �ر�
        if (stageIndex + 1 < region.stages.Count)
        {
            var nextStage = region.stages[stageIndex + 1];
            nextStage.isUnlocked = true;
            Debug.Log($"[StageManager] ���� �������� �ر�: {nextStage.stageName}");
        }
        else
        {
            // ���� ������ ������ ���������� Ŭ�������� �� �� ���� ���� �ر�
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
    }

    /// <summary>
    /// �������� ID�� ���������� �Ҽ� ���� ã��
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
    /// �������� ���� ó��
    /// </summary>
    public void EnterStage(string stageId)
    {
        StageData stage = stages.Find(s => s.stageId == stageId);
        if (stage == null)
        {
            Debug.LogError($"[StageManager] StageId '{stageId}'�� ã�� �� ����");
            return;
        }

        Debug.Log($"[StageManager] EnterStage ȣ��� �� {stage.stageName}");

        // ���� �� �ε� (StageData�� sceneName �߰� �ʿ�)
        if (!string.IsNullOrEmpty(stage.sceneName))
        {
            SceneManager.LoadScene(stage.sceneName);
        }
        else
        {
            Debug.LogWarning($"[StageManager] {stage.stageName}�� sceneName�� �������� ����");
        }
    }
}

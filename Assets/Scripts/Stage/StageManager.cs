using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Regions & Stages")]
    public List<RegionData> regions = new List<RegionData>();
    public List<StageData> stages = new List<StageData>(); // [NOTE] ���� �������� ����Ʈ (�ڵ� ����ȭ��)
    private Vector3 pendingSpawnPos = Vector3.zero; // [NEW] ���� ������ �� ��ġ


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // [NEW] �� ��ȯ���� ��Ƴ���
        }
        else
        {
            Destroy(gameObject);
        }
   
    }

    /// <summary>
    /// [NEW] regions ���� ��� stageData�� ��Ƽ� ���� stages ����Ʈ�� ����
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
        Debug.Log($"[StageManager] ���� �������� ����Ʈ ���� �Ϸ�. �� {stages.Count}�� ��������");
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

        // [NEW] Ŭ���� �� ����Ʈ ���� (unlock ���� �ݿ�)
        RefreshStageList();
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
        // [����] ���� ����Ʈ ������ �����ϰ� �˻�
        if (stages == null || stages.Count == 0) RefreshStageList();

        StageData stage = stages.Find(s => s.stageId == stageId);
        if (stage == null)
        {
            Debug.LogError($"[StageManager] StageId '{stageId}'�� ã�� �� ����");
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.lastPortalID = null; // [�߰�] ��Ż ��ġ ���� ��Ȱ��ȭ
        pendingSpawnPos = stage.spawnPosition;    // �Ʊ� �߰��ߴ� ���� ����Ʈ ���

        SceneManager.sceneLoaded += OnSceneLoaded_SetPlayerPos;
        SceneManager.LoadScene(stage.sceneName);

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

    private void OnSceneLoaded_SetPlayerPos(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded_SetPlayerPos;

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = pendingSpawnPos;
            Debug.Log($"[StageManager] Player ��ġ�� {pendingSpawnPos} �� �̵�");

            // [NEW] �Է°� �ִϸ��̼� ����
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
        Debug.Log("[StageManager] ������ ��ȯ");
        SceneManager.LoadScene("Town");
    }
}

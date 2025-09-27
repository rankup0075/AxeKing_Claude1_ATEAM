using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<QuestData> allQuests = new List<QuestData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<QuestData> GetAllQuests()
    {
        return allQuests;
    }

    // ��������Ʈ Ȯ��
    public bool IsQuestCompleted(string questId)
    {
        var quest = allQuests.Find(q => q.questId == questId);
        return quest != null && quest.isCompleted;
    }

    public void AcceptQuest(string questId)
    {
        var quest = allQuests.Find(q => q.questId == questId);
        if (quest != null)
        {
            quest.isAccepted = true;
            Debug.Log($"����Ʈ ����: {quest.questName}");
            UpdateQuestProgress(); // ���� �� �ٷ� ���൵ Ȯ��
        }
    }

    // �κ��丮 ������� ���൵ ������Ʈ
    public void UpdateQuestProgress()
    {
        var playerInv = FindFirstObjectByType<PlayerInventory>();
        if (playerInv == null) return;

        foreach (var quest in allQuests)
        {
            if (quest.isAccepted && !quest.isCompleted)
            {
                // �κ��丮���� ������ ���� Ȯ��
                quest.currentProgress = playerInv.GetItemCount(quest.requiredItemName);
            }
        }
    }

    public void CompleteQuest(string questId)
    {
        var quest = allQuests.Find(q => q.questId == questId);
        if (quest != null && quest.isAccepted && !quest.isCompleted)
        {
            var playerInv = FindFirstObjectByType<PlayerInventory>();
            if (playerInv != null &&
                playerInv.GetItemCount(quest.requiredItemName) >= quest.targetProgress)
            {
                // �䱸 ������ �ݳ�
                playerInv.RemoveItem(quest.requiredItemName, quest.targetProgress);

                // ���� ������Ʈ
                quest.isAccepted = false;
                quest.isCompleted = true;

                // ���� ���� (GameManager ���ؼ�)
                GameManager.Instance.AddGold(quest.rewardGold);

                Debug.Log($"����Ʈ �Ϸ�: {quest.questName} (���� {quest.rewardGold:N0} ���)");

                // ����Ʈ���� UI ���� (��� �ؽ�Ʈ ����)
                if (QuestBoardUI.Instance != null)
                    QuestBoardUI.Instance.RefreshUI();
            }
            else
            {
                Debug.LogWarning($"[Quest] {quest.questName} �Ϸ� ���� - ������ ����");
            }
        }
    }

}

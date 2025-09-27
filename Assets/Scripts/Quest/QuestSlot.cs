using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestSlotUI : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI rewardText;
    public Button actionButton;
    public Image slotBackground;   //���� ��ü ��� (Inspector���� ����)

    private QuestData questData;
    private Color normalColor;

    void Awake()
    {
        if (slotBackground != null)
            normalColor = slotBackground.color;
    }

    public void Setup(QuestData quest)
    {
        questData = quest;
        questNameText.text = quest.questName;
        questDescText.text = quest.description;
        rewardText.text = $"����: {quest.rewardGold:N0} Gold";

        actionButton.onClick.RemoveAllListeners();

        if (quest.isCompleted)
        {
            actionButton.gameObject.SetActive(false);
            progressText.gameObject.SetActive(true);
            progressText.text = $"�Ϸ�� {quest.targetProgress}/{quest.targetProgress}";

            if (slotBackground != null)
                slotBackground.color = Color.gray;
        }
        else if (quest.isAccepted)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = $"������ : {quest.currentProgress}/{quest.targetProgress}";

            if (quest.currentProgress >= quest.targetProgress)
            {
                actionButton.gameObject.SetActive(true);
                actionButton.interactable = true;
                actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "�Ϸ��ϱ�";
                actionButton.onClick.AddListener(() => OnComplete());
            }
            else
            {
                actionButton.gameObject.SetActive(true);
                actionButton.interactable = false;
                actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "�Ϸ��ϱ�";
            }

            if (slotBackground != null)
                slotBackground.color = normalColor;
        }
        else
        {
            progressText.gameObject.SetActive(false);
            actionButton.gameObject.SetActive(true);

            bool canAccept = string.IsNullOrEmpty(quest.prerequisiteQuestId)
                             || QuestManager.Instance.IsQuestCompleted(quest.prerequisiteQuestId);

            actionButton.interactable = canAccept;
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "�����ϱ�";
            actionButton.onClick.AddListener(() => OnAccept());

            if (slotBackground != null)
                slotBackground.color = canAccept ? normalColor : Color.gray; //���� �� �Ǹ� ȸ��
        }
    }

    private void OnAccept()
    {
        QuestManager.Instance.AcceptQuest(questData.questId);
        Setup(questData);
    }

    private void OnComplete()
    {
        QuestManager.Instance.CompleteQuest(questData.questId);

        // ���� ��Ȱ��ȭ ó��
        if (slotBackground != null)
            slotBackground.color = Color.gray;

        actionButton.gameObject.SetActive(false);
        progressText.text = $"�Ϸ�� {questData.targetProgress}/{questData.targetProgress}";

        // ���� ��ü ���ΰ�ħ (���� ����Ʈ Ȱ��ȭ �ݿ�)
        QuestBoardUI.Instance.RefreshUI();
    }

}

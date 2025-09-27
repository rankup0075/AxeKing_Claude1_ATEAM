using UnityEngine;
using TMPro;

public class QuestBoardUI : MonoBehaviour
{
    public static QuestBoardUI Instance;

    public Transform questListContainer;
    public GameObject questSlotPrefab;
    public TextMeshProUGUI emptyText;

    [Header("Player Info")]
    public TextMeshProUGUI currentGoldText; // ���� ��� ǥ��

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in questListContainer)
        {
            Destroy(child.gameObject);
        }

        var quests = QuestManager.Instance.GetAllQuests();
        bool hasQuests = false;

        foreach (var quest in quests)
        {
            var slot = Instantiate(questSlotPrefab, questListContainer);
            slot.GetComponent<QuestSlotUI>().Setup(quest);
            hasQuests = true;
        }

        emptyText.gameObject.SetActive(!hasQuests);

        // ���� ��� ����
        if (currentGoldText != null && GameManager.Instance != null)
            currentGoldText.text = $"���� ���: {GameManager.Instance.Gold:NO}G";
    }
}

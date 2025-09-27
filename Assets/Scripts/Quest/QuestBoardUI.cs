using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestBoardUI : MonoBehaviour
{
    public static QuestBoardUI Instance;

    public Transform questListContainer;
    public GameObject questSlotPrefab;
    public TextMeshProUGUI emptyText;

    public ScrollRect scrollRect;

    [Header("Player Info")]
    public TextMeshProUGUI currentGoldText; // ���� ��� ǥ��

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        RefreshUI();

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases(); // ���̾ƿ� ���� ����
            scrollRect.verticalNormalizedPosition = 1f; // �� ��
            Canvas.ForceUpdateCanvases(); // ���̾ƿ� ���� ����
        }
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
            currentGoldText.text = $"���� ���: {GameManager.Instance.Gold:N0}G";
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}

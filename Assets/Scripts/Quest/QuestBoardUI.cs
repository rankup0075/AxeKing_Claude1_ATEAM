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
    public TextMeshProUGUI currentGoldText; // 현재 골드 표시

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        RefreshUI();

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases(); // 레이아웃 강제 갱신
            scrollRect.verticalNormalizedPosition = 1f; // 맨 위
            Canvas.ForceUpdateCanvases(); // 레이아웃 강제 갱신
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

        // 현재 골드 갱신
        if (currentGoldText != null && GameManager.Instance != null)
            currentGoldText.text = $"현재 골드: {GameManager.Instance.Gold:N0}G";
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}

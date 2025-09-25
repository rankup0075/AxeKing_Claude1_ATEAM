//using System.Collections.Generic;
//using UnityEngine;

//public class GameQuestManager : MonoBehaviour
//{
//    public static GameQuestManager Instance;

//    [System.Serializable]
//    public class Quest
//    {
//        public int questId;
//        public string questName;
//        public string requiredItem;
//        public int requiredAmount;
//        public int goldReward;
//        public QuestStatus status;
//    }

//    public enum QuestStatus
//    {
//        Available,
//        Accepted,
//        Completed,
//        Claimed
//    }

//    public List<Quest> quests = new List<Quest>();

//    void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//            InitializeQuests();
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void InitializeQuests()
//    {
//        quests = new List<Quest>
//        {
//            new Quest { questId = 1, questName = "°íºí¸° Åä¹ú 1", requiredItem = "°íºí¸°ÀÇ °¡Á×", requiredAmount = 30, goldReward = 400, status = QuestStatus.Available },
//            new Quest { questId = 2, questName = "°íºí¸° Åä¹ú 2", requiredItem = "°íºí¸°ÀÇ °¡Á×", requiredAmount = 70, goldReward = 700, status = QuestStatus.Available },
//            new Quest { questId = 3, questName = "°ñ·½ Ã³Ä¡ 1", requiredItem = "°ñ·½ÀÇ ÆÄÆí", requiredAmount = 50, goldReward = 1400, status = QuestStatus.Available },
//            new Quest { questId = 4, questName = "°ñ·½ Ã³Ä¡ 2", requiredItem = "°ñ·½ÀÇ ÆÄÆí", requiredAmount = 70, goldReward = 2000, status = QuestStatus.Available },
//            new Quest { questId = 5, questName = "È­¿°Á¤·É Åä¹ú 1", requiredItem = "È­¿°±¸½½", requiredAmount = 50, goldReward = 2500, status = QuestStatus.Available },
//            new Quest { questId = 6, questName = "È­¿°Á¤·É Åä¹ú 2", requiredItem = "È­¿°±¸½½", requiredAmount = 80, goldReward = 3200, status = QuestStatus.Available },
//            new Quest { questId = 7, questName = "¾óÀ½Á¤·É Åä¹ú 1", requiredItem = "´«¹°Á¶°¢", requiredAmount = 50, goldReward = 4000, status = QuestStatus.Available },
//            new Quest { questId = 8, questName = "¾óÀ½Á¤·É Åä¹ú 2", requiredItem = "´«¹°Á¶°¢", requiredAmount = 50, goldReward = 4500, status = QuestStatus.Available },
//            new Quest { questId = 9, questName = "¾ÏÈæ¸¶¹ý»ç Åä¹ú 1", requiredItem = "Âõ¾îÁø °í¼­", requiredAmount = 50, goldReward = 5000, status = QuestStatus.Available },
//            new Quest { questId = 10, questName = "¾ÏÈæ¸¶¹ý»ç Åä¹ú 2", requiredItem = "Âõ¾îÁø °í¼­", requiredAmount = 100, goldReward = 5500, status = QuestStatus.Available }
//        };
//    }

//    public void AcceptQuest(int questId)
//    {
//        Quest quest = quests.Find(q => q.questId == questId);
//        if (quest != null && quest.status == QuestStatus.Available)
//        {
//            quest.status = QuestStatus.Accepted;
//        }
//    }

//    public void CompleteQuest(int questId)
//    {
//        Quest quest = quests.Find(q => q.questId == questId);
//        if (quest != null && quest.status == QuestStatus.Accepted)
//        {
//            PlayerInventory inventory = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();

//            if (inventory.HasItem(quest.requiredItem, quest.requiredAmount))
//            {
//                inventory.RemoveItem(quest.requiredItem, quest.requiredAmount);
//                GameManager.Instance.AddGold(quest.goldReward);
//                quest.status = QuestStatus.Completed;

//                Debug.Log($"Äù½ºÆ® ¿Ï·á: {quest.questName}, º¸»ó: {quest.goldReward}°ñµå");
//            }
//        }
//    }

//    public bool CanCompleteQuest(int questId)
//    {
//        Quest quest = quests.Find(q => q.questId == questId);
//        if (quest == null || quest.status != QuestStatus.Accepted) return false;

//        PlayerInventory inventory = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
//        return inventory.HasItem(quest.requiredItem, quest.requiredAmount);
//    }
//}
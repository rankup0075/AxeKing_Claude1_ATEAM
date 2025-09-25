//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class StageManager : MonoBehaviour
//{
//    public static StageManager Instance;

//    [System.Serializable]
//    public class StageData
//    {
//        public int territoryId;
//        public int stageId;
//        public int roundId;
//        public bool isUnlocked;
//        public bool isCleared;
//        public List<GameObject> enemyPrefabs;
//        public int enemyCount;
//        public bool isBossStage;
//        public GameObject bossPrefab;
//    }

//    [Header("Current Stage")]
//    public StageData currentStage;

//    [Header("Enemy Spawn")]
//    public Transform[] spawnPoints;
//    public List<GameObject> activeEnemies = new List<GameObject>();

//    private int enemiesDefeated = 0;

//    void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void Start()
//    {
//        StartStage();
//    }

//    public void StartStage()
//    {
//        enemiesDefeated = 0;
//        activeEnemies.Clear();
//        SpawnEnemies();
//    }

//    void SpawnEnemies()
//    {
//        if (currentStage.isBossStage && currentStage.bossPrefab != null)
//        {
//            // 보스 스폰
//            GameObject boss = Instantiate(currentStage.bossPrefab, spawnPoints[0].position, Quaternion.identity);
//            activeEnemies.Add(boss);

//            // 보스 체력바 표시
//            UIManager.Instance.ShowBossHealthBar(boss.GetComponent<EnemyHealth>());
//        }
//        else
//        {
//            // 일반 적 스폰
//            for (int i = 0; i < currentStage.enemyCount; i++)
//            {
//                if (currentStage.enemyPrefabs.Count > 0)
//                {
//                    GameObject enemyPrefab = currentStage.enemyPrefabs[Random.Range(0, currentStage.enemyPrefabs.Count)];
//                    Transform spawnPoint = spawnPoints[i % spawnPoints.Length];

//                    GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
//                    activeEnemies.Add(enemy);

//                    // 적 사망 이벤트 연결
//                    EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
//                    if (enemyHealth != null)
//                    {
//                        enemyHealth.OnDeath.AddListener(OnEnemyDefeated);
//                    }
//                }
//            }
//        }
//    }

//    public void OnEnemyDefeated()
//    {
//        enemiesDefeated++;

//        if (enemiesDefeated >= currentStage.enemyCount || (currentStage.isBossStage && enemiesDefeated >= 1))
//        {
//            CompleteStage();
//        }
//    }

//    void CompleteStage()
//    {
//        currentStage.isCleared = true;

//        // 다음 스테이지 잠금 해제
//        UnlockNextStage();

//        // 포탈 활성화
//        ActivateExitPortal();

//        // 완료 UI 표시
//        UIManager.Instance.ShowStageCompleteUI();

//        Debug.Log($"스테이지 완료: {currentStage.territoryId}-{currentStage.stageId}-{currentStage.roundId}");
//    }

//    void UnlockNextStage()
//    {
//        // 다음 라운드/스테이지/영지 잠금 해제 로직
//        GameManager.Instance.UnlockStage(currentStage.territoryId, currentStage.stageId, currentStage.roundId + 1);
//    }

//    void ActivateExitPortal()
//    {
//        GameObject exitPortal = GameObject.FindWithTag("ExitPortal");
//        if (exitPortal != null)
//        {
//            Portal portal = exitPortal.GetComponent<Portal>();
//            if (portal != null)
//            {
//                portal.isUnlocked = true;
//            }
//        }
//    }

//    public void LoadStage(int territoryId, int stageId, int roundId)
//    {
//        // 스테이지 데이터 로드 후 씬 전환
//        string sceneName = $"Stage_{territoryId}_{stageId}_{roundId}";
//        SceneManager.LoadScene(sceneName);
//    }
//}
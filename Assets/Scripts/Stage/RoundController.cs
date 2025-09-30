using UnityEngine;

public class RoundController : MonoBehaviour
{
    [Header("���� ����")]
    [Tooltip("GameManager���� ���� ���¸� ������ ���� ID (��: Stage101_R1)")]
    public string roundId;

    [Tooltip("�� ���忡 �����ϴ� ������ ���� ��ġ �� �巡�� ���")]
    public GameObject[] enemies;

    [Tooltip("���� Ŭ���� �� Ȱ��ȭ�� �ⱸ ��Ż")]
    public Portal exitPortal;

    private int aliveCount;

    void Start()
    {
        // �̹� Ŭ����� ������ �� ��Ż ��� Ȱ��ȭ
        if (exitPortal != null)
        {
            bool cleared = GameManager.Instance.IsRoundCleared(roundId);
            exitPortal.SetActiveState(cleared);
        }

        // �� ���� ī��Ʈ
        aliveCount = enemies.Length;

        // �� ���� �̺�Ʈ ����
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var ec = enemy.GetComponent<EnemyController>();
            if (ec != null)
            {
                // �������� ���ο� ���� ����
                if (ec.isBoss)
                    ec.onDeath += OnBossDeath;
                else
                    ec.onDeath += OnEnemyDeath;
            }
        }
    }

    void OnEnemyDeath()
    {
        aliveCount--;
        if (aliveCount <= 0) ClearRound();
    }

    void OnBossDeath()
    {
        // ���� óġ ���
        GameManager.Instance.SetBossDefeated(roundId);
        aliveCount--;
        if (aliveCount <= 0) ClearRound();
    }

    void ClearRound()
    {
        if (exitPortal != null)
        {
            exitPortal.SetActiveState(true);
            GameManager.Instance.SetRoundCleared(roundId);
            Debug.Log($"[RoundController] {roundId} Ŭ���� �� ��Ż Ȱ��ȭ");
        }
    }
}

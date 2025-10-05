using UnityEngine;

// ���� ���� Health : Die() ������ BossController�� ����
public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    public int goldDrop = 0;

    private BossController bossController;

    void Awake()
    {
        currentHP = maxHP;
        bossController = GetComponent<BossController>();
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;
        currentHP -= dmg;
        currentHP = Mathf.Max(0, currentHP);
        Debug.Log($"{name} takes {dmg} dmg -> {currentHP}/{maxHP}");

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        // ������ ��� �ı����� ����. BossController���� ����.
        if (bossController != null)
            bossController.OnPhaseEnded();
        else
        {
            // ������: ���� �� �׳� �ı�
            Debug.LogWarning("[BossHealth] BossController missing. Destroying object.");
            Destroy(gameObject);
        }
    }

    // �ܺο��� ������ ���۽� ü�� ����
    public void ResetTo(int hp)
    {
        maxHP = hp;
        currentHP = maxHP;
    }
}

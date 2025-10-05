using UnityEngine;

// 보스 전용 Health : Die() 동작을 BossController에 위임
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
        // 보스는 즉시 파괴하지 않음. BossController에게 위임.
        if (bossController != null)
            bossController.OnPhaseEnded();
        else
        {
            // 안전망: 실패 시 그냥 파괴
            Debug.LogWarning("[BossHealth] BossController missing. Destroying object.");
            Destroy(gameObject);
        }
    }

    // 외부에서 페이즈 시작시 체력 리셋
    public void ResetTo(int hp)
    {
        maxHP = hp;
        currentHP = maxHP;
    }
}

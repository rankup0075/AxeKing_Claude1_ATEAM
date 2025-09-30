using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHP = 10;
    private int currentHP;

    [Header("보상")]
    public int goldDrop = 5;

    private EnemyController controller;

    void Awake()
    {
        currentHP = maxHP;
        controller = GetComponent<EnemyController>();
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        Debug.Log($"{name} {dmg} 피해 → 남은 HP {currentHP}");

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        // 골드 드랍, 체력바 UI 갱신 등
        Debug.Log($"{name} 죽음 → {goldDrop}골드 드랍");

        // EnemyController에게 죽음을 알림
        if (controller != null)
            controller.OnDeath();
    }
}

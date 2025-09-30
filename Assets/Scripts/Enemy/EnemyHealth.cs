using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("ü�� ����")]
    public int maxHP = 10;
    private int currentHP;

    [Header("����")]
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
        Debug.Log($"{name} {dmg} ���� �� ���� HP {currentHP}");

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        // ��� ���, ü�¹� UI ���� ��
        Debug.Log($"{name} ���� �� {goldDrop}��� ���");

        // EnemyController���� ������ �˸�
        if (controller != null)
            controller.OnDeath();
    }
}

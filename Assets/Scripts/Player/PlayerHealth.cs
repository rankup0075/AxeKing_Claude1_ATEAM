using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);

        // [NEW] HUD�� ����ȭ
        UIManager.Instance.UpdateHUDHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"[PlayerHealth] Player took {damage} damage �� HP {CurrentHealth}/{MaxHealth}");
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);

        // [NEW] HUD�� ����
        UIManager.Instance.UpdateHUDHealth(currentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        if (currentHealth >= maxHealth) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);

        // [NEW] HUD�� ����
        UIManager.Instance.UpdateHUDHealth(currentHealth, maxHealth);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // ��� ����� ���� ü�µ� ����
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);

        // [NEW] HUD�� ����
        UIManager.Instance.UpdateHUDHealth(currentHealth, maxHealth);
    }
}

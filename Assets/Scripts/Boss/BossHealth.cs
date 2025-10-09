using UnityEngine;
using System;

[DisallowMultipleComponent]
public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 300;
    public int currentHP;

    public event Action OnBossDeath;

    void Awake() => currentHP = maxHP;

    public void TakeDamage(int dmg)
    {
        currentHP = Mathf.Max(0, currentHP - dmg);
        Debug.Log($"[BossHealth] {gameObject.name} took {dmg} damage �� HP {currentHP}/{maxHP}");
        if (currentHP == 0) Die();
    }

    void Die()
    {
        // �ݶ��̴� ���� �ִϸ��̼� ��ȯ�� �� ������ ��Ʈ�ѷ����� ó��
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        OnBossDeath?.Invoke();
        enabled = false;
    }
}

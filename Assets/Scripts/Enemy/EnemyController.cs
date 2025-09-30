using UnityEngine;
using System;

public class EnemyController : MonoBehaviour
{
    [Header("���� ����")]
    public bool isBoss = false;

    public event Action onDeath;

    // EnemyHealth�� ����� ȣ��
    public void OnDeath()
    {
        Debug.Log($"{name} ��� ó�� (RoundController�� �˸�)");
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}

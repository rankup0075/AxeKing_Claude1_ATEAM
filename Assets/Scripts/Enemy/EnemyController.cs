using UnityEngine;
using System;

public class EnemyController : MonoBehaviour
{
    [Header("보스 여부")]
    public bool isBoss = false;

    public event Action onDeath;

    // EnemyHealth가 사망시 호출
    public void OnDeath()
    {
        Debug.Log($"{name} 사망 처리 (RoundController에 알림)");
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}

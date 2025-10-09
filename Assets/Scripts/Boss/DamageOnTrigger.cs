// DamageOnTrigger.cs
using UnityEngine;

public class DamageOnTrigger : MonoBehaviour
{
    public int damage = 10;
    public bool continuous = false;      // true = ����ִ� ���� �ֱ��� ������
    public float tickInterval = 0.2f;    // continuous�� �� ����
    public bool destroyOnHit = false;    // �ܹ������� ������ �ı�

    private float lastTickTime = -999f;

    void OnTriggerStay(Collider other)
    {
        if (!continuous) return;
        if (!other.TryGetComponent<PlayerHealth>(out var ph)) return;

        if (Time.time - lastTickTime >= tickInterval)
        {
            ph.TakeDamage(damage);
            lastTickTime = Time.time;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (continuous) return;
        if (!other.TryGetComponent<PlayerHealth>(out var ph)) return;

        ph.TakeDamage(damage);
        if (destroyOnHit) Destroy(gameObject);
    }
}

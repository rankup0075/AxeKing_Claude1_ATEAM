using UnityEngine;
using System.Collections;

public class DamageArea : MonoBehaviour
{
    public float damage = 10f;
    public bool continuous = false;
    public float tickInterval = 0.2f;
    public bool oneShotPerEnter = false;
    public float life = 1f;
    public LayerMask targetLayer;

    private void OnEnable()
    {
        if (life > 0) Destroy(gameObject, life);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!oneShotPerEnter && !continuous) DoHit(other);
        if (oneShotPerEnter) DoHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!continuous) return;
        if (isActiveAndEnabled) StartCoroutine(Tick(other));
    }

    IEnumerator Tick(Collider col)
    {
        enabled = false;
        DoHit(col);
        yield return new WaitForSeconds(tickInterval);
        enabled = true;
    }

    void DoHit(Collider col)
    {
        if (((1 << col.gameObject.layer) & targetLayer.value) == 0) return;
        var d = col.GetComponentInParent<IDamageable>();
        if (d != null) d.ApplyDamage(damage);
    }
}

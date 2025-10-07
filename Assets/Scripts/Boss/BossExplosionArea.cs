using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class BossExplosionArea : MonoBehaviour
{
    CapsuleCollider col;
    LayerMask playerLayer;
    int tickDamage;
    float tickInterval;

    public void Activate(float radius, float height, int tickDamage, float tickInterval, float life, LayerMask playerLayer)
    {
        this.playerLayer = playerLayer;
        this.tickDamage = tickDamage;
        this.tickInterval = tickInterval;

        col = GetComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.radius = radius;
        col.height = height;
        col.center = new Vector3(0f, height * 0.5f, 0f);
        StartCoroutine(TickLoop());
        Destroy(gameObject, life);
    }

    IEnumerator TickLoop()
    {
        var w = new WaitForSeconds(tickInterval);
        while (true)
        {
            Collider[] hits = Physics.OverlapCapsule(
                transform.position + Vector3.up * col.radius,
                transform.position + Vector3.up * (col.height - col.radius),
                col.radius, playerLayer);

            foreach (var h in hits)
            {
                var hp = h.GetComponent<PlayerHealth>();
                if (hp != null) hp.TakeDamage(tickDamage);
            }
            yield return w;
        }
    }
}

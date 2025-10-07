using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float speed = 14f;
    public float life = 5f;
    public float damage = 10f;
    public LayerMask hitLayers;
    public bool destroyOnAnyHit = true;
    public GameObject hitVfx;

    Vector3 _dir;

    public void Fire(Vector3 dir)
    {
        _dir = dir.normalized;
        Destroy(gameObject, life);
    }

    void Update()
    {
        transform.position += _dir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitLayers.value) == 0) return;

        var d = other.GetComponentInParent<IDamageable>();
        if (d != null) d.ApplyDamage(damage);

        if (hitVfx) Instantiate(hitVfx, transform.position, Quaternion.identity);
        if (destroyOnAnyHit) Destroy(gameObject);
    }
}

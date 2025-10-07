using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class BossProjectile : MonoBehaviour
{
    public float life = 8f;
    public GameObject hitVfx;

    [Header("Collision")]
    public LayerMask environmentLayers;   // Ground 등

    Rigidbody rb;
    Collider myCol;
    Vector3 dir; float speed; int damage;
    LayerMask playerLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        myCol.isTrigger = true; // 플레이어는 Trigger로 판정
    }

    void OnEnable() => Destroy(gameObject, life);

    // ignoreThese: 보스 자신의 콜라이더들
    public void Launch(Vector3 dir, float speed, int damage, LayerMask playerLayer, Collider[] ignoreThese = null)
    {
        this.dir = dir.sqrMagnitude > 0f ? dir.normalized : Vector3.forward;
        this.speed = speed;
        this.damage = damage;
        this.playerLayer = playerLayer;

        // 보스와 초기가격 충돌 무시
        if (ignoreThese != null)
            foreach (var c in ignoreThese) if (c && myCol) Physics.IgnoreCollision(myCol, c, true);

        // 보스 콜라이더 밖으로 0.3m 밀어내기
        transform.position += this.dir * 0.3f;

        // 물리 이동
        rb.linearVelocity = this.dir * this.speed;
    }

    void Update()
    {
        // 안전장치: velocity가 0이 되지 않도록
        if (rb.linearVelocity.sqrMagnitude < 0.0001f) rb.linearVelocity = dir * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Hit {other.name} layer {other.gameObject.layer} mask {playerLayer.value}");
        int ol = other.gameObject.layer;

        // 플레이어 피격
        if ((playerLayer.value & (1 << ol)) != 0)
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            if (hitVfx) Instantiate(hitVfx, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        // 환경(바닥/벽 레이어) 접촉 시 파괴
        if ((environmentLayers.value & (1 << ol)) != 0)
        {
            if (hitVfx) Instantiate(hitVfx, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

    }
}

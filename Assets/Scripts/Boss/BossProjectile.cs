using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class BossProjectile : MonoBehaviour
{
    public float life = 8f;
    public GameObject hitVfx;

    [Header("Collision")]
    public LayerMask environmentLayers;   // Ground ��

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
        myCol.isTrigger = true; // �÷��̾�� Trigger�� ����
    }

    void OnEnable() => Destroy(gameObject, life);

    // ignoreThese: ���� �ڽ��� �ݶ��̴���
    public void Launch(Vector3 dir, float speed, int damage, LayerMask playerLayer, Collider[] ignoreThese = null)
    {
        this.dir = dir.sqrMagnitude > 0f ? dir.normalized : Vector3.forward;
        this.speed = speed;
        this.damage = damage;
        this.playerLayer = playerLayer;

        // ������ �ʱⰡ�� �浹 ����
        if (ignoreThese != null)
            foreach (var c in ignoreThese) if (c && myCol) Physics.IgnoreCollision(myCol, c, true);

        // ���� �ݶ��̴� ������ 0.3m �о��
        transform.position += this.dir * 0.3f;

        // ���� �̵�
        rb.linearVelocity = this.dir * this.speed;
    }

    void Update()
    {
        // ������ġ: velocity�� 0�� ���� �ʵ���
        if (rb.linearVelocity.sqrMagnitude < 0.0001f) rb.linearVelocity = dir * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Hit {other.name} layer {other.gameObject.layer} mask {playerLayer.value}");
        int ol = other.gameObject.layer;

        // �÷��̾� �ǰ�
        if ((playerLayer.value & (1 << ol)) != 0)
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            if (hitVfx) Instantiate(hitVfx, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        // ȯ��(�ٴ�/�� ���̾�) ���� �� �ı�
        if ((environmentLayers.value & (1 << ol)) != 0)
        {
            if (hitVfx) Instantiate(hitVfx, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

    }
}

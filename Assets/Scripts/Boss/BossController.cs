using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("����")]
    public Animator animator;
    public Transform firePoint;
    public LayerMask playerLayer;
    public EnemyHealth health;
    public Rigidbody rb;

    // �̵� ����
    [HideInInspector] public Vector3 pendingMove;

    [Header("������")]
    public Phase1Config phase1;

    Transform player;
    IBossState state;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!health) health = GetComponent<EnemyHealth>();
        if (!rb) rb = GetComponent<Rigidbody>();

        // === ���� �߰� ===
        // Rigidbody�� Kinematic�̸� �̵� �Ұ��ϹǷ� �ڵ� ����
        if (rb != null && rb.isKinematic)
            rb.isKinematic = false;
    }

    void Start()
    {
        player = PlayerController.Instance ? PlayerController.Instance.transform : null;
        SwitchToPhase1();
    }

    void Update()
    {
        state?.Tick();
    }

    // === ���� �߰� ===
    // ���� �̵� ó��
    void FixedUpdate()
    {
        Debug.Log("Boss Moving", this);

        if (rb == null) return;

        // pendingMove�� �����ϸ� ���� �̵� ����
        if (pendingMove.sqrMagnitude > 0f)
        {
            rb.MovePosition(rb.position + pendingMove);
            pendingMove = Vector3.zero;
        }

        // ���ʿ��� ȸ�� ���� (Y�� ȸ���� ���)
        Vector3 rot = rb.rotation.eulerAngles;
        rb.rotation = Quaternion.Euler(0f, rot.y, 0f);
    }

    public void SwitchToPhase1()
    {
        state?.Exit();
        state = new Phase1State(this, phase1);
        state.Enter();
    }

    public Transform Player => player != null ? player : (PlayerController.Instance ? PlayerController.Instance.transform : null);
    public bool HasPlayer => Player != null;

    public Vector3 GetPlayerAimPoint()
    {
        if (!HasPlayer) return Vector3.zero;
        var col = Player.GetComponent<Collider>();
        return col ? col.bounds.center : Player.position + Vector3.up;
    }

    // �ִϸ����� ����
    public void SetBool(string name, bool value) => animator?.SetBool(name, value);
    public void SetTrigger(string name) => animator?.SetTrigger(name);

    public void FacePlayerImmediate()
    {
        if (!HasPlayer) return;
        Vector3 d = Player.position - transform.position; d.y = 0f;
        if (d.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(d);
    }
}

public interface IBossState
{
    void Enter();
    void Tick();
    void Exit();
}

[System.Serializable]
public class Phase1Config
{
    [Header("�̵�")]
    [Range(0f, 1f)] public float chaseProbability = 0.6f;
    public float moveSpeed = 3f;
    public float decisionInterval = 2f;
    public float detectionRange = 20f;

    [Header("���� Ȯ��")]
    [Range(0f, 1f)] public float basicAttackWeight = 0.45f;
    [Range(0f, 1f)] public float skillA_ExplodeWeight = 0.3f;
    [Range(0f, 1f)] public float skillB_ProjectileWeight = 0.15f;
    [Range(0f, 1f)] public float skillC_LaserWeight = 0.1f;

    [Header("��ٿ�")]
    public Vector2 attackIntervalRange = new Vector2(4f, 7f);

    [Header("�⺻ ����(����ü)")]
    public GameObject basicProjectilePrefab;
    public float basicProjectileSpeed = 14f;
    public int basicProjectileDamage = 10;

    [Header("��ų A: ����(���� AOE)")]
    public GameObject aoePreviewPrefab;
    public GameObject aoeDamagePrefab;
    public float aoeTrackSeconds = 2f;
    public float aoeDelayAfterPreview = 1f;
    public float aoeRadius = 2.5f;
    public float aoeHeight = 5f;
    public int aoeTickDamage = 4;
    public float aoeTickInterval = 0.25f;
    public float aoeLife = 3f;

    [Header("��ų B: Ư�� ����ü")]
    public GameObject skillProjectilePrefab;
    public float skillProjectileSpeed = 18f;
    public int skillProjectileDamage = 16;

    [Header("��ų C: ���� ������")]
    public GameObject laserPreviewPrefab;
    public GameObject laserHitPrefab;
    public float laserTrackYSeconds = 2f;
    public float laserDelayAfterPreview = 1f;
    public float laserThickness = 1.2f;
    public int laserHitDamage = 20;
}

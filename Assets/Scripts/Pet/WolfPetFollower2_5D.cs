using UnityEngine;

/// <summary>
/// 2.5D(���̵��) ���� �� ������.
/// - X��(�¿�) �̵���
/// - ���� Y ȸ���� "������" �������� ���(�⺻: �� ȸ��) 180���� ��ȯ
/// - �÷��̾� Animator�� "Speed"(0/0.5/1)�� �켱 �е�, ������ �÷��̾� Transform�� ���� �ӵ��� �ȱ�/�޸��� ����
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WolfPetFollower2_5D : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("���� ���(���� PlayerRoot). ����θ� PlayerController.Instance�� 'Player' �±׸� �ڵ� Ž��")]
    public Transform target;

    [Header("Follow (Horizontal Only)")]
    public float followDistance = 1.6f;   // �� �̳��� ����
    public float desiredOffsetX = -1.0f;  // Ÿ�� ���� ���� ����(+X/-X)

    [Header("Speed (auto-switches with player's state)")]
    [Tooltip("�÷��̾ �ȴ� ���� �� ���� �ִ� �ӵ�")]
    public float walkFollowSpeed = 3.8f;
    [Tooltip("�÷��̾ �޸��� ���� �� ���� �ִ� �ӵ�")]
    public float runFollowSpeed = 6.0f;
    [Tooltip("����/����(m/s^2)")]
    public float accel = 12f;
    public float decel = 16f;

    [Header("Axis Locks")]
    public bool lockZ = true;
    public bool lockY = true;
    public float yLock = 0f;

    [Header("Facing")]
    [Tooltip("���� Y ȸ���� '������'���� ���(��: ������ 90�Ƹ� ��=90��, ��=270��)")]
    public bool useStartYawAsRight = true;
    [Tooltip("���� ������ ���� ���� �� ���� ����")]
    public float rightYaw = 0f;
    public float leftYaw = 180f;
    public float rotateSpeed = 1080f;     // deg/sec

    [Header("Sync With Player (fallback ready)")]
    [Tooltip("�÷��̾� Animator�� 'Speed'(0/0.5/1)�� ���� �õ�, ������ Transform �ӵ��� ����")]
    public bool matchPlayerSpeed = true;
    public string playerSpeedParam = "Speed";
    [Tooltip("�÷��̾� X�ӵ� ���� �ȱ�/�޸��� �Ӱ谪(�÷��̾� �Ķ���� ���� �� ���)")]
    public float playerWalkVxThreshold = 0.4f;
    public float playerRunVxThreshold = 3.0f;

    [Header("Safety")]
    public float snapIfFartherThan = 25f; // �ʹ� �ָ� ���� ����(0=off)

    private Rigidbody rb;
    private float vx;           // ���� X �ӵ�
    private float zLock;

    private PlayerController playerCtrl;
    private Animator playerAnim;
    private bool playerHasSpeedParam;

    // �÷��̾� �ӵ� ���� ��
    private float lastTargetX;

    // ���� �� ����� ���� ��/�� ����
    private float rightYawRT;
    private float leftYawRT;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = !lockY;

        // Ÿ��/�÷��̾� ����
        if (!target && PlayerController.Instance != null)
        {
            playerCtrl = PlayerController.Instance;
            target = playerCtrl.transform;
        }
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p)
            {
                target = p.transform;
                playerCtrl = p.GetComponent<PlayerController>();
            }
        }
        if (playerCtrl)
        {
            playerAnim = playerCtrl.GetComponent<Animator>();
            if (!playerAnim) playerAnim = playerCtrl.GetComponentInChildren<Animator>();
            playerHasSpeedParam = AnimatorHasParam(playerAnim, playerSpeedParam);
        }

        // �� ��� ����
        zLock = transform.position.z;
        if (lockY) yLock = transform.position.y;

        // ������/���� ���� ���� ����
        if (useStartYawAsRight)
        {
            rightYawRT = NormalizeYaw(transform.eulerAngles.y);   // ��: 90
            leftYawRT = NormalizeYaw(rightYawRT + 180f);         // ��: 270
        }
        else
        {
            rightYawRT = NormalizeYaw(rightYaw);
            leftYawRT = NormalizeYaw(leftYaw);
        }

        lastTargetX = target ? target.position.x : 0f;
    }

    private void FixedUpdate()
    {
        if (!target) return;

        // �÷��̾� ���� �Ұ�/�̵� �Ұ��� �굵 õõ�� ����
        if (playerCtrl != null && (!playerCtrl.canControl || !playerCtrl.canMove))
        {
            DecelerateToStop();
            LockAxesAndApply(rb.position);
            return;
        }

        // �� �÷��̾� ���� ����: Animator "Speed" �켱, ������ Transform X�ӵ��� ����
        bool playerIsRunning = false;
        bool playerIsWalking = false;

        if (matchPlayerSpeed && playerHasSpeedParam && playerAnim)
        {
            float s = playerAnim.GetFloat(playerSpeedParam); // 0 / 0.5 / 1
            playerIsRunning = s >= 0.75f;
            playerIsWalking = s >= 0.25f && s < 0.75f;
        }
        else
        {
            float nowX = target.position.x;
            float vxPlayer = Mathf.Abs((nowX - lastTargetX) / Mathf.Max(Time.fixedDeltaTime, 1e-4f));
            lastTargetX = nowX;

            playerIsRunning = vxPlayer >= playerRunVxThreshold;
            playerIsWalking = !playerIsRunning && (vxPlayer >= playerWalkVxThreshold);
        }

        // ��ǥ ��ġ(�÷��̾� ��/�� ������)
        bool playerFacingRight = Mathf.Abs(Mathf.DeltaAngle(target.eulerAngles.y, 0f)) < 90f;
        float offset = playerFacingRight ? -Mathf.Abs(desiredOffsetX) : Mathf.Abs(desiredOffsetX);
        float anchorX = target.position.x + offset;

        Vector3 pos = rb.position;

        // �ʹ� �ָ� ����
        if (snapIfFartherThan > 0f && Mathf.Abs(target.position.x - pos.x) > snapIfFartherThan)
        {
            pos.x = target.position.x + offset;
            vx = 0f;
            LockAxesAndApply(pos);
            return;
        }

        // �÷��̾� ���¿� ���� ���� �ִ� �ӵ� ����
        float maxSpeed = playerIsRunning ? runFollowSpeed : (playerIsWalking ? walkFollowSpeed : walkFollowSpeed);

        // ��ǥ �ӵ�
        float dx = anchorX - pos.x;
        float adx = Mathf.Abs(dx);
        float desiredVx = (adx > followDistance) ? Mathf.Sign(dx) * maxSpeed : 0f;

        // ������
        float rate = Mathf.Approximately(desiredVx, 0f) ? decel : accel;
        vx = Mathf.MoveTowards(vx, desiredVx, rate * Time.fixedDeltaTime);

        // �̵�(X��)
        pos.x += vx * Time.fixedDeltaTime;
        LockAxesAndApply(pos);

        // �ٶ󺸱�(������<->���� 180����)
        FaceByVelocity(vx);
    }

    private void DecelerateToStop()
    {
        vx = Mathf.MoveTowards(vx, 0f, decel * Time.fixedDeltaTime);
        var pos = rb.position;
        pos.x += vx * Time.fixedDeltaTime;
        rb.MovePosition(pos);
    }

    private void LockAxesAndApply(Vector3 pos)
    {
        if (lockZ) pos.z = zLock;
        if (lockY) pos.y = yLock;
        rb.MovePosition(pos);
    }

    private void FaceByVelocity(float vxNow)
    {
        if (Mathf.Abs(vxNow) < 0.001f) return; // ���� ������ ����
        float wantYaw = (vxNow >= 0f) ? rightYawRT : leftYawRT;
        Quaternion want = Quaternion.Euler(0f, wantYaw, 0f);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, want, rotateSpeed * Time.fixedDeltaTime));
    }

    private static float NormalizeYaw(float y)
    {
        y %= 360f;
        if (y < 0f) y += 360f;
        return y;
    }

    private static bool AnimatorHasParam(Animator animator, string paramName)
    {
        if (!animator || animator.runtimeAnimatorController == null) return false;
        foreach (var p in animator.parameters) if (p.name == paramName) return true;
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        var p = Application.isPlaying && rb ? rb.position : transform.position;
        Gizmos.DrawLine(p + Vector3.right * followDistance, p - Vector3.right * followDistance);
    }
#endif
}

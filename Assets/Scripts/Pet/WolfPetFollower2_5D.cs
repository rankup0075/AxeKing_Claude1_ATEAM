using UnityEngine;

/// <summary>
/// 2.5D(���̵��) ���� �� ������.
/// - X��(�¿�)���θ� �̵�
/// - Z, Y ��� ����(�÷� ������ �� Y ���� ��õ)
/// - PlayerController.Instance �Ǵ� Tag "Player"�� �ڵ� Ÿ������ ���
/// - �÷��̾ ���� �Ұ�(canControl=false) / �̵� �Ұ�(canMove=false)�� �굵 ���� ���
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WolfPetFollower2_5D : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("���� ���(���� PlayerRoot). ����θ� �ڵ����� PlayerController.Instance�� 'Player' �±׸� ã���ϴ�.")]
    public Transform target;

    [Header("Follow (Horizontal Only)")]
    [Tooltip("Ÿ�ٰ� �� �Ÿ� �̳��� �����մϴ�.")]
    public float followDistance = 1.6f;
    [Tooltip("���� Ÿ���� ��(Ȥ�� ��)�� �����Ϸ��� �⺻ ����(+X�� ������, -X�� ����).")]
    public float desiredOffsetX = -1.0f;
    [Tooltip("�ִ� �̵� �ӵ�(m/s)")]
    public float maxSpeed = 3.8f;
    [Tooltip("����/����(m/s^2)")]
    public float accel = 12f;
    public float decel = 16f;

    [Header("Axis Locks")]
    [Tooltip("Z�� ����(���̵�� ����)")]
    public bool lockZ = true;
    [Tooltip("Y�� ����(������ ����). false�� �߷� ���")]
    public bool lockY = true;
    public float yLock = 1.0f;

    [Header("Facing")]
    [Tooltip("������(+X) �ٶ� ���� Y����")]
    public float rightYaw = 0f;
    [Tooltip("����(-X) �ٶ� ���� Y����")]
    public float leftYaw = 180f;
    public float rotateSpeed = 1080f; // deg/sec

    [Header("Safety")]
    [Tooltip("�ʹ� �־����� �� �ٷ� ��ƴ��� �Ÿ�(0�̸� ��Ȱ��)")]
    public float snapIfFartherThan = 25f;

    private Rigidbody rb;
    private float vx;       // ���� X�ӵ�
    private float zLock;
    private PlayerController playerCtrl; // ��Ȳ ������

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = !lockY;

        // Ÿ�� �ڵ� �Ҵ�
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

        zLock = transform.position.z;
        if (lockY) yLock = transform.position.y;
    }

    private void FixedUpdate()
    {
        if (!target)
            return;

        // �÷��̾� ��Ȳ�� ���� ����(����/��ȭ ������ canControl=false, Ȥ�� canMove=false)
        if (playerCtrl != null)
        {
            if (!playerCtrl.canControl || !playerCtrl.canMove)
            {
                DecelerateToStop();
                LockAxesAndApply(rb.position);
                return;
            }
        }

        // "Ÿ�� ���� ����"�� ���� Ÿ���� �ٶ󺸴� ���⿡ ���� �������� �̵�
        bool targetFacingRight = Mathf.DeltaAngle(target.eulerAngles.y, rightYaw) < 90f;
        float offset = targetFacingRight ? -Mathf.Abs(desiredOffsetX) : Mathf.Abs(desiredOffsetX);
        float anchorX = target.position.x + offset;

        Vector3 pos = rb.position;
        float dx = (anchorX - pos.x);
        float adx = Mathf.Abs(dx);

        // �ʹ� �־����� ����(�ɼ�)
        if (snapIfFartherThan > 0f && Mathf.Abs(target.position.x - pos.x) > snapIfFartherThan)
        {
            pos.x = target.position.x + offset;
            vx = 0f;
            LockAxesAndApply(pos);
            FaceByVelocity(0f); // �ٷ� ���� ����
            return;
        }

        // ���ϴ� �ӵ�
        float desiredVx = 0f;
        if (adx > followDistance)
            desiredVx = Mathf.Sign(dx) * maxSpeed;

        // ���������� �ε巴�� ����
        float rate = Mathf.Approximately(desiredVx, 0f) ? decel : accel;
        vx = Mathf.MoveTowards(vx, desiredVx, rate * Time.fixedDeltaTime);

        // ��ġ ���� (X�� �̵�)
        pos.x += vx * Time.fixedDeltaTime;

        LockAxesAndApply(pos);
        FaceByVelocity(vx);
    }

    private void DecelerateToStop()
    {
        vx = Mathf.MoveTowards(vx, 0f, decel * Time.fixedDeltaTime);
        Vector3 pos = rb.position;
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
        // ���� ������ ȸ�� ����
        if (Mathf.Abs(vxNow) < 0.001f) return;

        float targetYaw = (vxNow >= 0f) ? rightYaw : leftYaw;
        Quaternion want = Quaternion.Euler(0f, targetYaw, 0f);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, want, rotateSpeed * Time.fixedDeltaTime));
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 p = Application.isPlaying ? rb.position : transform.position;
        Gizmos.DrawLine(p + Vector3.right * followDistance, p - Vector3.right * followDistance);
    }
#endif
}

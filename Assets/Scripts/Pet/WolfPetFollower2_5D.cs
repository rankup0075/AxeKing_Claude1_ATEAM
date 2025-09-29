using UnityEngine;

/// <summary>
/// 2.5D(사이드뷰) 전용 펫 추적자.
/// - X축(좌우)으로만 이동
/// - Z, Y 잠금 가능(플랫 지면일 때 Y 고정 추천)
/// - PlayerController.Instance 또는 Tag "Player"를 자동 타겟으로 사용
/// - 플레이어가 제어 불가(canControl=false) / 이동 불가(canMove=false)면 펫도 서서 대기
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WolfPetFollower2_5D : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("따라갈 대상(보통 PlayerRoot). 비워두면 자동으로 PlayerController.Instance나 'Player' 태그를 찾습니다.")]
    public Transform target;

    [Header("Follow (Horizontal Only)")]
    [Tooltip("타겟과 이 거리 이내면 정지합니다.")]
    public float followDistance = 1.6f;
    [Tooltip("펫이 타겟의 뒤(혹은 앞)에 유지하려는 기본 간격(+X는 오른쪽, -X는 왼쪽).")]
    public float desiredOffsetX = -1.0f;
    [Tooltip("최대 이동 속도(m/s)")]
    public float maxSpeed = 3.8f;
    [Tooltip("가속/감속(m/s^2)")]
    public float accel = 12f;
    public float decel = 16f;

    [Header("Axis Locks")]
    [Tooltip("Z축 고정(사이드뷰 유지)")]
    public bool lockZ = true;
    [Tooltip("Y축 고정(평지면 권장). false면 중력 사용")]
    public bool lockY = true;
    public float yLock = 1.0f;

    [Header("Facing")]
    [Tooltip("오른쪽(+X) 바라볼 때의 Y각도")]
    public float rightYaw = 0f;
    [Tooltip("왼쪽(-X) 바라볼 때의 Y각도")]
    public float leftYaw = 180f;
    public float rotateSpeed = 1080f; // deg/sec

    [Header("Safety")]
    [Tooltip("너무 멀어졌을 때 바로 잡아당기는 거리(0이면 비활성)")]
    public float snapIfFartherThan = 25f;

    private Rigidbody rb;
    private float vx;       // 현재 X속도
    private float zLock;
    private PlayerController playerCtrl; // 상황 인지용

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = !lockY;

        // 타겟 자동 할당
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

        // 플레이어 상황에 따라 정지(상점/대화 등으로 canControl=false, 혹은 canMove=false)
        if (playerCtrl != null)
        {
            if (!playerCtrl.canControl || !playerCtrl.canMove)
            {
                DecelerateToStop();
                LockAxesAndApply(rb.position);
                return;
            }
        }

        // "타겟 뒤쪽 유지"를 위해 타겟의 바라보는 방향에 따라 기준점을 이동
        bool targetFacingRight = Mathf.DeltaAngle(target.eulerAngles.y, rightYaw) < 90f;
        float offset = targetFacingRight ? -Mathf.Abs(desiredOffsetX) : Mathf.Abs(desiredOffsetX);
        float anchorX = target.position.x + offset;

        Vector3 pos = rb.position;
        float dx = (anchorX - pos.x);
        float adx = Mathf.Abs(dx);

        // 너무 멀어지면 스냅(옵션)
        if (snapIfFartherThan > 0f && Mathf.Abs(target.position.x - pos.x) > snapIfFartherThan)
        {
            pos.x = target.position.x + offset;
            vx = 0f;
            LockAxesAndApply(pos);
            FaceByVelocity(0f); // 바로 정지 상태
            return;
        }

        // 원하는 속도
        float desiredVx = 0f;
        if (adx > followDistance)
            desiredVx = Mathf.Sign(dx) * maxSpeed;

        // 가감속으로 부드럽게 보간
        float rate = Mathf.Approximately(desiredVx, 0f) ? decel : accel;
        vx = Mathf.MoveTowards(vx, desiredVx, rate * Time.fixedDeltaTime);

        // 위치 적분 (X만 이동)
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
        // 거의 정지면 회전 유지
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

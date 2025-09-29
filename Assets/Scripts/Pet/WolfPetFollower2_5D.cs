using UnityEngine;

/// <summary>
/// 2.5D(사이드뷰) 전용 펫 추적자.
/// - X축(좌우) 이동만
/// - 시작 Y 회전을 "오른쪽" 기준으로 삼아(기본: 현 회전) 180도만 전환
/// - 플레이어 Animator의 "Speed"(0/0.5/1)를 우선 읽되, 없으면 플레이어 Transform의 수평 속도로 걷기/달리기 추정
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WolfPetFollower2_5D : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("따라갈 대상(보통 PlayerRoot). 비워두면 PlayerController.Instance나 'Player' 태그를 자동 탐색")]
    public Transform target;

    [Header("Follow (Horizontal Only)")]
    public float followDistance = 1.6f;   // 이 이내면 정지
    public float desiredOffsetX = -1.0f;  // 타겟 기준 유지 간격(+X/-X)

    [Header("Speed (auto-switches with player's state)")]
    [Tooltip("플레이어가 걷는 중일 때 펫의 최대 속도")]
    public float walkFollowSpeed = 3.8f;
    [Tooltip("플레이어가 달리는 중일 때 펫의 최대 속도")]
    public float runFollowSpeed = 6.0f;
    [Tooltip("가속/감속(m/s^2)")]
    public float accel = 12f;
    public float decel = 16f;

    [Header("Axis Locks")]
    public bool lockZ = true;
    public bool lockY = true;
    public float yLock = 0f;

    [Header("Facing")]
    [Tooltip("시작 Y 회전을 '오른쪽'으로 사용(예: 시작이 90°면 우=90°, 좌=270°)")]
    public bool useStartYawAsRight = true;
    [Tooltip("시작 각도를 쓰지 않을 때 직접 지정")]
    public float rightYaw = 0f;
    public float leftYaw = 180f;
    public float rotateSpeed = 1080f;     // deg/sec

    [Header("Sync With Player (fallback ready)")]
    [Tooltip("플레이어 Animator의 'Speed'(0/0.5/1)를 먼저 시도, 없으면 Transform 속도로 추정")]
    public bool matchPlayerSpeed = true;
    public string playerSpeedParam = "Speed";
    [Tooltip("플레이어 X속도 기준 걷기/달리기 임계값(플레이어 파라미터 없을 때 사용)")]
    public float playerWalkVxThreshold = 0.4f;
    public float playerRunVxThreshold = 3.0f;

    [Header("Safety")]
    public float snapIfFartherThan = 25f; // 너무 멀면 순간 보정(0=off)

    private Rigidbody rb;
    private float vx;           // 현재 X 속도
    private float zLock;

    private PlayerController playerCtrl;
    private Animator playerAnim;
    private bool playerHasSpeedParam;

    // 플레이어 속도 추정 용
    private float lastTargetX;

    // 실행 중 사용할 실제 좌/우 각도
    private float rightYawRT;
    private float leftYawRT;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = !lockY;

        // 타겟/플레이어 참조
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

        // 축 잠금 기준
        zLock = transform.position.z;
        if (lockY) yLock = transform.position.y;

        // 오른쪽/왼쪽 기준 각도 결정
        if (useStartYawAsRight)
        {
            rightYawRT = NormalizeYaw(transform.eulerAngles.y);   // 예: 90
            leftYawRT = NormalizeYaw(rightYawRT + 180f);         // 예: 270
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

        // 플레이어 제어 불가/이동 불가면 펫도 천천히 정지
        if (playerCtrl != null && (!playerCtrl.canControl || !playerCtrl.canMove))
        {
            DecelerateToStop();
            LockAxesAndApply(rb.position);
            return;
        }

        // ★ 플레이어 상태 판정: Animator "Speed" 우선, 없으면 Transform X속도로 추정
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

        // 목표 위치(플레이어 앞/뒤 오프셋)
        bool playerFacingRight = Mathf.Abs(Mathf.DeltaAngle(target.eulerAngles.y, 0f)) < 90f;
        float offset = playerFacingRight ? -Mathf.Abs(desiredOffsetX) : Mathf.Abs(desiredOffsetX);
        float anchorX = target.position.x + offset;

        Vector3 pos = rb.position;

        // 너무 멀면 스냅
        if (snapIfFartherThan > 0f && Mathf.Abs(target.position.x - pos.x) > snapIfFartherThan)
        {
            pos.x = target.position.x + offset;
            vx = 0f;
            LockAxesAndApply(pos);
            return;
        }

        // 플레이어 상태에 맞춰 펫의 최대 속도 선택
        float maxSpeed = playerIsRunning ? runFollowSpeed : (playerIsWalking ? walkFollowSpeed : walkFollowSpeed);

        // 목표 속도
        float dx = anchorX - pos.x;
        float adx = Mathf.Abs(dx);
        float desiredVx = (adx > followDistance) ? Mathf.Sign(dx) * maxSpeed : 0f;

        // 가감속
        float rate = Mathf.Approximately(desiredVx, 0f) ? decel : accel;
        vx = Mathf.MoveTowards(vx, desiredVx, rate * Time.fixedDeltaTime);

        // 이동(X만)
        pos.x += vx * Time.fixedDeltaTime;
        LockAxesAndApply(pos);

        // 바라보기(오른쪽<->왼쪽 180도만)
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
        if (Mathf.Abs(vxNow) < 0.001f) return; // 거의 정지면 유지
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

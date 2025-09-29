using UnityEngine;

/// <summary>
/// 플레이어를 자동으로 찾아 따라가는 펫.
/// Idle / Walk / Run 애니메이션 동기화 + 점프 동기화.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WolfPetFollower2_5D : MonoBehaviour
{
    [Header("Follow Settings")]
    public float followDistance = 1.5f;
    public float walkFollowSpeed = 4f;
    public float runFollowSpeed = 6f;
    public float accel = 10f;
    public float decel = 14f;

    [Header("Animation")]
    public Animator anim;
    public string speedParam = "Speed";

    private Rigidbody rb;
    private float vx;
    private float zLock;

    private PlayerController player;   // 자동으로 찾음
    private Animator playerAnim;
    private Rigidbody playerRb;

    private bool prevPlayerGrounded = true; // 점프 시작 감지용

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (!anim) anim = GetComponentInChildren<Animator>();

        zLock = transform.position.z;

        // 플레이어 자동 참조
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance;
            playerAnim = player.GetComponent<Animator>();
            playerRb = player.GetComponent<Rigidbody>();
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.GetComponent<PlayerController>();
                playerAnim = p.GetComponent<Animator>();
                playerRb = p.GetComponent<Rigidbody>();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!player || !playerAnim || !playerRb) return;

        // === 1. 달리기 여부 판정 (실제 속도 기준) ===
        float vxPlayer = Mathf.Abs(playerRb.linearVelocity.x);
        float runCutoff = Mathf.Max(0.6f * player.runSpeed, player.walkSpeed + 0.1f);
        bool playerRunningNow = vxPlayer >= runCutoff;

        // === 2. 점프 시작 시 Y축 힘 적용 ===
        bool groundedNow = player.IsGrounded;
        if (prevPlayerGrounded && !groundedNow) // 착지 -> 점프 전환
        {
            rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
        }
        prevPlayerGrounded = groundedNow;

        // === 3. X축 추적 (MovePosition 기반) ===
        Vector3 pos = rb.position;
        float dx = player.transform.position.x - pos.x;
        float adx = Mathf.Abs(dx);

        float maxSpeed = playerRunningNow ? runFollowSpeed : walkFollowSpeed;
        float desiredVx = (adx > followDistance) ? Mathf.Sign(dx) * maxSpeed : 0f;

        float rate = Mathf.Approximately(desiredVx, 0f) ? decel : accel;
        vx = Mathf.MoveTowards(vx, desiredVx, rate * Time.fixedDeltaTime);

        pos.x += vx * Time.fixedDeltaTime;
        pos.z = zLock;
        rb.MovePosition(pos);

        // === 4. 바라보기 ===
        if (Mathf.Abs(vx) > 0.01f)
        {
            float yaw = (vx >= 0) ? 0f : 180f;
            rb.MoveRotation(Quaternion.Euler(0, yaw, 0));
        }

        // === 5. 애니메이션 ===
        float animSpeed = (Mathf.Abs(desiredVx) < 0.01f) ? 0f : (playerRunningNow ? 1f : 0.5f);
        anim.SetFloat(speedParam, animSpeed);
    }
}

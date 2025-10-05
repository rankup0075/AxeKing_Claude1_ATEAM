// Assets/Scripts/Pet/WolfPetFollower2_5D.cs
using UnityEngine;

/// <summary>
/// 플레이어를 자동으로 따라오는 2.5D 펫.
/// - Idle / Walk / Run 애니메이션 동기화
/// - 플레이어 점프 시작 시 펫도 점프
/// - Enter로 AI 대화 시작(고정 대화 없음, DialogueManager가 전송/응답 처리)
/// - 씬 전환 이후 참조 자동 보정
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

    [Header("Dialogue (AI Only)")]
    public string petName = "Wolf";
    public float talkDistance = 2.0f;
    public KeyCode talkKey = KeyCode.Return;      // Enter
    [TextArea(4, 10)]
    public string petSystemPrompt =
        "너는 플레이어를 따라다니는 친근한 늑대 펫이야. 항상 한국어로 짧고 따뜻하게 답해. " +
        "게임(2.5D 액션) 맥락에서 간단 조언을 줄 수도 있어. 이모지는 쓰지 마.";

    private Rigidbody rb;
    private float vx;
    private float zLock;

    private PlayerController player;    // 자동으로 찾음
    private Animator playerAnim;
    private Rigidbody playerRb;

    private bool prevPlayerGrounded = true; // 점프 시작 감지

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (!anim) anim = GetComponentInChildren<Animator>();
        zLock = transform.position.z;

        TryResolvePlayer(); // 처음 한 번 시도
    }

    /// <summary>씬 전환/리로드 후 참조 보정</summary>
    private void TryResolvePlayer()
    {
        // PlayerController 싱글톤 우선
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance;
            playerAnim = player.GetComponent<Animator>();
            playerRb = player.GetComponent<Rigidbody>();
            return;
        }

        // 태그로 찾기
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            player = pObj.GetComponent<PlayerController>();
            playerAnim = pObj.GetComponent<Animator>();
            playerRb = pObj.GetComponent<Rigidbody>();
        }
    }

    private void EnsureDialogueManager()
    {
        if (DialogueManager.Instance == null)
        {
            var go = new GameObject("DialogueManager");
            go.AddComponent<DialogueManager>(); // Awake에서 UI 자동 생성/설정
        }
    }

    private void Update()
    {
        // 씬 전환/리로드 후 참조가 날아갔을 수 있으니 가벼운 재탐색
        if (player == null || playerRb == null || playerAnim == null)
            TryResolvePlayer();

        // Enter 처리(대화창이 열려 있으면 간섭하지 않음 → TMP_InputField가 전송 처리)
        if (Input.GetKeyDown(talkKey))
        {
            var dm = DialogueManager.Instance;
            if (dm != null && dm.IsOpen)
            {
                // 이미 열려 있으면 여기선 아무 것도 안 함(엔터는 입력창 전송용)
                return;
            }

            // player가 없으면 대화 트리거 중단
            if (player == null) return;

            // 거리 체크(너무 멀면 열지 않음)
            float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
            if (sqrDist <= talkDistance * talkDistance)
            {
                EnsureDialogueManager();
                dm = DialogueManager.Instance;
                if (dm == null)
                {
                    Debug.LogError("[WolfPetFollower2_5D] DialogueManager 생성 실패");
                    return;
                }

                // 🔁 기존: dm.systemPrompt = petSystemPrompt; dm.Open(petName);
                // ✅ 변경: StartAIDialogue로 한 번에 전달
                dm.StartAIDialogue(petName, petSystemPrompt, null);
            }
        }
    }

    private void FixedUpdate()
    {
        if (player == null || playerAnim == null || playerRb == null)
            return;

        // 대화창이 열려 있으면 이동/애니 정지 + 플레이어 바라보기만 유지
        bool dialogueOpen = DialogueManager.Instance != null && DialogueManager.Instance.IsOpen;
        if (dialogueOpen)
        {
            var pos = rb.position;
            pos.z = zLock;
            rb.MovePosition(pos);
            FaceTowardsPlayerSlow();
            if (anim) anim.SetFloat(speedParam, 0f);
            return;
        }

        // 1) 플레이어 실제 속도 기준으로 달리기 판정
        float vxPlayer = Mathf.Abs(playerRb.linearVelocity.x);
        float runCutoff = Mathf.Max(0.6f * player.runSpeed, player.walkSpeed + 0.1f);
        bool playerRunningNow = vxPlayer >= runCutoff;

        // 2) 점프 시작 감지 → 펫도 살짝 점프
        bool groundedNow = GetPlayerGroundedSafe();
        if (prevPlayerGrounded && !groundedNow)
        {
            rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
        }
        prevPlayerGrounded = groundedNow;

        // 3) X축 추적
        Vector3 pos2 = rb.position;
        float dx = player.transform.position.x - pos2.x;
        float adx = Mathf.Abs(dx);

        float maxSpeed = playerRunningNow ? runFollowSpeed : walkFollowSpeed;
        float desiredVx = (adx > followDistance) ? Mathf.Sign(dx) * maxSpeed : 0f;

        float rate = Mathf.Approximately(desiredVx, 0f) ? decel : accel;
        vx = Mathf.MoveTowards(vx, desiredVx, rate * Time.fixedDeltaTime);

        pos2.x += vx * Time.fixedDeltaTime;
        pos2.z = zLock;
        rb.MovePosition(pos2);

        // 4) 바라보기(좌우 180도)
        if (Mathf.Abs(vx) > 0.01f)
        {
            // ⚠ 모델이 기본 90°를 정면으로 쓰면 아래 두 줄에서 0/180 → 90/270으로 바꾸세요.
            float yaw = (vx >= 0) ? 0f : 180f;
            rb.MoveRotation(Quaternion.Euler(0, yaw, 0));
        }

        // 5) 애니메이션
        float animSpeed = (Mathf.Abs(desiredVx) < 0.01f) ? 0f : (playerRunningNow ? 1f : 0.5f);
        if (anim) anim.SetFloat(speedParam, animSpeed);
    }

    private void FaceTowardsPlayerSlow()
    {
        if (player == null) return;
        float dir = Mathf.Sign(player.transform.position.x - transform.position.x);
        float yaw = dir >= 0 ? 0f : 180f; // 필요시 90/270 사용
        var want = Quaternion.Euler(0, yaw, 0);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, want, 720f * Time.fixedDeltaTime));
    }

    // PlayerController의 공개 접근자가 없어도 안전하게 착지 여부 추정
    private bool GetPlayerGroundedSafe()
    {
        return Mathf.Abs(playerRb.linearVelocity.y) < 0.01f;
    }
}

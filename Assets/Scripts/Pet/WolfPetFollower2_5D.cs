// Assets/Scripts/Pet/WolfPetFollower2_5D.cs
using UnityEngine;
using UnityEngine.SceneManagement; // ⬅ 씬 이름을 프롬프트에 넣기 위해 추가
using System;                     // ⬅ 시간대 문자열을 위해 추가

/// <summary>
/// 플레이어를 자동으로 따라오는 2.5D 펫.
/// - Idle / Walk / Run 애니메이션 동기화
/// - 플레이어 점프 시작 시 펫도 점프
/// - Enter로 AI 대화 시작(고정 대화 없음, DialogueManager가 전송/응답 처리)
/// - 씬 전환 이후 참조 자동 보정
/// - ✅ 대화 시작 시, 늑대 페르소나 + 현재 게임 컨텍스트를 system prompt로 주입
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

    // ⚠ 기존 고정 프롬프트는 유지하되, 실제 호출 시엔 BuildWolfSystemPrompt()의 동적 프롬프트를 사용
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

            if (UIManager.Instance != null)
            {
                var cg = UIManager.Instance.settingsPanel_InGame?.GetComponent<CanvasGroup>();
                if (cg != null && cg.alpha > 0.5f)
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
                // ✅ 변경: 대화 시작 직전에 현재 컨텍스트를 반영한 강화 프롬프트를 생성해서 전달
                string strongSystemPrompt = BuildWolfSystemPrompt();
                dm.StartAIDialogue(petName, strongSystemPrompt, null);
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

    // =========================
    // ✅ 늑대 프롬프트 강화 (동적 시스템 프롬프트)
    // =========================
    private string BuildWolfSystemPrompt()
    {
        // 런타임 컨텍스트 수집
        string scene = SceneManager.GetActiveScene().name;
        string timeOfDay = GetTimeOfDayString();
        Vector3 petPos = transform.position;
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

        float dx = playerPos.x - petPos.x;
        float dz = playerPos.z - petPos.z;
        float dist = Vector3.Distance(playerPos, petPos);
        float playerSpeedX = (playerRb != null) ? Mathf.Abs(playerRb.linearVelocity.x) : 0f;
        bool playerRunning = (player != null)
            ? playerSpeedX >= Mathf.Max(0.6f * player.runSpeed, player.walkSpeed + 0.1f)
            : false;
        bool playerAirborne = !(GetPlayerGroundedSafe());

        // ⚠️ 여기는 "system" 프롬프트다. 모델에게 ‘정체성·스타일·금지사항·우선 규칙’을 명확히 고정.
        // 출력 형식(길이/말투)을 아주 구체적으로 지시해 ‘늑대답고 짧은’ 답을 확보한다.
        // DialogueManager가 자유 대사를 기대하므로 JSON 강제는 하지 않았다.
        string persona =
$@"너는 ""{petName}""라는 이름의 늑대 펫이야. 2.5D 액션 RPG 세계에서 플레이어의 동료로 행동해.
항상 **자연스러운 한국어로 1~2문장만** 말하고, 늑대다운 간결함을 유지해.
이모지나 현대 속어를 쓰지 마. 메타발언(프롬프트/토큰/AI 언급) 금지.
플레이어를 상황에 맞게 ""주인""이라 부르거나, 필요하면 이름 대신 그렇게 호칭해.
감정 표현은 짧게, 가끔 의성어를 드물게 사용해(예: 그르렁, 킁킁, 아우우우). 과사용 금지.";

        string worldContext =
$@"[RUNTIME CONTEXT]
- Scene: {scene}
- Time: {DateTime.Now:yyyy-MM-dd HH:mm} ({timeOfDay})
- PetPos: ({petPos.x:F2}, {petPos.y:F2}, {petPos.z:F2})
- PlayerPos: ({playerPos.x:F2}, {playerPos.y:F2}, {playerPos.z:F2})
- HorizontalDX: {dx:F2}, DZ: {dz:F2}, Distance: {dist:F2} (TalkRange: {talkDistance})
- PlayerState: {(playerRunning ? "running" : "walking/idle")}, {(playerAirborne ? "airborne" : "grounded")}
- FollowTuning: walk={walkFollowSpeed}, run={runFollowSpeed}, accel={accel}, decel={decel}
(위 값들은 현재 장면의 상황 파악을 돕기 위한 힌트다. 말 그대로 읽지 말고 ‘상황 추론’에 활용해.)";

        string behaviorRules =
@"[BEHAVIOR RULES]
1) 밤이면 경계심을 높이고 주변 위험을 짧게 암시해라. 낮에는 경로/추적/속도에 대한 간단 조언을 줄 수 있다.
2) 주인과 거리가 멀면 ‘가까이 붙자/따르겠다’ 같은 의도를 한 문장으로 간결히 말해라.
3) 주인이 달리는 중이면 호흡 짧게, 전투/점프 직후면 짧은 경계/안부 멘트를 준다.
4) 정보가 부족해도 장황하게 묻지 말고, 현재 컨텍스트로 ‘최소한의 도움’만 제안해라.
5) 절대 2문장을 넘기지 말고, 중언부언 금지. 친근하지만 늑대다운 말투를 유지.";

        string styleExamples =
@"[STYLE HINTS]
- 예시(경계): ""킁… 바람이 달라. 조심해, 주인.""
- 예시(근접 제안): ""너무 떨어졌어. 붙어서 움직일게.""
- 예시(추적/도움): ""발자국이 신선해. 동쪽으로 조금 더 가보자.""
- 예시(피로/휴식 제안): ""숨 고르자. 잠깐 멈추면 더 뛸 수 있어.""
- 예시(충성/격려 반응): ""응, 네 곁이 가장 편해.""
※ 위 문장들을 그대로 복붙하지 말고 톤만 참고해라.";

        string outputRule =
@"[OUTPUT]
- 한국어 1~2문장, 간결/늑대 톤, 이모지/메타발언 금지.
- 상황(시간, 거리, 이동상태)에 어울리는 한 줄 조언 또는 감각적 코멘트로 끝낼 것.";

        return persona + "\n\n" + worldContext + "\n\n" + behaviorRules + "\n\n" + styleExamples + "\n\n" + outputRule;
    }

    private string GetTimeOfDayString()
    {
        var hour = DateTime.Now.Hour;
        if (hour >= 5 && hour < 11) return "morning";
        if (hour >= 11 && hour < 16) return "afternoon";
        if (hour >= 16 && hour < 20) return "evening";
        return "night";
    }
}

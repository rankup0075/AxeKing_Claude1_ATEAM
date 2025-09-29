using UnityEngine;

/// <summary>
/// Animator "Speed"를 세팅.
/// - 기본: 플레이어 Animator의 "Speed"(0/0.5/1)를 미러링
/// - 폴백: 내 X속도로 0/0.5/1 계산 (Animator 파라미터 없거나 컨트롤러 미연결 시)
/// - 자식에 붙은 Animator도 자동 탐색
/// </summary>
public class WolfPetAnimator2D : MonoBehaviour
{
    [Header("Animator Reference")]
    [Tooltip("비워두면 이 오브젝트 또는 자식에서 자동 탐색")]
    public Animator anim;

    [Header("Animator Param")]
    public string speedParam = "Speed";

    [Header("Mirror Player (recommended)")]
    public bool mirrorPlayerSpeed = true;
    public string playerSpeedParam = "Speed";

    [Header("If NOT mirroring, compute from velocity")]
    public float idleThreshold = 0.05f; // 이하면 Idle
    public float runThreshold = 2.4f;   // 이상이면 Run, 그 사이는 Walk
    public float lerpSpeed = 5f;        // 보간 속도

    private Rigidbody rb;
    private Animator playerAnim;

    private Vector3 lastPos;
    private bool hasSpeedParam;
    private bool hasPlayerSpeedParam;
    private bool warnedOnce;

    private void Awake()
    {
        // Animator 자동 탐색(자식 포함)
        if (!anim) anim = GetComponent<Animator>();
        if (!anim) anim = GetComponentInChildren<Animator>();

        rb = GetComponent<Rigidbody>();

        if (PlayerController.Instance)
        {
            playerAnim = PlayerController.Instance.GetComponent<Animator>();
            if (!playerAnim) playerAnim = PlayerController.Instance.GetComponentInChildren<Animator>();
        }

        lastPos = transform.position;

        hasSpeedParam = AnimatorHasParam(anim, speedParam);
        hasPlayerSpeedParam = playerAnim && AnimatorHasParam(playerAnim, playerSpeedParam);

        // 내 Animator에 컨트롤러나 파라미터가 없으면 미러링 비활성화
        if (!hasSpeedParam || anim == null || anim.runtimeAnimatorController == null)
        {
            mirrorPlayerSpeed = false;
            WarnOnce($"Animator 또는 Float '{speedParam}' 파라미터가 없습니다. " +
                     $"→ 해결: Wolf.controller에 Float '{speedParam}' 추가하고 해당 Animator에 연결하세요. " +
                     $"지금은 좌표 기반 폴백으로 동작합니다.");
        }
        else if (mirrorPlayerSpeed && !hasPlayerSpeedParam)
        {
            WarnOnce($"플레이어 Animator에 Float '{playerSpeedParam}' 파라미터가 없습니다. " +
                     $"펫은 자체 속도 기반으로 애니를 전환합니다.");
        }
    }

    private void Update()
    {
        if (!anim) return;

        float target;

        if (mirrorPlayerSpeed && hasSpeedParam && hasPlayerSpeedParam && playerAnim)
        {
            // 플레이어 Speed(0/0.5/1) 그대로
            target = playerAnim.GetFloat(playerSpeedParam);
        }
        else
        {
            // 폴백: 내 수평 속도 → 0/0.5/1
            Vector3 p = transform.position;
            float vx = Mathf.Abs((p.x - lastPos.x) / Mathf.Max(Time.deltaTime, 1e-4f));
            lastPos = p;

            if (vx <= idleThreshold) target = 0f;
            else if (vx >= runThreshold) target = 1f;
            else target = 0.5f;
        }

        if (hasSpeedParam)
        {
            float current = anim.GetFloat(speedParam);
            float next = Mathf.Lerp(current, target, Time.deltaTime * lerpSpeed);
            anim.SetFloat(speedParam, next);
        }
    }

    private static bool AnimatorHasParam(Animator animator, string paramName)
    {
        if (!animator || animator.runtimeAnimatorController == null) return false;
        foreach (var p in animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    private void WarnOnce(string msg)
    {
        if (warnedOnce) return;
        warnedOnce = true;
        Debug.LogWarning($"[WolfPetAnimator2D] {msg}", this);
    }
}

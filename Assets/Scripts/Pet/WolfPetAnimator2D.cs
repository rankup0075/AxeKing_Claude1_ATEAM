using UnityEngine;

/// <summary>
/// Rigidbody의 수평 속도를 읽어 Animator "Speed" 파라미터(0/0.5/1)로 매핑.
/// 플레이어의 PlayerAnimationController 로직(Idle/Walk/Run)과 동일한 체감값을 맞추기 위한 드라이버.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class WolfPetAnimator2D : MonoBehaviour
{
    [Header("Animator Param")]
    public string speedParam = "Speed";
    [Tooltip("이 값 이하이면 Idle(0)로 취급")]
    public float idleThreshold = 0.05f;
    [Tooltip("이 값 미만은 Walk(0.5), 이상은 Run(1)로 취급")]
    public float runThreshold = 2.4f;
    [Tooltip("보간 속도")]
    public float lerpSpeed = 5f;

    private Animator anim;
    private Rigidbody rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float speedX = Mathf.Abs(rb.velocity.x); // 수평 속도만
        float target = 0f;

        if (speedX > idleThreshold)
            target = (speedX >= runThreshold) ? 1f : 0.5f;

        float current = anim.GetFloat(speedParam);
        float next = Mathf.Lerp(current, target, Time.deltaTime * lerpSpeed);
        anim.SetFloat(speedParam, next);
    }
}

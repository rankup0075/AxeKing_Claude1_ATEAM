using UnityEngine;

/// <summary>
/// Rigidbody�� ���� �ӵ��� �о� Animator "Speed" �Ķ����(0/0.5/1)�� ����.
/// �÷��̾��� PlayerAnimationController ����(Idle/Walk/Run)�� ������ ü������ ���߱� ���� ����̹�.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class WolfPetAnimator2D : MonoBehaviour
{
    [Header("Animator Param")]
    public string speedParam = "Speed";
    [Tooltip("�� �� �����̸� Idle(0)�� ���")]
    public float idleThreshold = 0.05f;
    [Tooltip("�� �� �̸��� Walk(0.5), �̻��� Run(1)�� ���")]
    public float runThreshold = 2.4f;
    [Tooltip("���� �ӵ�")]
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
        float speedX = Mathf.Abs(rb.velocity.x); // ���� �ӵ���
        float target = 0f;

        if (speedX > idleThreshold)
            target = (speedX >= runThreshold) ? 1f : 0.5f;

        float current = anim.GetFloat(speedParam);
        float next = Mathf.Lerp(current, target, Time.deltaTime * lerpSpeed);
        anim.SetFloat(speedParam, next);
    }
}

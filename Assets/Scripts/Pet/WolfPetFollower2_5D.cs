using UnityEngine;

/// <summary>
/// �÷��̾ �ڵ����� ã�� ���󰡴� ��.
/// Idle / Walk / Run �ִϸ��̼� ����ȭ + ���� ����ȭ.
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

    private PlayerController player;   // �ڵ����� ã��
    private Animator playerAnim;
    private Rigidbody playerRb;

    private bool prevPlayerGrounded = true; // ���� ���� ������

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (!anim) anim = GetComponentInChildren<Animator>();

        zLock = transform.position.z;

        // �÷��̾� �ڵ� ����
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

        // === 1. �޸��� ���� ���� (���� �ӵ� ����) ===
        float vxPlayer = Mathf.Abs(playerRb.linearVelocity.x);
        float runCutoff = Mathf.Max(0.6f * player.runSpeed, player.walkSpeed + 0.1f);
        bool playerRunningNow = vxPlayer >= runCutoff;

        // === 2. ���� ���� �� Y�� �� ���� ===
        bool groundedNow = player.IsGrounded;
        if (prevPlayerGrounded && !groundedNow) // ���� -> ���� ��ȯ
        {
            rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
        }
        prevPlayerGrounded = groundedNow;

        // === 3. X�� ���� (MovePosition ���) ===
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

        // === 4. �ٶ󺸱� ===
        if (Mathf.Abs(vx) > 0.01f)
        {
            float yaw = (vx >= 0) ? 0f : 180f;
            rb.MoveRotation(Quaternion.Euler(0, yaw, 0));
        }

        // === 5. �ִϸ��̼� ===
        float animSpeed = (Mathf.Abs(desiredVx) < 0.01f) ? 0f : (playerRunningNow ? 1f : 0.5f);
        anim.SetFloat(speedParam, animSpeed);
    }
}

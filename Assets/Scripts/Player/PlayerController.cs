using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 10f;

    [Header("Combat Settings")]
    public int attackDamage = 1;
    public float attackCooldown = 0.5f;     // 공격 간격
    public float hitStunDuration = 0.5f;    // 피격 후 경직 시간

    // 컴포넌트
    private Rigidbody rb;
    private Animator animator;
    private PlayerHealth playerHealth;
    private PlayerInventory inventory;

    // 상태 플래그
    private bool isGrounded = true;
    private bool isAttacking = false;
    private bool isStunned = false;
    public bool canMove = true;

    private float lastAttackTime = 0f;
    private float horizontalInput;

    // Animator 해시값
    private int speedHash = Animator.StringToHash("Speed");
    private int groundedHash = Animator.StringToHash("IsGrounded");
    private int attackHash = Animator.StringToHash("Attack");
    private int hitHash = Animator.StringToHash("Hit");
    private int dieHash = Animator.StringToHash("Die");

    public static PlayerController Instance;
    [HideInInspector] public bool canControl = true; // 상점 등 외부 UI에서 제어할 때 사용

    void Awake()
    {
        Debug.Log($"[PlayerController] Awake 실행됨 - {gameObject.name} / Scene: {gameObject.scene.name}");
        var root = transform.root.gameObject;
        if (PlayerController.Instance != null && PlayerController.Instance != this)
        {
            Debug.LogWarning($"[PlayerController] 중복 Player 감지 → {root.name} 삭제");
            Destroy(root);
            return;
        }

        PlayerController.Instance = this;
        DontDestroyOnLoad(root);
        Debug.Log($"[PlayerController] PlayerRoot 유지됨: {root.name}");
    }

    void OnDestroy()
    {
        Debug.LogError($"[PlayerController] Player 파괴됨! Scene: {gameObject.scene.name}");
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
        inventory = GetComponent<PlayerInventory>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 시 항상 정상 상태 보장
        canControl = true;
        Time.timeScale = 1f;

        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;
            animator.updateMode = AnimatorUpdateMode.Normal;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Update()
    {
        // 외부 제어(canControl=false)일 때 → 완전 멈춤
        if (!canControl)
        {
            if (rb != null)
            {
                var v = rb.linearVelocity;
                v.x = 0f;
                rb.linearVelocity = v;
            }
            animator.SetFloat(speedHash, 0f);
            return;
        }

        if (isStunned || !canMove) return;

        // 입력
        horizontalInput = Input.GetAxisRaw("Horizontal");
        bool jumpPressed = Input.GetKeyDown(KeyCode.C);
        bool attackPressed = Input.GetKeyDown(KeyCode.Z);

        // 이동, 점프, 공격
        HandleMovement(horizontalInput);
        if (jumpPressed) HandleJump();
        if (attackPressed) HandleAttack();

        HandlePotions();
        HandleInteraction();

        // Animator 상태 업데이트
        animator.SetBool(groundedHash, isGrounded);

        if (isGrounded && !isAttacking)
        {
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                bool isRunning = Input.GetKey(KeyCode.LeftShift);
                animator.SetFloat(speedHash, isRunning ? 1f : 0.5f);
            }
            else animator.SetFloat(speedHash, 0f);
        }
        else
        {
            animator.SetFloat(speedHash, 0f);
        }

        if (canMove == false)
        {
            animator.SetFloat("Speed", 0f); // 이동 속도 0
            animator.Play("Idle"); // Idle 애니메이션 재생
        }
    }

    // ================= 이동, 점프, 공격 =================
    void HandleMovement(float horizontal)
    {
        if (isAttacking) return;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 vel = rb.linearVelocity;
        vel.x = horizontal * currentSpeed;
        rb.linearVelocity = vel;

        if (horizontal > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (horizontal < 0) transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    void HandleJump()
    {
        if (isGrounded && !isAttacking)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void HandleAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown && isGrounded && !isAttacking)
            StartAttack();
    }

    void StartAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, rb.linearVelocity.z);

        animator.ResetTrigger(attackHash);
        animator.SetTrigger(attackHash);
    }

    public void EndAttack() // 애니메이션 이벤트
    {
        isAttacking = false;
        animator.ResetTrigger(attackHash);
    }

    public void ProcessAttackHit() // 애니메이션 이벤트
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward, 2f);
        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                //스테이지 및 적 추가 후 주석 풀기
                //EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                //if (enemyHealth != null) enemyHealth.TakeDamage(attackDamage);
            }
        }
    }

    // ================= 전투 & 피격 =================
    public void TakeHit(int damage)
    {
        if (isStunned) return;

        playerHealth.TakeDamage(damage);
        animator.SetTrigger(hitHash);
        StartCoroutine(HitStun());

        if (playerHealth.CurrentHealth <= 0) Die();
    }

    IEnumerator HitStun()
    {
        isStunned = true;
        canMove = false;
        yield return new WaitForSeconds(hitStunDuration);
        isStunned = false;
        canMove = true;
    }

    void Die()
    {
        canMove = false;
        animator.SetTrigger(dieHash);
        GameManager.Instance.GameOver();
    }

    // ================= 충돌 처리 =================
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    // ================= 아이템/상호작용 =================
    void HandlePotions()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.UsePotion(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.UsePotion(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.UsePotion(2);
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Collider[] interactables = Physics.OverlapSphere(transform.position, 1.5f);
            foreach (var obj in interactables)
            {
                if (obj == null || obj.gameObject == gameObject) continue;

                if (obj.CompareTag("Portal"))
                {
                    var portal = obj.GetComponent<Portal>();
                    if (portal != null) { portal.Interact(); return; }
                }
            }
        }
    }

    public void StopImmediately()
    {
        // 이동 중일 때 강제로 멈춤
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (TryGetComponent<CharacterController>(out var cc))
        {
            cc.Move(Vector3.zero); // 일단 0으로 던져서 멈춤
        }

        // 애니메이션 중일 수 있으니 Idle 상태로 전환
        if (TryGetComponent<Animator>(out var anim))
        {
            anim.SetFloat("MoveSpeed", 0f);
        }
    }

    // ================= 기즈모 =================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, 2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}

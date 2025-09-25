//using UnityEngine;
//using System.Collections;

//public class EnemyController : MonoBehaviour
//{
//    [Header("Enemy Settings")]
//    public int attackDamage = 5;
//    public int CurrentHealth = 100;
//    public float detectionRange = 5f;
//    public float attackRange = 2f;
//    public float moveSpeed = 2f;
//    public float attackCooldown = 2f;
    
//    [Header("Drops")]
//    public int minGold = 5;
//    public int maxGold = 10;
//    public GameObject dropItem;
//    public int minDropCount = 0;
//    public int maxDropCount = 3;
    
//    private Transform player;
//    private Rigidbody rb;
//    private Animator animator;
//    private EnemyHealth enemyHealth;
//    private EnemyAI enemyAI;
    
//    private bool isAttacking = false;
//    private bool isStunned = false;
//    private float lastAttackTime = 0f;
    
//    // 애니메이션 해시값들
//    private int walkHash = Animator.StringToHash("Walk");
//    private int attackHash = Animator.StringToHash("Attack");
//    private int hitHash = Animator.StringToHash("Hit");
//    private int deathHash = Animator.StringToHash("Death");
    
//    void Start()
//    {
//        // FindObjectOfType 대신 GameObject.FindWithTag 사용
//        GameObject playerObj = GameObject.FindWithTag("Player");
//        if (playerObj != null)
//            player = playerObj.transform;
        
//        rb = GetComponent<Rigidbody>();
//        animator = GetComponent<Animator>();
//        enemyHealth = GetComponent<EnemyHealth>();
//        enemyAI = GetComponent<EnemyAI>();
//    }
    
//    void Update()
//    {
//        if (isStunned || isAttacking || enemyHealth.CurrentHealth <= 0) return;
        
//        // EnemyAI가 있으면 AI가 제어, 없으면 기본 AI
//        if (enemyAI == null)
//        {
//            BasicAI();
//        }
//    }
    
//    void BasicAI()
//    {
//        if (player == null) return;
        
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
//        if (distanceToPlayer <= attackRange)
//        {
//            AttackPlayer();
//        }
//        else if (distanceToPlayer <= detectionRange)
//        {
//            MoveTowardsPlayer();
//        }
//        else
//        {
//            // Idle 상태
//            animator.SetBool(walkHash, false);
//        }
//    }
    
//    void MoveTowardsPlayer()
//    {
//        Vector3 direction = (player.position - transform.position).normalized;
//        Vector3 movement = new Vector3(direction.x, 0, direction.z);
        
//        Vector3 newPosition = transform.position + movement * moveSpeed * Time.deltaTime;
//        rb.MovePosition(newPosition);
        
//        // 플레이어 방향으로 회전
//        if (direction.x > 0)
//            transform.rotation = Quaternion.Euler(0, 0, 0);
//        else if (direction.x < 0)
//            transform.rotation = Quaternion.Euler(0, 180, 0);
        
//        animator.SetBool(walkHash, true);
//    }
    
//    public void AttackPlayer()
//    {
//        if (Time.time < lastAttackTime + attackCooldown) return;
        
//        isAttacking = true;
//        lastAttackTime = Time.time;
//        animator.SetTrigger(attackHash);
        
//        // 플레이어에게 데미지
//        PlayerController playerController = player.GetComponent<PlayerController>();
//        if (playerController != null)
//        {
//            playerController.TakeHit(attackDamage);
//        }
        
//        Invoke(nameof(EndAttack), 1f);
//    }
    
//    void EndAttack()
//    {
//        isAttacking = false;
//    }
    
//    public void TakeHit()
//    {
//        if (enemyHealth.CurrentHealth <= 0) return;
        
//        // AI에 피격 알림
//        if (enemyAI != null)
//            enemyAI.OnHit();
        
//        StartCoroutine(HitStun());
//        animator.SetTrigger(hitHash);
//    }
    
//    IEnumerator HitStun()
//    {
//        isStunned = true;
//        yield return new WaitForSeconds(0.5f);
//        isStunned = false;
//    }
    
//    public void Die()
//    {
//        // AI에 사망 알림
//        if (enemyAI != null)
//            enemyAI.OnDeath();
        
//        animator.SetTrigger(deathHash);
        
//        // 골드 및 아이템 드롭
//        DropRewards();
        
//        // 1초 후 오브젝트 삭제
//        Destroy(gameObject, 1f);
//    }
    
//    void DropRewards()
//    {
//        // 골드 드롭
//        int goldAmount = Random.Range(minGold, maxGold + 1);
//        GameManager.Instance.AddGold(goldAmount);
        
//        // 아이템 드롭
//        if (dropItem != null)
//        {
//            int dropCount = Random.Range(minDropCount, maxDropCount + 1);
//            for (int i = 0; i < dropCount; i++)
//            {
//                // 아이템을 인벤토리에 직접 추가
//                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
//                if (inventory != null)
//                {
//                    inventory.AddItem(dropItem.name, 1);
//                }
//            }
//        }
//    }
    
//    void OnDrawGizmosSelected()
//    {
//        // 탐지 범위
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
//        // 공격 범위
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, attackRange);
//    }
//}
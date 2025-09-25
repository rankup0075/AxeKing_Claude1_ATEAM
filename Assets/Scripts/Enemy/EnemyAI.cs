//using UnityEngine;
//using UnityEngine.AI;

//public class EnemyAI : MonoBehaviour
//{
//    [Header("AI Settings")]
//    public float detectionRange = 8f;
//    public float attackRange = 2f;
//    public float moveSpeed = 3f;
//    public float rotationSpeed = 5f;

//    [Header("Patrol Settings")]
//    public bool usePatrol = true;
//    public Transform[] patrolPoints;
//    public float patrolWaitTime = 2f;
//    public float patrolSpeed = 2f;

//    [Header("Combat Settings")]
//    public float attackCooldown = 2f;
//    public float loseTargetTime = 5f;

//    [Header("References")]
//    public Transform player;
//    public NavMeshAgent navAgent;
//    public Animator animator;
//    //public EnemyController enemyController;

//    // AI 상태
//    public enum AIState
//    {
//        Idle,
//        Patrol,
//        Chase,
//        Attack,
//        Hit,
//        Dead
//    }

//    [Header("Debug")]
//    public AIState currentState = AIState.Idle;

//    // 내부 변수들
//    private float lastAttackTime;
//    private float loseTargetTimer;
//    private int currentPatrolIndex = 0;
//    private float patrolTimer;
//    private Vector3 lastKnownPlayerPosition;
//    private bool hasTarget = false;

//    // 애니메이션 해시값들
//    private int walkHash = Animator.StringToHash("Walk");
//    private int attackHash = Animator.StringToHash("Attack");
//    private int hitHash = Animator.StringToHash("Hit");
//    private int deathHash = Animator.StringToHash("Death");

//    void Start()
//    {
//        InitializeComponents();
//        InitializeAI();
//    }

//    void InitializeComponents()
//    {
//        // 컴포넌트 자동 할당
//        if (navAgent == null)
//            navAgent = GetComponent<NavMeshAgent>();
//        if (animator == null)
//            animator = GetComponentInChildren<Animator>();
//        if (enemyController == null)
//            enemyController = GetComponent<EnemyController>();

//        // 플레이어 찾기
//        if (player == null)
//        {
//            GameObject playerObj = GameObject.FindWithTag("Player");
//            if (playerObj != null)
//                player = playerObj.transform;
//        }
//    }

//    void InitializeAI()
//    {
//        if (navAgent != null)
//        {
//            navAgent.speed = moveSpeed;
//            navAgent.stoppingDistance = attackRange * 0.8f;
//            navAgent.acceleration = 12f;
//        }

//        // 패트롤 포인트가 없으면 비활성화
//        if (patrolPoints == null || patrolPoints.Length == 0)
//        {
//            usePatrol = false;
//        }

//        // 초기 상태 설정
//        ChangeState(usePatrol ? AIState.Patrol : AIState.Idle);
//    }

//    void Update()
//    {
//        if (enemyController != null && enemyController.CurrentHealth <= 0)
//        {
//            ChangeState(AIState.Dead);
//            return;
//        }

//        // 상태별 업데이트
//        switch (currentState)
//        {
//            case AIState.Idle:
//                UpdateIdle();
//                break;
//            case AIState.Patrol:
//                UpdatePatrol();
//                break;
//            case AIState.Chase:
//                UpdateChase();
//                break;
//            case AIState.Attack:
//                UpdateAttack();
//                break;
//            case AIState.Hit:
//                UpdateHit();
//                break;
//            case AIState.Dead:
//                UpdateDead();
//                break;
//        }

//        // 플레이어 감지 확인 (Dead 상태가 아닐 때만)
//        if (currentState != AIState.Dead && currentState != AIState.Hit)
//        {
//            CheckPlayerDetection();
//        }
//    }

//    void CheckPlayerDetection()
//    {
//        if (player == null) return;

//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

//        // 플레이어가 감지 범위 안에 있는지 확인
//        if (distanceToPlayer <= detectionRange)
//        {
//            // 시야 확인 (레이캐스트)
//            if (CanSeePlayer())
//            {
//                hasTarget = true;
//                lastKnownPlayerPosition = player.position;
//                loseTargetTimer = 0f;

//                // 공격 범위 안에 있으면 공격, 아니면 추적
//                if (distanceToPlayer <= attackRange && currentState != AIState.Attack)
//                {
//                    ChangeState(AIState.Attack);
//                }
//                else if (currentState != AIState.Chase && currentState != AIState.Attack)
//                {
//                    ChangeState(AIState.Chase);
//                }
//            }
//        }
//        else if (hasTarget)
//        {
//            // 타겟을 잃어가는 중
//            loseTargetTimer += Time.deltaTime;
//            if (loseTargetTimer >= loseTargetTime)
//            {
//                hasTarget = false;
//                ChangeState(usePatrol ? AIState.Patrol : AIState.Idle);
//            }
//        }
//    }

//    bool CanSeePlayer()
//    {
//        if (player == null) return false;

//        Vector3 directionToPlayer = (player.position - transform.position).normalized;
//        RaycastHit hit;

//        // 레이캐스트로 장애물 확인
//        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
//        {
//            return hit.collider.CompareTag("Player");
//        }

//        return false;
//    }

//    void ChangeState(AIState newState)
//    {
//        if (currentState == newState) return;

//        // 상태 종료 처리
//        ExitState(currentState);

//        // 새 상태 진입
//        currentState = newState;
//        EnterState(newState);
//    }

//    void EnterState(AIState state)
//    {
//        switch (state)
//        {
//            case AIState.Idle:
//                if (navAgent != null) navAgent.isStopped = true;
//                if (animator != null) animator.SetBool(walkHash, false);
//                break;

//            case AIState.Patrol:
//                if (navAgent != null)
//                {
//                    navAgent.isStopped = false;
//                    navAgent.speed = patrolSpeed;
//                }
//                SetPatrolDestination();
//                break;

//            case AIState.Chase:
//                if (navAgent != null)
//                {
//                    navAgent.isStopped = false;
//                    navAgent.speed = moveSpeed;
//                }
//                if (animator != null) animator.SetBool(walkHash, true);
//                break;

//            case AIState.Attack:
//                if (navAgent != null) navAgent.isStopped = true;
//                if (animator != null) animator.SetBool(walkHash, false);
//                break;

//            case AIState.Hit:
//                if (navAgent != null) navAgent.isStopped = true;
//                if (animator != null)
//                {
//                    animator.SetBool(walkHash, false);
//                    animator.SetTrigger(hitHash);
//                }
//                break;

//            case AIState.Dead:
//                if (navAgent != null) navAgent.isStopped = true;
//                if (animator != null)
//                {
//                    animator.SetBool(walkHash, false);
//                    animator.SetTrigger(deathHash);
//                }
//                break;
//        }
//    }

//    void ExitState(AIState state)
//    {
//        // 필요한 경우 상태 종료 처리
//    }

//    void UpdateIdle()
//    {
//        // 대기 상태 - 특별한 처리 없음
//    }

//    void UpdatePatrol()
//    {
//        if (!usePatrol || patrolPoints.Length == 0) return;

//        if (navAgent != null && !navAgent.pathPending)
//        {
//            if (navAgent.remainingDistance < 0.5f)
//            {
//                patrolTimer += Time.deltaTime;
//                if (patrolTimer >= patrolWaitTime)
//                {
//                    patrolTimer = 0f;
//                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
//                    SetPatrolDestination();
//                }
//            }
//        }

//        // 애니메이션 업데이트
//        if (animator != null)
//        {
//            bool isMoving = navAgent.velocity.magnitude > 0.1f;
//            animator.SetBool(walkHash, isMoving);
//        }
//    }

//    void SetPatrolDestination()
//    {
//        if (patrolPoints.Length > 0 && navAgent != null)
//        {
//            navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
//        }
//    }

//    void UpdateChase()
//    {
//        if (player == null || navAgent == null) return;

//        // 플레이어 위치로 이동
//        Vector3 targetPosition = hasTarget ? player.position : lastKnownPlayerPosition;
//        navAgent.SetDestination(targetPosition);

//        // 공격 범위 확인
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
//        if (distanceToPlayer <= attackRange && hasTarget)
//        {
//            ChangeState(AIState.Attack);
//        }

//        // 애니메이션 업데이트
//        if (animator != null)
//        {
//            bool isMoving = navAgent.velocity.magnitude > 0.1f;
//            animator.SetBool(walkHash, isMoving);
//        }
//    }

//    void UpdateAttack()
//    {
//        if (player == null) return;

//        // 플레이어를 향해 회전
//        Vector3 directionToPlayer = (player.position - transform.position).normalized;
//        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
//        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

//        // 공격 쿨다운 확인
//        if (Time.time >= lastAttackTime + attackCooldown)
//        {
//            PerformAttack();
//        }

//        // 공격 범위를 벗어났는지 확인
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
//        if (distanceToPlayer > attackRange * 1.2f) // 약간의 여유를 둠
//        {
//            ChangeState(AIState.Chase);
//        }
//    }

//    void PerformAttack()
//    {
//        lastAttackTime = Time.time;

//        if (animator != null)
//        {
//            animator.SetTrigger(attackHash);
//        }

//        // EnemyController의 공격 메서드 호출
//        if (enemyController != null)
//        {
//            enemyController.AttackPlayer();
//        }
//    }

//    void UpdateHit()
//    {
//        // 피격 상태 - 애니메이션이 끝날 때까지 대기
//        // 애니메이션 이벤트로 상태 전환될 예정
//    }

//    void UpdateDead()
//    {
//        // 사망 상태 - 아무것도 하지 않음
//    }

//    // 외부에서 호출할 수 있는 메서드들
//    public void OnHit()
//    {
//        if (currentState != AIState.Dead)
//        {
//            ChangeState(AIState.Hit);

//            // 플레이어를 타겟으로 설정 (반격)
//            hasTarget = true;
//            loseTargetTimer = 0f;
//        }
//    }

//    public void OnDeath()
//    {
//        ChangeState(AIState.Dead);
//    }

//    // 애니메이션 이벤트로 호출될 메서드들
//    public void OnHitAnimationEnd()
//    {
//        if (hasTarget)
//            ChangeState(AIState.Chase);
//        else
//            ChangeState(usePatrol ? AIState.Patrol : AIState.Idle);
//    }

//    public void OnAttackAnimationEnd()
//    {
//        // 공격 애니메이션 완료 후 상태 결정
//        if (hasTarget)
//        {
//            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
//            if (distanceToPlayer <= attackRange)
//                ChangeState(AIState.Attack); // 계속 공격
//            else
//                ChangeState(AIState.Chase); // 추적
//        }
//        else
//        {
//            ChangeState(usePatrol ? AIState.Patrol : AIState.Idle);
//        }
//    }

//    // 디버그용 기즈모
//    void OnDrawGizmosSelected()
//    {
//        // 감지 범위
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, detectionRange);

//        // 공격 범위
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, attackRange);

//        // 패트롤 경로
//        if (usePatrol && patrolPoints != null && patrolPoints.Length > 1)
//        {
//            Gizmos.color = Color.blue;
//            for (int i = 0; i < patrolPoints.Length; i++)
//            {
//                if (patrolPoints[i] != null)
//                {
//                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);

//                    // 다음 포인트와 연결선
//                    int nextIndex = (i + 1) % patrolPoints.Length;
//                    if (patrolPoints[nextIndex] != null)
//                    {
//                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
//                    }
//                }
//            }
//        }
//    }
//}
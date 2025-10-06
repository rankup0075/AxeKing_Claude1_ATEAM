using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

[RequireComponent(typeof(BossHealth))]
public class BossController : MonoBehaviour
{
    [System.Serializable]
    public class Phase
    {
        public string name = "Phase";
        public int startHP = 100;
        public float idleBeforeStart = 0.8f;
        public List<BossSkill> skills = new List<BossSkill>();
        public float phaseMusicFadeDelay = 0.2f;
    }

    [Header("Phases (�������)")]
    public Phase[] phases;

    [Header("General")]
    public float timeBetweenAttacks = 0.5f;
    public Transform playerTarget;
    public LayerMask playerLayerMask;
    public Transform[] globalSkillSpawnPoints;

    [Header("VFX / SFX")]
    public GameObject phaseTransitionVFX;
    public float phaseTransitionPause = 1.2f;

    private int currentPhaseIndex = -1;
    private BossHealth bossHealth;
    private Coroutine attackLoopCoroutine;
    private bool isAlive = true;
    private bool isInvulnerable = false;


    [Header("Movement")]
    public bool enableMovement = true;
    public float moveSpeed = 3f;
    public float followDistance = 3f; // �� �Ÿ� ���̸� �߰� ����
    [Range(0f, 1f)] public float idleChance = 0.3f; // ���� Ȯ���� ������ �߰����� ����
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    private Coroutine moveCoroutine;
    private bool isExecutingSkill = false;

    // Animator �߰�
    private Animator animator;

    [Header("Grounding")]
    public LayerMask groundLayerMask; // ���� ���̾� ����

    void Start()
    {
        moveCoroutine = StartCoroutine(MoveRoutine());
        bossHealth = GetComponent<BossHealth>();

        // Animator ���� �ʱ�ȭ: �ڽ�, �ڽ�, �Ǵ� ���� ���� ������Ʈ Ž��
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerTarget == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) playerTarget = p.transform;
        }

        StartNextPhase();
    }


    void StartNextPhase()
    {
        currentPhaseIndex++;
        if (currentPhaseIndex >= phases.Length)
        {
            FinalDeath();
            return;
        }

        Phase p = phases[currentPhaseIndex];
        bossHealth.ResetTo(p.startHP);
        StartCoroutine(PhaseStartRoutine(p));
    }

    IEnumerator PhaseStartRoutine(Phase p)
    {
        isInvulnerable = true;
        if (phaseTransitionVFX != null)
        {
            Instantiate(phaseTransitionVFX, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(p.idleBeforeStart);
        isInvulnerable = false;

        if (attackLoopCoroutine != null) StopCoroutine(attackLoopCoroutine);
        attackLoopCoroutine = StartCoroutine(AttackLoop(p));
    }

    IEnumerator AttackLoop(Phase p)
    {
        while (isAlive && bossHealth.currentHP > 0)
        {
            if (p.skills == null || p.skills.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            var skill = p.skills[Random.Range(0, p.skills.Count)];
            yield return StartCoroutine(ExecuteSkill(skill));
            float wait = Mathf.Max(skill.cooldown, timeBetweenAttacks);
            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator ExecuteSkill(BossSkill skill)
    {
        if (skill == null) yield break;

        isExecutingSkill = true;

        if (animator != null && !string.IsNullOrEmpty(skill.animationTrigger))
            animator.SetTrigger(skill.animationTrigger);

        float waitFor = Mathf.Max(skill.castDelay, skill.animationLength);
        if (waitFor > 0f)
            yield return new WaitForSeconds(waitFor);

        switch (skill.type)
        {
            case BossSkill.SkillType.Projectile:
                DoProjectile(skill);
                break;
            case BossSkill.SkillType.AOE:
                yield return StartCoroutine(DoAOE(skill));
                break;
            case BossSkill.SkillType.Laser:
                yield return StartCoroutine(DoLaser(skill));
                break;
        }
        isExecutingSkill = false;
    }

    void DoProjectile(BossSkill skill)
    {
        if (skill.projectilePrefab == null) return;

        Transform spawn = (skill.spawnPoint != null) ? skill.spawnPoint : transform;
        GameObject proj = Instantiate(skill.projectilePrefab, spawn.position, spawn.rotation);
        var rb = proj.GetComponent<Rigidbody>();
        Vector3 dir = Vector3.forward;
        if (playerTarget != null)
            dir = (playerTarget.position - spawn.position).normalized;
        if (rb != null) rb.linearVelocity = dir * skill.projectileSpeed;

        Component projectileComp = null;
        foreach (var comp in proj.GetComponents<Component>())
        {
            if (comp == null) continue;
            var tname = comp.GetType().Name;
            if (tname == "Projectile" || tname.EndsWith("Projectile"))
            {
                projectileComp = comp;
                break;
            }
        }

        if (projectileComp == null)
        {
            var sp = proj.AddComponent<SimpleProjectile>();
            sp.Init(skill.damage, skill.projectileLife, playerLayerMask);
        }

        Destroy(proj, skill.projectileLife);
    }

    IEnumerator DoAOE(BossSkill skill)
    {
        // 1) �߽� ��� (���� aoeTarget �б� ����)
        Vector3 center = transform.position;
        switch (skill.aoeTarget)
        {
            case BossSkill.AoeTargetMode.SpawnPoint:
                center = (skill.spawnPoint != null) ? skill.spawnPoint.position : transform.position;
                break;
            case BossSkill.AoeTargetMode.Boss:
                center = transform.position;
                break;
            case BossSkill.AoeTargetMode.Player:
                if (playerTarget != null) center = playerTarget.position;
                break;
            case BossSkill.AoeTargetMode.RandomAroundPlayer:
                if (playerTarget != null)
                    center = playerTarget.position + (Vector3)(Random.insideUnitCircle * skill.randomRadius);
                else
                    center = transform.position + (Vector3)(Random.insideUnitCircle * skill.randomRadius);
                break;
            case BossSkill.AoeTargetMode.RandomAroundBoss:
                center = transform.position + (Vector3)(Random.insideUnitCircle * skill.randomRadius);
                break;
        }

        // 2) �������� ���� (groundLayerMask �ʿ�). �����ϸ� ���� center ����.
        RaycastHit groundHit;
        Vector3 rayStart = center + Vector3.up * 10f;
        if (Physics.Raycast(rayStart, Vector3.down, out groundHit, 50f, groundLayerMask))
        {
            center = groundHit.point;
        }
        else
        {
            center.y += 0.05f;
        }

        // 3) preview ǥ�� + ���� ����ȭ �˻�
        GameObject preview = null;
        if (skill.previewVFX != null)
        {
            preview = Instantiate(skill.previewVFX, center + Vector3.up * 0.02f, Quaternion.identity);

            float diameter = Mathf.Max(0.01f, skill.aoeRadius * 2f);
            preview.transform.localScale = new Vector3(diameter, 0.2f, diameter);

            // ���־� �����̹Ƿ� Collider ��Ȱ��ȭ
            foreach (var col in preview.GetComponentsInChildren<Collider>()) col.enabled = false;
            // ������ ���� Ȱ��ȭ(�������� �����־ ���̰�)
            foreach (var r in preview.GetComponentsInChildren<Renderer>()) r.enabled = true;

            // yield �� ������ �༭ �ν��Ͻ��� �׷����� ��
            yield return null;
        }
        else
        {
            // preview prefab�� �߸��Ǿ� ������ �ӽ� �������� ǥ�� (����׿�)
            preview = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            preview.transform.position = center + Vector3.up * 0.02f;
            float diameter = Mathf.Max(0.01f, skill.aoeRadius * 2f);
            preview.transform.localScale = new Vector3(diameter, 0.02f, diameter);
            // �ö��̴� ����
            foreach (var col in preview.GetComponentsInChildren<Collider>()) Destroy(col);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.green;
            foreach (var r in preview.GetComponentsInChildren<Renderer>()) r.material = mat;
            yield return null;
        }

        Debug.Log($"[Boss] AOE preview at {center} radius {skill.aoeRadius} (delay {skill.aoeDelay})");

        // 4) ��� (�÷��̾ ���� ���� �ð�)
        if (skill.aoeDelay > 0f) yield return new WaitForSeconds(skill.aoeDelay);

        // 5) ���� ���� preview ����(������)
        if (preview != null) Destroy(preview);

        // 6) ����: OverlapSphere�� �浹�� ã��. �÷��̾� '��' ���� �˻�� ���� ȸ�� ����
        Collider[] hits = Physics.OverlapSphere(center, skill.aoeRadius, playerLayerMask);
        Debug.Log($"[Boss] AOE explode hits count: {hits.Length}");
        foreach (var c in hits)
        {
            if (c == null) continue;

            float feetY = c.bounds.min.y;

            if (skill.maxVerticalOffset > 0f && feetY > center.y + skill.maxVerticalOffset)
            {
                Debug.Log($"[Boss] {c.name} avoided by vertical offset. feetY={feetY:F2} centerY={center.y:F2}");
                continue;
            }

            // ������ ����
            c.gameObject.SendMessage("TakeDamage", skill.damage, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[Boss] AOE attempted damage {c.name} for {skill.damage}");

            // ���� HP ����� ���: PlayerHealth�� ������ �ʵ�/������Ƽ�� ���÷������� ã�Ƽ� ���
            var ph = c.GetComponent("PlayerHealth");
            string hpLog = "unknown";
            if (ph != null)
            {
                object hpVal = TryExtractHP(ph);
                if (hpVal != null) hpLog = hpVal.ToString();
                Debug.Log($"[Boss] {c.name} remaining HP (reflection) = {hpLog}");
            }
        }

        // 7) ����Ʈ VFX ���� �� ���� ����
        if (skill.impactVFX != null)
        {
            var imp = Instantiate(skill.impactVFX, center, Quaternion.identity);
            foreach (var col in imp.GetComponentsInChildren<Collider>()) col.enabled = false;
            Destroy(imp, 3f);
        }

        yield break;

        // ���� �Լ�: ���÷������� ���� �ʵ�/������Ƽ �̸� ã�� HP ��ȯ
        object TryExtractHP(object comp)
        {
            if (comp == null) return null;
            var t = comp.GetType();
            string[] names = new string[] { "currentHP", "currentHealth", "hp", "health", "CurrentHP", "Health", "HP" };
            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null)
                {
                    var v = f.GetValue(comp);
                    if (v != null) return v;
                }
                var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.CanRead)
                {
                    var v2 = p.GetValue(comp);
                    if (v2 != null) return v2;
                }
            }
            return null;
        }
    }





    IEnumerator DoLaser(BossSkill skill)
    {
        float elapsed = 0f;
        while (elapsed < skill.laserDuration)
        {
            if (playerTarget != null)
            {
                Vector3 dir = (playerTarget.position - transform.position).normalized;
                Ray ray = new Ray(transform.position, dir);
                if (Physics.Raycast(ray, out RaycastHit hit, skill.laserRange))
                {
                    var ph = hit.collider.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.SendMessage("TakeDamage", skill.damage, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            elapsed += skill.laserTickInterval;
            yield return new WaitForSeconds(skill.laserTickInterval);
        }
    }

    public void OnPhaseEnded()
    {
        if (!isAlive) return;
        StartCoroutine(PhaseEndRoutine());
    }

    IEnumerator PhaseEndRoutine()
    {
        isInvulnerable = true;
        if (attackLoopCoroutine != null) StopCoroutine(attackLoopCoroutine);

        if (phaseTransitionVFX != null) Instantiate(phaseTransitionVFX, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(phaseTransitionPause);

        StartNextPhase();
    }

    void FinalDeath()
    {
        isAlive = false;
        Debug.Log("[BossController] Final death. Drop rewards and destroy.");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (phases != null && currentPhaseIndex >= 0 && currentPhaseIndex < phases.Length)
        {
            var p = phases[currentPhaseIndex];
            if (p.skills != null)
            {
                foreach (var s in p.skills)
                {
                    if (s != null && s.type == BossSkill.SkillType.AOE)
                    {
                        Transform sp = s.spawnPoint != null ? s.spawnPoint : transform;
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(sp.position, s.aoeRadius);
                    }
                }
            }
        }
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ�� ����
    public void OnAnimationHit()
    {
        Debug.Log("Animation hit event received.");
    }

    IEnumerator MoveRoutine()
    {
        if (!enableMovement) yield break;
        while (isAlive)
        {
            if (isInvulnerable || isExecutingSkill || playerTarget == null)
            {
                yield return null;
                continue;
            }

            // �Ǵ�: ���� or �߰�
            if (Random.value < idleChance)
            {
                float idleT = Random.Range(minIdleTime, maxIdleTime);
                float t = 0f;
                while (t < idleT)
                {
                    if (!isExecutingSkill) t += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                // �߰� ����: follow until within followDistance or interrupted
                while (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) > followDistance)
                {
                    if (isExecutingSkill || isInvulnerable) break;
                    Vector3 targetPos = playerTarget.position;
                    targetPos.y = transform.position.y;
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    // optional: look at player
                    Vector3 look = (playerTarget.position - transform.position);
                    look.y = 0;
                    if (look.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(look);
                    yield return null;
                }
                // ª�� �޽�
                yield return new WaitForSeconds(0.2f);
            }
            yield return null;
        }
    }

}

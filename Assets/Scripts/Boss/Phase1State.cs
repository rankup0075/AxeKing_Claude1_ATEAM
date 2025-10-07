using System.Collections;
using UnityEngine;

public class Phase1State : IBossState
{
    readonly BossController boss;
    readonly Phase1Config cfg;

    float nextDecisionAt;
    bool willChase;
    bool isAttacking = false;

    public Phase1State(BossController boss, Phase1Config cfg)
    {
        this.boss = boss;
        this.cfg = cfg;
    }

    public void Enter()
    {
        DecideMove();
    }

    public void Tick()
    {
        if (!boss.HasPlayer || isAttacking) return;

        if (Time.time >= nextDecisionAt) DecideMove();

        if (willChase)
            DoChase();
        else
            boss.SetBool("IsMoving", false);

        // 랜덤 공격 시도
        boss.StartCoroutine(AttackRoutine());
    }

    public void Exit() { }

    void DecideMove()
    {
        nextDecisionAt = Time.time + Mathf.Max(0.5f, cfg.decisionInterval);
        willChase = Random.value < cfg.chaseProbability;
        boss.SetBool("IsMoving", willChase);
    }

    void DoChase()
    {
        var root = boss.transform;
        var player = boss.Player;
        if (!player) { boss.SetBool("IsMoving", false); return; }

        Vector3 dir = player.position - root.position;
        dir.y = 0f;
        float dist = dir.magnitude;
        if (dist < 0.5f) { boss.SetBool("IsMoving", false); return; }

        dir.Normalize();

        // 여기서 실제로는 이동하지 않고, 물리 루프에 델타를 넘김
        boss.pendingMove = dir * cfg.moveSpeed * Time.fixedDeltaTime;

        // 루트 회전
        if (dir.sqrMagnitude > 0f)
            root.rotation = Quaternion.Slerp(root.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        boss.SetBool("IsMoving", true);
    }


    IEnumerator AttackRoutine()
    {
        if (isAttacking) yield break;
        isAttacking = true;

        // ===== 스킬 선택 =====
        float wBasic = cfg.basicAttackWeight;
        float wA = cfg.skillA_ExplodeWeight;
        float wB = cfg.skillB_ProjectileWeight;
        float wC = cfg.skillC_LaserWeight;
        float sum = wBasic + wA + wB + wC;
        float r = Random.value * sum;

        if ((r -= wBasic) < 0f)
            yield return boss.StartCoroutine(DoBasicProjectile());
        else if ((r -= wA) < 0f)
            yield return boss.StartCoroutine(DoSkillA());
        else if ((r -= wB) < 0f)
            yield return boss.StartCoroutine(DoSkillB());
        else
            yield return boss.StartCoroutine(DoSkillC());

        // 공격 간 대기 (느리게)
        yield return new WaitForSeconds(Random.Range(cfg.attackIntervalRange.x, cfg.attackIntervalRange.y) + 1.5f);
        isAttacking = false;
    }

    // ======================= 공격 =======================
    Collider[] BossCols()
    {
        return boss.GetComponentsInChildren<Collider>(includeInactive: false);
    }

    IEnumerator DoBasicProjectile()
    {
        boss.FacePlayerImmediate();
        boss.SetTrigger("Attack_Projectile");
        yield return new WaitForSeconds(0.4f);

        if (!boss.HasPlayer || cfg.basicProjectilePrefab == null || boss.firePoint == null) yield break;

        Vector3 target = boss.GetPlayerAimPoint();
        Vector3 dir = (target - boss.firePoint.position).normalized;
        var go = GameObject.Instantiate(cfg.basicProjectilePrefab, boss.firePoint.position, Quaternion.LookRotation(dir));
        var pr = go.GetComponent<BossProjectile>();
        pr.Launch(dir, cfg.basicProjectileSpeed, cfg.basicProjectileDamage, boss.playerLayer, BossCols());
        yield return null;
    }

    IEnumerator DoSkillA()
    {
        boss.FacePlayerImmediate();
        // 프리뷰 (애니메이션 없음)
        var preview = GameObject.Instantiate(cfg.aoePreviewPrefab);
        var pv = preview.GetComponent<BossAOEPreview>();
        pv.Setup(cfg.aoeRadius, 0.01f);

        float end = Time.time + cfg.aoeTrackSeconds;
        while (Time.time < end && boss.HasPlayer)
        {
            Vector3 pos = boss.GetPlayerAimPoint();
            pos.y = 0f;
            pv.SetPreviewCenter(pos);
            yield return null;
        }

        pv.Hide();
        yield return new WaitForSeconds(cfg.aoeDelayAfterPreview);

        // 폭발 시점에 애니메이션
        boss.SetTrigger("SkillA_Explode");

        Vector3 spawn = pv.LastCenter;
        var dmgGo = GameObject.Instantiate(cfg.aoeDamagePrefab, spawn, Quaternion.identity);
        var area = dmgGo.GetComponent<BossExplosionArea>();
        area.Activate(cfg.aoeRadius, cfg.aoeHeight, cfg.aoeTickDamage, cfg.aoeTickInterval, cfg.aoeLife, boss.playerLayer);

        GameObject.Destroy(preview);
    }

    IEnumerator DoSkillB()
    {
        boss.FacePlayerImmediate();
        boss.SetTrigger("SkillB_Shot");
        yield return new WaitForSeconds(0.5f);

        if (!boss.HasPlayer || cfg.skillProjectilePrefab == null || boss.firePoint == null) yield break;

        Vector3 target = boss.GetPlayerAimPoint();
        Vector3 dir = (target - boss.firePoint.position).normalized;
        var go = GameObject.Instantiate(cfg.skillProjectilePrefab, boss.firePoint.position, Quaternion.LookRotation(dir));
        var pr = go.GetComponent<BossProjectile>();
        pr.Launch(dir, cfg.skillProjectileSpeed, cfg.skillProjectileDamage, boss.playerLayer, BossCols());
    }

    IEnumerator DoSkillC()
    {
        boss.FacePlayerImmediate();
        // 프리뷰
        var pv = GameObject.Instantiate(cfg.laserPreviewPrefab).GetComponent<BossLaser>();
        pv.Setup(true, cfg.laserThickness, 0, boss.playerLayer);

        float end = Time.time + cfg.laserTrackYSeconds;
        while (Time.time < end && boss.HasPlayer)
        {
            float y = boss.GetPlayerAimPoint().y;
            pv.AlignHorizontalAtY(y);
            yield return null;
        }

        pv.Hide();
        yield return new WaitForSeconds(cfg.laserDelayAfterPreview);

        // 레이저 발사 애니메이션
        boss.SetTrigger("SkillC_Laser");

        var hit = GameObject.Instantiate(cfg.laserHitPrefab).GetComponent<BossLaser>();
        hit.Setup(false, cfg.laserThickness, cfg.laserHitDamage, boss.playerLayer);
        hit.AlignHorizontalAtY(pv.LastY);
        hit.FireOnceThenDie(1f);

        GameObject.Destroy(pv.gameObject);
    }
}

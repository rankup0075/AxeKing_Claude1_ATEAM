using UnityEngine;
using System.Collections;

public enum BossPhase { Phase1, Phase2, Phase3 }

[RequireComponent(typeof(Animator))]
public class BossAI : MonoBehaviour, IDamageable
{
    public BossConfig config;
    public BossPhase phase = BossPhase.Phase1;
    public Transform player;
    public LayerMask groundMask;
    public float arenaHorizontalSpan = 40f; // 가로 레이저 길이
    public float arenaVerticalSpan = 24f;   // 세로 레이저 길이

    [Header("Visual")]
    [SerializeField] Animator anim;        // God Boss의 Animator 할당
    [SerializeField] Transform visualRoot; // God Boss Transform
    public bool faceToPlayer = true;
    public float turnLerp = 12f;

    bool _isBusy => _isAttacking || _isExploding;

    public Transform firePoint;

    Animator _anim;
    float _nextBasic;
    bool _shielding;

    bool _isAttacking;

    void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = anim ? anim.transform : transform;
        _anim = anim; // ← 추가
    }

    void Update()
    {
        if (!player) return;
        switch (phase)
        {
            case BossPhase.Phase1: TickPhase1(); FaceToPlayerXZ(); break;
            case BossPhase.Phase2: TickPhase2(); FaceToPlayerXZ(); break;
            case BossPhase.Phase3: TickPhase3(); FaceToPlayerXZ(); break;
        }
    }

    // ===== 공통 유틸 =====
    void SetMoveAnim(float speed)
    {
        if (anim) anim.SetFloat(config.animParamMoveSpeed, speed);
    }
    void LateUpdate() { FaceToPlayerXZ(); }
    void FaceToPlayerXZ()
    {
        if (!faceToPlayer || !player || !visualRoot) return;
        var dir = player.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude < 1e-4f) return;
        var q = Quaternion.LookRotation(dir.normalized, Vector3.up);
        visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, q, turnLerp * Time.deltaTime);
    }

    bool ReadyBasic() => Time.time >= _nextBasic;
    void SetBasicCooldown(float cd) => _nextBasic = Time.time + cd;

    Vector3 DirToPlayerXZ()
    {
        var d = player.position - transform.position;
        d.y = 0;
        return d.normalized;
    }

    // ===== Phase 1 =====
    float _p1StateUntil;
    bool _p1Following;

    void TickPhase1()
    {
        var p1 = config.phase1;

        if (_isBusy)
        {
            SetMoveAnim(0f);
            return;
        }

        // --- 이하 기존 이동 + 공격 로직 ---
        if (Time.time >= _p1StateUntil)
        {
            _p1Following = Random.value < p1.followChance;
            _p1StateUntil = Time.time + Random.Range(p1.idleTimeMin, p1.idleTimeMax);
        }

        if (_p1Following)
        {
            transform.position += DirToPlayerXZ() * p1.moveSpeed * Time.deltaTime;
            SetMoveAnim(p1.moveSpeed);
        }
        else
            SetMoveAnim(0f);

        if (!IsInvoking(nameof(P1_AttackDecision)))
        {
            float next = Random.Range(p1.attackInterval.x, p1.attackInterval.y);
            Invoke(nameof(P1_AttackDecision), next);
        }
    }

    // === 통합 공격 확률 로직 추가 ===
    void P1_AttackDecision()
    {
        if (_isBusy) return; // 공격 중이면 무시

        var p1 = config.phase1;

        // 전체 확률 합
        float total = p1.basicAttackChance + p1.explodeChance + p1.projChance + p1.laserChance;
        if (total <= 0f) return;

        float roll = Random.value * total;

        if (roll < p1.basicAttackChance)
        {
            TryBasicAttack();
        }
        else if (roll < p1.basicAttackChance + p1.explodeChance)
        {
            StartCoroutine(P1_Explode());
        }
        else if (roll < p1.basicAttackChance + p1.explodeChance + p1.projChance)
        {
            StartCoroutine(P1_SkillProjectile());
        }
        else
        {
            StartCoroutine(P1_HLaser());
        }

        // 다음 공격 결정 예약
        float next = Random.Range(p1.attackInterval.x, p1.attackInterval.y);
        Invoke(nameof(P1_AttackDecision), next);
    }   

    // --- 새로 추가 ---
    void TryBasicAttack()
    {
        anim.ResetTrigger(config.animTriggerBasic);

        if (_isAttacking || _isExploding) return;  // 스킬 중이면 발사 금지
        _isAttacking = true;

        anim.SetTrigger(config.animTriggerBasic);
        StartCoroutine(ThrowProjectile(0.4f));

        SetBasicCooldown(config.basicAttackCooldown);

        // 공격 애니메이션 끝난 뒤 이동 가능하도록 복귀
        StartCoroutine(EndAttackAfter(1.0f)); // 애니메이션 길이에 맞게
    }

    IEnumerator EndAttackAfter(float t)
    {
        yield return new WaitForSeconds(t);
        _isAttacking = false;
    }

    // --- 새로 추가 ---
    IEnumerator ThrowProjectile(float delay)
    {
        Debug.Log("Start ThrowProjectile");
        yield return new WaitForSeconds(delay);

        Vector3 startPos = firePoint
            ? firePoint.position
            : transform.position + Vector3.up * 1.2f;

        if (config.basicProjectilePrefab == null)
        {
            Debug.LogError("basicProjectilePrefab is null!");
            yield break;
        }

        var go = Instantiate(config.basicProjectilePrefab, startPos, Quaternion.identity);
        if (go == null)
        {
            Debug.LogError("Instantiate failed");
            yield break;
        }

        var pr = go.GetComponent<Projectile>();
        if (pr == null)
        {
            Debug.LogError("Projectile component missing!");
            yield break;
        }

        pr.speed = config.basicProjectileSpeed;
        pr.life = config.basicProjectileLife;
        pr.damage = 10f;
        pr.hitLayers = config.playerLayer;
        pr.destroyOnAnyHit = true;
        pr.Fire(DirToPlayerXZ());
        Debug.Log("Projectile Fired");
    }

    bool _isExploding;
    IEnumerator P1_Explode()
    {
        if (_isExploding) yield break;   // 이미 폭발 중이면 무시
        _isExploding = true;

        var p1 = config.phase1;

        // 프리뷰 생성
        var preview = Instantiate(
            p1.explodePreviewPrefab,
            new Vector3(player.position.x, transform.position.y, transform.position.z),
            Quaternion.identity);

        float t = 0f;
        while (t < p1.explodeTrackSeconds)
        {
            if (preview != null)
            {
                Vector3 follow = new Vector3(player.position.x, transform.position.y, transform.position.z);
                preview.transform.position = follow;
            }
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(p1.explodeDelay);

        // 폭발 직전 애니메이션 트리거
        anim.ResetTrigger(config.animTriggerSkillA);   // 중복 방지
        anim.SetTrigger(config.animTriggerSkillA);

        // 폭발 생성
        var explosion = Instantiate(p1.explodeEffectPrefab, preview.transform.position, Quaternion.identity);
        var col = explosion.GetComponent<CapsuleCollider>();
        if (col)
        {
            col.radius = p1.explodeRadius;
            col.height = p1.explodeHeight;
        }

        // 8. 폭발 콜라이더와 DamageArea 설정
        var da = explosion.GetComponent<DamageArea>();
        if (da)
        {
            da.damage = p1.explodeDamagePerTick;
            da.continuous = true;
            da.tickInterval = p1.explodeTickInterval;
            da.life = p1.explodeLife;
            da.targetLayer = config.playerLayer;
        }

        // 9. 프리뷰 제거
        Destroy(preview, 0.1f);

        // 10. 폭발 이펙트가 끝날 때까지 대기
        yield return new WaitForSeconds(p1.explodeLife);

        // 11. 스킬 종료 → 이동 가능
        _isExploding = false;
    }




    IEnumerator P1_SkillProjectile()
    {
        if (_isExploding || _isAttacking) yield break; // 다른 행동 중엔 금지
        _isExploding = true; // 행동 락

        var p1 = config.phase1;

        // 1. 애니메이션 트리거
        anim.ResetTrigger(config.animTriggerSkillB);
        anim.SetTrigger(config.animTriggerSkillB);

        // 2. 플레이어의 마지막 위치 기록
        Vector3 aimPos = player.position;
        aimPos.y = transform.position.y + 1.2f; // 보스 눈높이 혹은 firePoint 높이에 맞춤
        yield return new WaitForSeconds(0.3f); // 모션 선딜 타이밍 맞춰 조정

        // 3. 투사체 생성
        Vector3 firePos = firePoint
            ? firePoint.position
            : transform.position + Vector3.up * 1.2f;
        var go = Instantiate(p1.skillProjectilePrefab, firePos, Quaternion.identity);

        var pr = go.GetComponent<Projectile>();
        if (pr)
        {
            pr.speed = p1.skillProjectileSpeed;
            pr.life = p1.skillProjectileLife;
            pr.damage = p1.skillProjectileDamage;
            pr.hitLayers = config.playerLayer; // 필요 시 Player+Wall
            pr.destroyOnAnyHit = true;

            // 조준 방향 수평 정렬
            Vector3 dir = aimPos - firePos;
            dir.y = 0; // 수평 발사
            pr.Fire(dir.normalized);
        }

        // 4. 애니메이션 및 투사체 종료까지 대기
        yield return new WaitForSeconds(0.8f);
        _isExploding = false;
    }

    
    IEnumerator P1_HLaser()
    {
        if (_isExploding || _isAttacking) yield break;
        _isExploding = true;

        var p1 = config.phase1;

        float y = player.position.y + p1.laserYOffset;

        // === 프리뷰 시작 ===
        var preview = Instantiate(
            p1.laserPreviewPrefab,
            new Vector3(transform.position.x, y, transform.position.z),
            Quaternion.identity);

        preview.transform.localScale = new Vector3(arenaHorizontalSpan, 1f, p1.laserWidth);

        // 프리뷰 시작 시 애니메이션(기 모으기) 트리거
        anim.ResetTrigger(config.animTriggerSkillC);
        anim.SetTrigger(config.animTriggerSkillC);

        // === 2초 안에 추적 + 잠깐 사라짐을 모두 포함시킴 ===
        float totalChargeTime = 2f;   // 기 모으는 총 시간
        float vanishGap = 0.5f;         // 프리뷰 사라지고 잠깐의 공백
        float trackTime = Mathf.Max(0f, totalChargeTime - vanishGap);

        // (1) trackTime 동안 플레이어 Y좌표 추적
        float t = 0f;
        while (t < trackTime)
        {
            y = player.position.y + p1.laserYOffset;
            if (preview)
                preview.transform.position =
                    new Vector3(transform.position.x, y, transform.position.z);
            t += Time.deltaTime;
            yield return null;
        }

        // (2) 잠깐의 사라지는 공백
        if (preview) Destroy(preview);
        yield return new WaitForSeconds(vanishGap);

        // === 1.41초 시점: 두 팔 벌리기 + 레이저 발사 ===
        var beam = Instantiate(
            p1.laserBeamPrefab,
            new Vector3(transform.position.x, y, transform.position.z),
            Quaternion.identity);

        var ls = beam.GetComponent<LaserSweep>();
        if (ls)
        {
            ls.width = p1.laserWidth;
            ls.damageOnce = p1.laserDamageOnce;
            ls.life = p1.laserLife;
            ls.targetLayer = config.playerLayer;
            ls.ApplySpan(arenaHorizontalSpan, arenaVerticalSpan);

            bool horiz = ls.horizontal;
            float len = horiz ? arenaHorizontalSpan : arenaVerticalSpan;
            float wid = p1.laserWidth;
            beam.transform.localScale = horiz
                ? new Vector3(len, 1f, wid)
                : new Vector3(wid, 1f, len);
        }

        // === 애니메이션 전체 3.15초 동안 이동 금지 유지 ===
        float totalAnim = 3.15f;
        float remain = Mathf.Max(0f, totalAnim - totalChargeTime);
        yield return new WaitForSeconds(remain);

        _isExploding = false;
    }



    // ===== Phase 2 =====
    float _p2NextBasic;
    bool _p2Fast;

    void TickPhase2()
    {
        var p2 = config.phase2;

        // 이동
        if (!IsInvoking(nameof(P2_MoveRoll)))
            Invoke(nameof(P2_MoveRoll), 0.7f);
        float ms = _p2Fast ? p2.fastSpeed : p2.slowSpeed;
        transform.position += DirToPlayerXZ() * ms * Time.deltaTime;
        SetMoveAnim(ms);

        // 기본 공격: 주먹 → 전방 파동
        if (Time.time >= _p2NextBasic)
        {
            _anim.SetTrigger(config.animTriggerBasic);
            StartCoroutine(P2_FistWave(p2));
            _p2NextBasic = Time.time + p2.basicCooldown;
        }

        // 스킬
        if (!IsInvoking(nameof(P2_SkillGate)))
        {
            float next = Random.Range(p2.skillInterval.x, p2.skillInterval.y);
            Invoke(nameof(P2_SkillGate), next);
        }
    }

    void P2_MoveRoll()
    {
        var p2 = config.phase2;
        _p2Fast = Random.value < p2.fastApproachChance;
    }

    IEnumerator P2_FistWave(BossConfig.Phase2 p2)
    {
        yield return new WaitForSeconds(0.1f); // 모션 선딜 가정
        // 보스 앞 히트박스 확장
        var wave = Instantiate(p2.fistWavePrefab, transform.position + transform.forward * (p2.fistRange * 0.5f), transform.rotation);
        var da = wave.GetComponent<DamageArea>();
        if (da)
        {
            da.damage = p2.fistDamage;
            da.continuous = false;
            da.oneShotPerEnter = true;
            da.life = p2.fistWaveDuration;
            da.targetLayer = config.playerLayer;
        }
        Destroy(wave, p2.fistWaveDuration);
        yield return null;
    }

    void P2_SkillGate()
    {
        float r = Random.value;
        if (r < 0.34f) StartCoroutine(P2_Slam());
        else if (r < 0.67f) StartCoroutine(P2_Shield());
        else StartCoroutine(P2_Dash());
    }

    IEnumerator P2_Slam()
    {
        var p2 = config.phase2;
        _anim.SetTrigger(config.animTriggerSkillA);
        yield return new WaitForSeconds(0.15f); // 내려찍기 타이밍

        float step = p2.slamSpan / (p2.slamCount - 1);
        for (int i = 0; i < p2.slamCount; i++)
        {
            float offset = -p2.slamSpan * 0.5f + step * i;
            foreach (int dir in new int[] { -1, 1 })
            {
                var pos = transform.position + transform.right * offset * dir;
                var fx = Instantiate(p2.slamEffectPrefab, pos, Quaternion.identity);
                var da = fx.GetComponent<DamageArea>();
                if (da)
                {
                    da.damage = p2.slamDamage;
                    da.oneShotPerEnter = true;
                    da.targetLayer = config.playerLayer;
                }
                Destroy(fx, 1.5f);
            }
            yield return new WaitForSeconds(p2.slamStepDelay);
        }
    }

    IEnumerator P2_Shield()
    {
        if (_shielding) yield break;
        var p2 = config.phase2;
        _anim.SetTrigger(config.animTriggerSkillB);
        _shielding = true;

        var shield = Instantiate(p2.shieldPrefab, transform);
        var da = shield.GetComponent<DamageArea>();
        if (da)
        {
            da.damage = p2.shieldTouchDamage;
            da.continuous = true;
            da.tickInterval = 0.2f;
            da.targetLayer = config.playerLayer;
            da.life = p2.shieldDuration;
        }
        yield return new WaitForSeconds(p2.shieldDuration);
        Destroy(shield);
        _shielding = false;
    }

    IEnumerator P2_Dash()
    {
        var p2 = config.phase2;
        _anim.SetTrigger(config.animTriggerSkillC);

        // 프리뷰
        var preview = Instantiate(p2.dashPreviewPrefab, transform.position, Quaternion.identity);
        var dirSign = Mathf.Sign(player.position.x - transform.position.x);
        var dashDir = new Vector3(dirSign, 0, 0); // x축만
        Destroy(preview, p2.dashWindup);

        yield return new WaitForSeconds(p2.dashWindup);

        // 돌진
        var trail = p2.dashTrailPrefab ? Instantiate(p2.dashTrailPrefab, transform) : null;
        float t = 0;
        Vector3 start = transform.position;
        Vector3 end = start + dashDir * p2.dashDistance;

        while (t < p2.dashTime)
        {
            float a = t / p2.dashTime;
            transform.position = Vector3.Lerp(start, end, a);
            // 히트박스
            var hits = Physics.OverlapSphere(transform.position, p2.dashHitboxRadius, config.playerLayer);
            foreach (var h in hits)
            {
                var d = h.GetComponentInParent<IDamageable>();
                if (d != null) d.ApplyDamage(p2.dashDamage);
            }
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
        if (trail) Destroy(trail, 0.5f);
    }

    // ===== Phase 3 =====
    void TickPhase3()
    {
        SetMoveAnim(0);

        // 기본 공격: 수직 레이저
        if (!IsInvoking(nameof(P3_BasicVLaser)))
        {
            float delay = Random.Range(1.2f, 1.8f);
            Invoke(nameof(P3_BasicVLaser), delay);
        }

        // 스킬
        if (!IsInvoking(nameof(P3_SkillGate)))
        {
            var p3 = config.phase3;
            float next = Random.Range(p3.skillInterval.x, p3.skillInterval.y);
            Invoke(nameof(P3_SkillGate), next);
        }
    }

    void P3_BasicVLaser() => StartCoroutine(P3_VLaser());

    IEnumerator P3_VLaser()
    {
        var p3 = config.phase3;

        // X 추적
        float t = 0; float x = player.position.x;
        while (t < p3.trackSecondsX)
        {
            x = player.position.x;
            t += Time.deltaTime;
            yield return null;
        }

        // 프리뷰 → 실제
        Vector3 pos = new Vector3(x, transform.position.y, transform.position.z);
        yield return PreviewSpawner.SpawnAfter(
            p3.vLaserPreviewPrefab, p3.vLaserBeamPrefab, pos, Quaternion.identity, p3.fireDelay,
            a =>
            {
                var ls = a.GetComponent<LaserSweep>();
                if (ls)
                {
                    ls.width = p3.beamWidth;
                    ls.damageOnce = p3.beamDamageOnce;
                    ls.life = p3.beamLife;
                    ls.SetSpan(arenaVerticalSpan, false);
                    var col = a.GetComponent<BoxCollider>();
                    if (col) col.isTrigger = true;
                }
            });
    }

    void P3_SkillGate()
    {
        float r = Random.value;
        if (r < 0.34f) StartCoroutine(P3_EyeBeams());
        else if (r < 0.67f) StartCoroutine(P3_GroundSweep());
        else StartCoroutine(P3_Summon());
    }

    IEnumerator P3_EyeBeams()
    {
        var p3 = config.phase3;

        // 두 갈래 레이저 트래킹 2초(지속 데미지)
        float t = 0;
        while (t < p3.eyeBeamTrackSeconds)
        {
            // 프리팹은 보스 눈 기준에서 플레이어 쪽으로 라인 히트 스캔 처리하도록 구현 권장
            var beam = Instantiate(p3.eyeBeamPrefab, transform.position + Vector3.up * 1.6f, Quaternion.LookRotation(DirToPlayerXZ()));
            var da = beam.GetComponent<DamageArea>();
            if (da)
            {
                da.damage = p3.eyeBeamTickDamage;
                da.continuous = true;
                da.tickInterval = p3.eyeBeamTickInterval;
                da.life = p3.eyeBeamTickInterval * 1.2f;
                da.targetLayer = config.playerLayer;
            }
            Destroy(beam, p3.eyeBeamTickInterval * 1.2f);

            // 두 개
            var beam2 = Instantiate(p3.eyeBeamPrefab, transform.position + Vector3.up * 1.6f, Quaternion.LookRotation(DirToPlayerXZ()));
            var da2 = beam2.GetComponent<DamageArea>();
            if (da2)
            {
                da2.damage = p3.eyeBeamTickDamage;
                da2.continuous = true;
                da2.tickInterval = p3.eyeBeamTickInterval;
                da2.life = p3.eyeBeamTickInterval * 1.2f;
                da2.targetLayer = config.playerLayer;
            }
            Destroy(beam2, p3.eyeBeamTickInterval * 1.2f);

            t += p3.eyeBeamTickInterval;
            yield return new WaitForSeconds(p3.eyeBeamTickInterval);
        }
    }

    IEnumerator P3_GroundSweep()
    {
        var p3 = config.phase3;
        // 프리뷰 → 손 쓸기 이펙트
        var pos = transform.position; // 필요 시 위치 조정
        yield return PreviewSpawner.SpawnAfter(
            p3.sweepPreviewPrefab, p3.sweepEffectPrefab, pos, transform.rotation, p3.sweepDelay,
            a =>
            {
                var da = a.GetComponent<DamageArea>();
                if (da)
                {
                    da.damage = p3.sweepDamageOnce;
                    da.oneShotPerEnter = true;
                    da.targetLayer = config.playerLayer;
                    da.life = p3.sweepLife;
                }
            });
    }

    IEnumerator P3_Summon()
    {
        yield return EnemySpawner.Summon(config.phase3, transform, groundMask);
    }

    // ===== 피격 처리 =====
    public void ApplyDamage(float amount)
    {
        if (_shielding) return; // 보호막 중 무적
        // 체력 시스템 연동 지점
        // Debug.Log($"Boss took {amount}");
    }
}

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
    public float arenaHorizontalSpan = 40f; // ���� ������ ����
    public float arenaVerticalSpan = 24f;   // ���� ������ ����

    [Header("Visual")]
    [SerializeField] Animator anim;        // God Boss�� Animator �Ҵ�
    [SerializeField] Transform visualRoot; // God Boss Transform
    public bool faceToPlayer = true;
    public float turnLerp = 12f;

    public Transform firePoint;

    Animator _anim;
    float _nextBasic;
    bool _shielding;

    bool _isAttacking;

    void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = anim ? anim.transform : transform;
        _anim = anim; // �� �߰�
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

    // ===== ���� ��ƿ =====
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

        // �̵�
        if (!_isAttacking)   // ���� ���̸� �̵� ����
        {
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
            {
                SetMoveAnim(0f);
            }
        }
        else
        {
            SetMoveAnim(0f); // ���� �߿� �׻� Idle ����
        }

        // ==== �⺻ ���� ====
        if (ReadyBasic() && Random.value < p1.basicAttackChance)
        {
            TryBasicAttack();   // �� ���� ������
        }

        // ==== ��ų ====
        if (!IsInvoking(nameof(P1_SkillGate)))
        {
            float next = Random.Range(p1.skillInterval.x, p1.skillInterval.y);
            Invoke(nameof(P1_SkillGate), next);
        }
    }

    // --- ���� �߰� ---
    void TryBasicAttack()
    {
        if (_isAttacking) return;        // �̹� ���� ���̸� ����
        _isAttacking = true;             // ���� ���� ����

        anim.SetTrigger(config.animTriggerBasic);
        StartCoroutine(ThrowProjectile(0.4f));

        SetBasicCooldown(config.basicAttackCooldown);

        // ���� �ִϸ��̼� ���� �� �̵� �����ϵ��� ����
        StartCoroutine(EndAttackAfter(1.0f)); // �ִϸ��̼� ���̿� �°�
    }

    IEnumerator EndAttackAfter(float t)
    {
        yield return new WaitForSeconds(t);
        _isAttacking = false;
    }

    // --- ���� �߰� ---
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

    void BasicRanged(GameObject prefab, float speed, float life, LayerMask layers, float dmg, Vector3? posOverride = null)
    {
        Vector3 startPos = posOverride ?? transform.position + Vector3.up * 1.2f;
        var go = Instantiate(prefab, startPos, Quaternion.identity);
        var pr = go.GetComponent<Projectile>();
        pr.speed = speed;
        pr.life = life;
        pr.damage = dmg;
        pr.hitLayers = layers;
        pr.destroyOnAnyHit = true;
        pr.Fire(DirToPlayerXZ());
    }

    void P1_SkillGate()
    {
        var p1 = config.phase1;
        float r = Random.value;
        if (r < p1.explodeChance) StartCoroutine(P1_Explode());
        else if (r < p1.explodeChance + p1.projChance) StartCoroutine(P1_SkillProjectile());
        else StartCoroutine(P1_HLaser());
    }

    IEnumerator P1_Explode()
    {
        var p1 = config.phase1;
        _anim.SetTrigger(config.animTriggerSkill);

        // X ���� 2��
        float t = 0;
        Vector3 pos = Vector3.zero;
        while (t < p1.explodeTrackSeconds)
        {
            pos = new Vector3(player.position.x, transform.position.y, transform.position.z);
            t += Time.deltaTime;
            yield return null;
        }

        // ������ �� ����
        yield return PreviewSpawner.SpawnAfter(
            p1.explodePreviewPrefab, p1.explodeEffectPrefab, pos, Quaternion.identity, p1.explodeDelay,
            a =>
            {
                // ���� �ݶ��̴��� DamageArea ����
                var da = a.GetComponent<DamageArea>();
                if (da)
                {
                    da.damage = p1.explodeDamagePerTick;
                    da.continuous = true;
                    da.tickInterval = p1.explodeTickInterval;
                    da.life = p1.explodeLife;
                    da.targetLayer = config.playerLayer;
                }
                // �ݶ��̴� ������� �����տ��� �ݰ�/���̸� ���ߴ� ���� ����
            });
    }

    IEnumerator P1_SkillProjectile()
    {
        var p1 = config.phase1;
        _anim.SetTrigger(config.animTriggerSkill);

        // �÷��̾� ������ ��ġ�� �������� ������ �߻�
        Vector3 aimPos = player.position;
        yield return null; // ª�� �ҷ� "������ ��ġ" ����
        var go = Instantiate(p1.skillProjectilePrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
        var pr = go.GetComponent<Projectile>();
        pr.speed = p1.skillProjectileSpeed;
        pr.life = p1.skillProjectileLife;
        pr.damage = p1.skillProjectileDamage;
        pr.hitLayers = config.playerLayer;
        pr.destroyOnAnyHit = true; // ���� �� ����Ʈ �Ҹ�
        pr.Fire((aimPos - transform.position).normalized);
    }

    IEnumerator P1_HLaser()
    {
        var p1 = config.phase1;
        _anim.SetTrigger(config.animTriggerSkill);

        // Y ���� 2��
        float t = 0;
        float y = player.position.z;
        while (t < p1.laserTrackSecondsY)
        {
            y = player.position.z;
            t += Time.deltaTime;
            yield return null;
        }

        // ������
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, y);
        yield return PreviewSpawner.SpawnAfter(
            p1.laserPreviewPrefab, p1.laserBeamPrefab, pos, Quaternion.identity, p1.laserDelay,
            a =>
            {
                var ls = a.GetComponent<LaserSweep>();
                if (ls)
                {
                    ls.width = p1.laserWidth;
                    ls.damageOnce = p1.laserDamageOnce;
                    ls.life = p1.laserLife;
                    ls.GetComponent<LaserSweep>().SetSpan(arenaHorizontalSpan, true);
                    var col = a.GetComponent<BoxCollider>();
                    if (col) col.isTrigger = true;
                }
            });
    }

    // ===== Phase 2 =====
    float _p2NextBasic;
    bool _p2Fast;

    void TickPhase2()
    {
        var p2 = config.phase2;

        // �̵�
        if (!IsInvoking(nameof(P2_MoveRoll)))
            Invoke(nameof(P2_MoveRoll), 0.7f);
        float ms = _p2Fast ? p2.fastSpeed : p2.slowSpeed;
        transform.position += DirToPlayerXZ() * ms * Time.deltaTime;
        SetMoveAnim(ms);

        // �⺻ ����: �ָ� �� ���� �ĵ�
        if (Time.time >= _p2NextBasic)
        {
            _anim.SetTrigger(config.animTriggerBasic);
            StartCoroutine(P2_FistWave(p2));
            _p2NextBasic = Time.time + p2.basicCooldown;
        }

        // ��ų
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
        yield return new WaitForSeconds(0.1f); // ��� ���� ����
        // ���� �� ��Ʈ�ڽ� Ȯ��
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
        _anim.SetTrigger(config.animTriggerSkill);
        yield return new WaitForSeconds(0.15f); // ������� Ÿ�̹�

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
        _anim.SetTrigger(config.animTriggerSkill);
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
        _anim.SetTrigger(config.animTriggerSkill);

        // ������
        var preview = Instantiate(p2.dashPreviewPrefab, transform.position, Quaternion.identity);
        var dirSign = Mathf.Sign(player.position.x - transform.position.x);
        var dashDir = new Vector3(dirSign, 0, 0); // x�ุ
        Destroy(preview, p2.dashWindup);

        yield return new WaitForSeconds(p2.dashWindup);

        // ����
        var trail = p2.dashTrailPrefab ? Instantiate(p2.dashTrailPrefab, transform) : null;
        float t = 0;
        Vector3 start = transform.position;
        Vector3 end = start + dashDir * p2.dashDistance;

        while (t < p2.dashTime)
        {
            float a = t / p2.dashTime;
            transform.position = Vector3.Lerp(start, end, a);
            // ��Ʈ�ڽ�
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

        // �⺻ ����: ���� ������
        if (!IsInvoking(nameof(P3_BasicVLaser)))
        {
            float delay = Random.Range(1.2f, 1.8f);
            Invoke(nameof(P3_BasicVLaser), delay);
        }

        // ��ų
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

        // X ����
        float t = 0; float x = player.position.x;
        while (t < p3.trackSecondsX)
        {
            x = player.position.x;
            t += Time.deltaTime;
            yield return null;
        }

        // ������ �� ����
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

        // �� ���� ������ Ʈ��ŷ 2��(���� ������)
        float t = 0;
        while (t < p3.eyeBeamTrackSeconds)
        {
            // �������� ���� �� ���ؿ��� �÷��̾� ������ ���� ��Ʈ ��ĵ ó���ϵ��� ���� ����
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

            // �� ��
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
        // ������ �� �� ���� ����Ʈ
        var pos = transform.position; // �ʿ� �� ��ġ ����
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

    // ===== �ǰ� ó�� =====
    public void ApplyDamage(float amount)
    {
        if (_shielding) return; // ��ȣ�� �� ����
        // ü�� �ý��� ���� ����
        // Debug.Log($"Boss took {amount}");
    }
}

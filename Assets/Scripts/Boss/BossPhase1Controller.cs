// BossPhase1Controller.cs
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossPhase1Controller : MonoBehaviour
{
    [Header("Refs")]
    public BossHealth health;
    public Animator anim;
    public Transform modelRoot;
    public Transform player; // PlayerController.Instance.transform �Ҵ� ��õ

    [Header("Movement")]
    public float followSpeed = 3f;
    [Range(0f, 1f)] public float moveFollowProbability = 0.6f; // ���� Ȯ��
    public float decisionInterval = 1.2f;

    [Header("Basic Attack")]
    public GameObject basicProjectilePrefab;
    public Transform shootPoint;
   // public float basicCooldown = 2.0f;

    [Header("Skill A: ����")]
    public CylindricalExplosion explosionPrefab; // �����տ� CylindricalExplosion ����
    public float trackXTime_A = 2f;
    public float waitAfterPreview_A = 0.5f;   // ������ ����� �� ��� �ð�

    [Header("Skill B: ����ü")]
    public GameObject skillBProjectilePrefab; // �ٸ� VFX    

    [Header("Skill C: ������ ����")]
    public ScreenLaser laserHPrefab;          // axis=Horizontal, continuous=false
    //public float trackYTime_C = 2f;           // Y�� ���� �ð�
    //public float waitAfterPreview_C = 0.5f;   // ������ ����� �� ��� �ð�
    public float aimYOffset = 1.0f;           // ���� ���� ������
    [Header("Laser Settings")]
    public float laserWorldWidth = 60f;

    private ScreenLaser currentLaser;         // ������ ������ �ν��Ͻ� ����

    [Header("Probabilities")]
    [Range(0f, 1f)] public float pBasic = 0.45f;
    [Range(0f, 1f)] public float pSkillA = 0.30f;
    [Range(0f, 1f)] public float pSkillB = 0.20f;
    [Range(0f, 1f)] public float pSkillC = 0.05f; // ���� ����

    [Header("Phase2")]
    public GameObject phase2Model;     // ��Ȱ���� ��ġ
    public BossPhase2Controller phase2Controller; // ������2 ��ũ��Ʈ ����

    bool canAct = true;   // �̵� ���� ����
    bool isBusy = false;  // ���� �� ����
    bool movingToPlayer;
    float lastBasicTime = -999f;
    Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!player) player = PlayerController.Instance?.transform;
        health.OnBossDeath += HandlePhaseChange;
        StartCoroutine(AILoop());
        StartCoroutine(MoveDecider());
    }

    [Header("AI Decision")]
    public float attackDecisionInterval = 1.5f;  // ���� �Ǵ� �ֱ�
    [Range(0f, 1f)] public float attackProbability = 0.3f; // �̹� �Ͽ� ���� �õ��� Ȯ��

    IEnumerator AILoop()
    {
        while (enabled)
        {
            if (!canAct || isBusy || player == null)
            {
                yield return null;
                continue;
            }

            // 1 ���� �ֱ⸶�� �Ǵ�
            yield return new WaitForSeconds(attackDecisionInterval);

            // 2 �̹� �Ͽ� �������� ����
            if (Random.value > attackProbability)
                continue; // �������� �ʰ� Idle ����

            // 3 �����ϱ�� �ߴٸ� ��ų ���� ����
            float total = pBasic + pSkillA + pSkillB + pSkillC;
            if (total <= 0.001f)
                continue;

            float r = Random.value * total;

            //if (Time.time - lastBasicTime >= basicCooldown && r < pBasic)
            //yield return StartCoroutine(BasicAttack());

            if (r < pBasic)
                yield return StartCoroutine(BasicAttack());
            else if (r < pBasic + pSkillA)
                yield return StartCoroutine(SkillA_Explosion());
            else if (r < pBasic + pSkillA + pSkillB)
                yield return StartCoroutine(SkillB_Projectile());
            else
                yield return StartCoroutine(SkillC_LaserH());
        }
    }





    IEnumerator MoveDecider()
    {
        while (enabled)
        {
            movingToPlayer = Random.value < moveFollowProbability;
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    void FixedUpdate()
    {
        float speedValue = 0f;

        // �ൿ ������ ���ȸ� �̵�
        if (canAct && player)
        {
            var pos = rb.position;
            float dir = Mathf.Sign(player.position.x - pos.x);
            float dist = Mathf.Abs(player.position.x - pos.x);

            // Ȯ�������θ� �̵�
            if (movingToPlayer && dist > 0.05f)
            {
                pos.x += dir * followSpeed * Time.fixedDeltaTime;
                rb.MovePosition(pos);
                speedValue = followSpeed;
            }

            Face(dir);
        }
        else if (player)
        {
            // ��ų ���� ���� ���⸸ ����, �̵��� ����
            float dir = Mathf.Sign(player.position.x - rb.position.x);
            Face(dir);
            speedValue = 0f;
        }

        if (anim) anim.SetFloat("MoveSpeed", speedValue);
    }


    void Face(float dir)
    {
        if (!modelRoot) return;

        // ���� +Z�� �ٶ󺸰� �ִٸ� �̷���
        modelRoot.localRotation = Quaternion.Euler(0f, dir >= 0 ? 90f : -90f, 0f);
    }





    IEnumerator BasicAttack()
    {
        isBusy = true;
        canAct = false;

        if (anim) anim.SetTrigger("Basic");
        yield return new WaitForSeconds(0.2f); // Ÿ�̹�
        //FireProjectile();                      // Animator Event ��� �� ���� ����
        lastBasicTime = Time.time;

        yield return new WaitForSeconds(0.3f);
        
        isBusy = false;
    }
    void FireProjectile()
    {
        if (!player || !basicProjectilePrefab || !shootPoint) return;

        var proj = Instantiate(basicProjectilePrefab, shootPoint.position, Quaternion.identity)
                    .GetComponent<Projectile>();
        Vector3 dir = (player.position - shootPoint.position).normalized;
        dir.y = 0f;
        proj.Launch(dir);
    }

    void EndBasic()
    {
        canAct = true;
    }

    IEnumerator SkillA_Explosion()
    {
        isBusy = true;
        canAct = false;

        // 1. ������ ����
        Vector3 spawnPos = transform.position;
        var inst = Instantiate(explosionPrefab, spawnPos, Quaternion.identity);
        inst.ConfigureShape();
        if (inst.preview) inst.preview.gameObject.SetActive(true);

        // 2. �÷��̾� X ����
        float t = 0f;
        while (t < trackXTime_A)
        {
            if (player)
            {
                Vector3 p = inst.transform.position;
                p.x = player.position.x;
                inst.transform.position = p;
            }
            t += Time.deltaTime;
            yield return null;
        }

        // 3. ������ ���� �� ���
        if (inst.preview) inst.preview.gameObject.SetActive(false);
        yield return new WaitForSeconds(waitAfterPreview_A);

        // 4. ���� �� �ִϸ��̼� ���
        if (anim) anim.SetTrigger("SkillA");
        if (inst.explosion) inst.explosion.SetActive(true);

        // 5. �ִϸ��̼� �̺�Ʈ�� EndSkillA ȣ���� ������ ���
        while (!canAct)
            yield return null;

        Destroy(inst.gameObject, inst.autoDestroyAfter);
        isBusy = false;
    }
    public void EndSkillA()
    {
        canAct = true;
    }


    IEnumerator SkillB_Projectile()
    {
        //1. �̵� ����
        isBusy = true;
        canAct = false;

        // 2. �÷��̾� ������ ��ġ�� �ٶ󺸰� ���� �߻�
        Vector3 last = player ? player.position : shootPoint.position + transform.right;
        yield return new WaitForSeconds(0.25f);

        // 3. �ִϸ��̼�
        if (anim) anim.SetTrigger("SkillB");

        while (!canAct)
            yield return null;

        isBusy = false;

    }

    public void FireSkillBProjectile()
    {
        if (!player || !skillBProjectilePrefab || !shootPoint) return;

        Vector3 lastPos = player.position;
        var proj = Instantiate(skillBProjectilePrefab, shootPoint.position, Quaternion.identity)
                    .GetComponent<Projectile>();
        Vector3 dir = (lastPos - shootPoint.position).normalized;
        dir.y = 0f;
        proj.Launch(dir);
    }

    public void EndSkillB()
    {
        canAct = true;
    }


    bool allowLaserTracking = false;
    IEnumerator SkillC_LaserH()
    {
        isBusy = true;
        canAct = false;

        // ������ ����
        float y = player ? player.position.y + aimYOffset : transform.position.y;
        currentLaser = Instantiate(laserHPrefab, new Vector3(0, y, 0), Quaternion.identity);
        currentLaser.axis = ScreenLaser.Axis.Horizontal;
        currentLaser.continuous = false;
        currentLaser.SetupToCameraBounds(laserWorldWidth);

        // ���� ����
        allowLaserTracking = true;

        // �ִϸ��̼� ����
        if (anim) anim.SetTrigger("SkillC");

        // Y�� ���� ����
        while (allowLaserTracking)
        {
            if (player && currentLaser)
            {
                Vector3 pos = currentLaser.transform.position;
                pos.y = player.position.y + aimYOffset;
                currentLaser.transform.position = pos;
            }
            yield return null;
        }

        // �ִϸ��̼� �̺�Ʈ EndSkillC ȣ�� ���
        while (!canAct)
            yield return null;

        isBusy = false;
    }


    // ���� ���� (0s, ��� ������)
    public void StartLaserPreview()
    {
        if (currentLaser && currentLaser.preview)
            currentLaser.preview.gameObject.SetActive(true);
        allowLaserTracking = true; // Y�� ���� ����
    }

    // ���� ���� (1.3s, �� �� ���� ��)
    public void HideLaserPreview()
    {
        if (currentLaser && currentLaser.preview)
            currentLaser.preview.gameObject.SetActive(false);
        allowLaserTracking = false; // Y�� ���� ����
    }

    // �߻� (1.4s)
    public void FireLaser()
    {
        if (!currentLaser) return;
        StartCoroutine(currentLaser.FireSequence());
    }

    // ���� (3.15s)
    public void EndSkillC()
    {
        canAct = true;
    }




    void HandlePhaseChange()
    {
        DestroyAllSkillObjects();

        // 1������ ���� ����
        if (anim) anim.SetTrigger("DiePhase");

        // 2������ �� Ȱ��
        if (phase2Model) phase2Model.SetActive(true);
        if (phase2Controller)
        {
            phase2Controller.BeginPhase();
        }

        // 1������ �� ��Ȱ��
        gameObject.SetActive(false);
    }

    void DestroyAllSkillObjects()
{
    // ���� ������ �� ���� ������Ʈ ����
    foreach (var exp in FindObjectsByType<CylindricalExplosion>(FindObjectsSortMode.None))
        Destroy(exp.gameObject);

    // ������ ����
    foreach (var laser in FindObjectsByType<ScreenLaser>(FindObjectsSortMode.None))
        Destroy(laser.gameObject);

    // ��ų B ����ü ����
    foreach (var proj in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
    {
        if (proj.CompareTag("EnemyProjectile"))
            Destroy(proj.gameObject);
    }
}


}

// BossPhase2Controller.cs (2.5D ���� ���� ����)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossPhase2Controller : MonoBehaviour
{
    [Header("Refs")]
    public BossHealth health;
    public Animator anim;
    public Transform player;

    [SerializeField] Transform neckBone;
    public Quaternion targetNeckRotation = Quaternion.Euler(20f, -1.466f, 1.016f);

    [Header("Basic: ���� ��ũ�� ������(���ִϸ��̼�)")]
    public ScreenLaser laserVPrefab;
    public float trackXTime_Basic = 2f;
    public float waitAfterPreview_Basic = 1f;
    public float basicCooldown = 2.5f;

    [Header("Laser Settings")]
    public float laserWorldWidth = 60f;
    private ScreenLaser currentLaser;

    [Header("Skill: Area Pulse (���� ���� ����)")]
    public CylindricalExplosion cylindricalExplosionPrefab;
    public int pulseWaveCount = 3;
    public float pulseInterval = 1.0f;
    public Vector2 mapHalfSizeX = new Vector2(-20f, 20f); // X�� ����
    public Vector2 pulseCountRange = new Vector2(5, 8);
    public float previewTime = 1.0f;

    [Header("Skill: �� ����")]
    public float sweepPreviewTime = 0.5f;
    public float sweepDuration = 0.25f;
    public GameObject sweepPreviewVFX;
    public GameObject sweepHitboxPrefab;

    [Header("Skill: ��ȯ")]
    public List<GameObject> enemyPrefabs;
    public int spawnCount = 4;
    public Vector2 spawnAreaMin = new Vector2(-8, 0);
    public Vector2 spawnAreaMax = new Vector2(8, 0);
    public GameObject spawnMarkerPrefab;
    public float summonPreviewTime = 1.0f;   // ������ ���� �ð�
    public float summonDelay = 0.5f;   // ������ ����� �� ��ȯ���� ���

    [Header("AI Timings")]
    public float skillInterval = 4f;

    [Header("AI Decision")]
    public float attackDecisionInterval = 1.5f;
    [Range(0f, 1f)] public float attackProbability = 0.4f;

    [Header("Attack Probabilities")]
    [Range(0f, 1f)] public float pBasic = 0.5f;
    [Range(0f, 1f)] public float pAreaPulse = 0.3f;
    [Range(0f, 1f)] public float pGroundSweep = 0.15f;
    [Range(0f, 1f)] public float pSummon = 0.05f;

    bool canAct = true;
    bool isBusy = false;
    bool running;
    bool allowLaserTracking = false;

    public void BeginPhase()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        if (!player) player = PlayerController.Instance?.transform;
        running = true;
        StartCoroutine(AILoop());
    }

    void LateUpdate()
    {
        if (neckBone)
            neckBone.localRotation = targetNeckRotation;
    }

    IEnumerator AILoop()
    {
        float total;
        while (running)
        {
            if (isBusy || !player)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(attackDecisionInterval);

            if (Random.value > attackProbability)
                continue;

            total = pBasic + pAreaPulse + pGroundSweep + pSummon;
            if (total <= 0.001f) continue;
            float r = Random.value * total;

            if (r < pBasic)
                yield return StartCoroutine(BasicVerticalLaser());
            else if (r < pBasic + pAreaPulse)
                yield return StartCoroutine(Skill_AreaPulse());
            else if (r < pBasic + pAreaPulse + pGroundSweep)
                yield return StartCoroutine(Skill_GroundSweep());
            else
                yield return StartCoroutine(Skill_SummonAdds());
        }
    }

    IEnumerator BasicVerticalLaser()
    {
        isBusy = true;
        canAct = false;

        float x = player ? player.position.x : transform.position.x;
        currentLaser = Instantiate(laserVPrefab, new Vector3(x, 0, 0), Quaternion.identity);
        currentLaser.axis = ScreenLaser.Axis.Vertical;
        currentLaser.continuous = false;
        currentLaser.SetupToCameraBounds(laserWorldWidth);

        allowLaserTracking = true;

        if (anim) anim.SetTrigger("Basic");

        while (allowLaserTracking)
        {
            if (player && currentLaser)
            {
                Vector3 pos = currentLaser.transform.position;
                pos.x = player.position.x;
                currentLaser.transform.position = pos;
            }
            yield return null;
        }

        while (!canAct)
            yield return null;

        isBusy = false;
    }

    public void StartLaserPreview()
    {
        if (currentLaser && currentLaser.preview)
            currentLaser.preview.gameObject.SetActive(true);
        allowLaserTracking = true;
    }

    public void HideLaserPreview()
    {
        if (currentLaser && currentLaser.preview)
            currentLaser.preview.gameObject.SetActive(false);
        allowLaserTracking = false;
    }

    public void FireLaser()
    {
        if (!currentLaser) return;
        StartCoroutine(currentLaser.FireSequence());
    }

    public void EndBasic()
    {
        canAct = true;
    }

    // 2.5D ���� X�� ���� ����
    IEnumerator Skill_AreaPulse()
    {
        isBusy = true;
        canAct = false;
        if (anim) anim.SetTrigger("AreaPulse");

        for (int wave = 0; wave < pulseWaveCount; wave++)
        {
            int count = Random.Range((int)pulseCountRange.x, (int)pulseCountRange.y + 1);

            for (int i = 0; i < count; i++)
            {
                float xPos = Random.Range(mapHalfSizeX.x, mapHalfSizeX.y);
                Vector3 pos = new Vector3(xPos, 0f, 0f);

                var fx = Instantiate(cylindricalExplosionPrefab, pos, Quaternion.identity);
                fx.ConfigureShape();
                StartCoroutine(fx.ShowThenExplode(previewTime));
            }

            yield return new WaitForSeconds(pulseInterval);
        }

        yield return new WaitForSeconds(1.0f);
        canAct = true;
        isBusy = false;
    }

    IEnumerator Skill_GroundSweep()
    {
        isBusy = true;
        canAct = false;

        // 1. ������ ����
        GameObject pv = null;
        if (sweepPreviewVFX)
        {
            pv = Instantiate(sweepPreviewVFX, Vector3.zero, Quaternion.identity);
            pv.transform.localScale = new Vector3(80f, 2f, 1f); // �� �� ����
        }

        // 2. ������ ����
        yield return new WaitForSeconds(sweepPreviewTime);

        // 3. ������ ����
        if (pv)
        {
            Destroy(pv); // ��� ����
            pv = null;
        }

        // 4. �ִϸ��̼� ���
        if (anim) anim.SetTrigger("GroundSweep");

        // �ִϸ��̼� �̺�Ʈ GroundSweep_Hit()���� Ÿ�� ����

        yield return new WaitForSeconds(sweepDuration + 0.5f);

        canAct = true;
        isBusy = false;
    }
    // �ִϸ��̼� �̺�Ʈ��
    public void GroundSweep_Hit()
    {
        StartCoroutine(SpawnSweepHitbox());
    }

    IEnumerator SpawnSweepHitbox()
    {
        // ��Ʈ�ڽ� ���� (�� ��ü ��)
        var hit = Instantiate(sweepHitboxPrefab, Vector3.zero, Quaternion.identity);

        // ���� ���� ����
        var col = hit.GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(80f, 2f, 1f);     // X=�� ��, Y=���� ����
        col.center = new Vector3(0, 1f, 0);    // Y=�߽� ���� (������ ȸ�� ����)

        // ���� �ð� ���� �� �ı�
        yield return new WaitForSeconds(sweepDuration);
        Destroy(hit);
    }


    IEnumerator Skill_SummonAdds()
    {
        isBusy = true;
        canAct = false;

        // 1. ������ ����
        var markers = new List<GameObject>();
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                spawnAreaMin.y,
                0f
            );

            var mk = Instantiate(spawnMarkerPrefab, pos, Quaternion.identity);
            markers.Add(mk);
        }

        // 2. ������ ����
        yield return new WaitForSeconds(summonPreviewTime);

        // 3. ������ ����
        foreach (var mk in markers)
            Destroy(mk);

        // 4. �ִϸ��̼� ���
        if (anim) anim.SetTrigger("Summon");
        yield return new WaitForSeconds(summonDelay);

        // 5. ���� �� ��ȯ
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                spawnAreaMin.y,
                0f
            );
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(prefab, pos, Quaternion.identity);
        }

        canAct = true;
        isBusy = false;
    }
}

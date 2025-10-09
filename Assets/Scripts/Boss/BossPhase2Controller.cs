// BossPhase2Controller.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossPhase2Controller : MonoBehaviour
{
    [Header("Refs")]
    public BossHealth health;         // ������2 ���� HP(����)
    public Transform player;
    public Camera mainCam;

    [Header("Basic: ���� ��ũ�� ������(���ִϸ��̼�)")]
    public ScreenLaser laserVPrefab;  // axis=Vertical, continuous=false
    public float trackXTime_Basic = 2f;
    public float waitAfterPreview_Basic = 1f;
    public float basicCooldown = 2.5f;

    [Header("Skill: ������ �� 2��(����)")]
    public ScreenLaser eyeLaserPrefab; // continuous=true�� ���
    public Transform leftEye;
    public Transform rightEye;
    public float eyeTrackTime = 2f;

    [Header("Skill: �� ����")]
    public Transform sweepHand;                   // �� ������Ʈ
    public Transform sweepStart;
    public Transform sweepEnd;
    public float sweepPreviewTime = 0.5f;
    public float sweepDuration = 0.25f;           // ������
    public GameObject sweepPreviewVFX;
    public GameObject sweepHitboxPrefab;          // BoxCollider + DamageOnTrigger(continuous=false)

    [Header("Skill: �� ��ȯ")]
    public List<GameObject> enemyPrefabs;
    public int spawnCount = 4;
    public Vector2 spawnAreaMin = new Vector2(-8, 0);
    public Vector2 spawnAreaMax = new Vector2(8, 0);
    public GameObject spawnMarkerPrefab;          // 1�� ǥ��

    [Header("AI Timings")]
    public float skillInterval = 4f;

    bool running;

    public void BeginPhase()
    {
        if (!player) player = PlayerController.Instance?.transform;
        running = true;
        StartCoroutine(AILoop());
    }

    IEnumerator AILoop()
    {
        float lastBasic = -999f;
        while (running)
        {
            if (Time.time - lastBasic >= basicCooldown)
            {
                lastBasic = Time.time;
                yield return StartCoroutine(BasicVerticalLaser());
            }

            // ��ų �����̼�: ���� �� ���� �� ��ȯ
            yield return StartCoroutine(Skill_EyeBeams());
            yield return new WaitForSeconds(skillInterval);

            yield return StartCoroutine(Skill_GroundSweep());
            yield return new WaitForSeconds(skillInterval);

            yield return StartCoroutine(Skill_SummonAdds());
            yield return new WaitForSeconds(skillInterval);
        }
    }

    IEnumerator BasicVerticalLaser()
    {
        // X 2�� ���� �� 1�� �� ���� ������ �ܹ�
        float t = 0f;
        float targetX = transform.position.x;
        while (t < trackXTime_Basic)
        {
            if (player) targetX = player.position.x;
            t += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(waitAfterPreview_Basic);

        var laser = Instantiate(laserVPrefab, new Vector3(targetX, 0, 0), Quaternion.identity);
        laser.axis = ScreenLaser.Axis.Vertical;
        laser.continuous = false;
        laser.SetupToCameraBounds();
        yield return laser.FireSequence();
    }

    IEnumerator Skill_EyeBeams()
    {
        // �� �������� 2�ʰ� �÷��̾ ����. ���� ������.
        ScreenLaser l = Instantiate(eyeLaserPrefab, leftEye.position, Quaternion.identity);
        ScreenLaser r = Instantiate(eyeLaserPrefab, rightEye.position, Quaternion.identity);
        l.axis = ScreenLaser.Axis.Vertical; l.continuous = true;
        r.axis = ScreenLaser.Axis.Vertical; r.continuous = true;
        l.SetupToCameraBounds();
        r.SetupToCameraBounds();

        // ��������� ��� �߻� ���·� ����� 2�� ���� Y/X�� �÷��̾ ���� �̵�
        StartCoroutine(l.FireSequence());
        StartCoroutine(r.FireSequence());

        float t = 0f;
        while (t < eyeTrackTime)
        {
            if (player)
            {
                l.transform.position = new Vector3(player.position.x - 1.2f, l.transform.position.y, l.transform.position.z);
                r.transform.position = new Vector3(player.position.x + 1.2f, r.transform.position.y, r.transform.position.z);
            }
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Skill_GroundSweep()
    {
        // ������ ǥ��
        var pv = Instantiate(sweepPreviewVFX, Vector3.zero, Quaternion.identity);
        pv.transform.position = new Vector3(sweepStart.position.x, sweepStart.position.y, sweepStart.position.z);
        Destroy(pv, sweepPreviewTime + sweepDuration + 0.5f);
        yield return new WaitForSeconds(sweepPreviewTime);

        // ��Ʈ�ڽ� 1ȸ ����
        var hit = Instantiate(sweepHitboxPrefab, sweepStart.position, Quaternion.identity);
        var box = hit.GetComponent<BoxCollider>();
        var dmg = hit.GetComponent<DamageOnTrigger>(); // continuous=false, �ܹ�
        dmg.destroyOnHit = false; // ���� ��� 1ȸ��

        float t = 0f;
        while (t < sweepDuration)
        {
            hit.transform.position = Vector3.Lerp(sweepStart.position, sweepEnd.position, t / sweepDuration);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(hit);
    }

    IEnumerator Skill_SummonAdds()
    {
        // ��Ŀ 1�� �� �� ��ȯ
        var markers = new List<GameObject>();
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                                      spawnAreaMin.y, 0f);
            var mk = Instantiate(spawnMarkerPrefab, pos, Quaternion.identity);
            markers.Add(mk);
        }
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < markers.Count; i++)
        {
            var mk = markers[i];
            Vector3 pos = mk.transform.position;
            Destroy(mk);
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(prefab, pos, Quaternion.identity);
        }
    }
}

// BossPhase2Controller.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossPhase2Controller : MonoBehaviour
{
    [Header("Refs")]
    public BossHealth health;         // 페이즈2 전용 HP(별도)
    public Transform player;
    public Camera mainCam;

    [Header("Basic: 수직 스크린 레이저(무애니메이션)")]
    public ScreenLaser laserVPrefab;  // axis=Vertical, continuous=false
    public float trackXTime_Basic = 2f;
    public float waitAfterPreview_Basic = 1f;
    public float basicCooldown = 2.5f;

    [Header("Skill: 눈에서 빔 2개(지속)")]
    public ScreenLaser eyeLaserPrefab; // continuous=true로 사용
    public Transform leftEye;
    public Transform rightEye;
    public float eyeTrackTime = 2f;

    [Header("Skill: 땅 쓸기")]
    public Transform sweepHand;                   // 손 오브젝트
    public Transform sweepStart;
    public Transform sweepEnd;
    public float sweepPreviewTime = 0.5f;
    public float sweepDuration = 0.25f;           // 빠르게
    public GameObject sweepPreviewVFX;
    public GameObject sweepHitboxPrefab;          // BoxCollider + DamageOnTrigger(continuous=false)

    [Header("Skill: 적 소환")]
    public List<GameObject> enemyPrefabs;
    public int spawnCount = 4;
    public Vector2 spawnAreaMin = new Vector2(-8, 0);
    public Vector2 spawnAreaMax = new Vector2(8, 0);
    public GameObject spawnMarkerPrefab;          // 1초 표시

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

            // 스킬 로테이션: 눈빔 → 쓸기 → 소환
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
        // X 2초 추적 → 1초 뒤 수직 레이저 단발
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
        // 두 레이저가 2초간 플레이어를 추적. 지속 데미지.
        ScreenLaser l = Instantiate(eyeLaserPrefab, leftEye.position, Quaternion.identity);
        ScreenLaser r = Instantiate(eyeLaserPrefab, rightEye.position, Quaternion.identity);
        l.axis = ScreenLaser.Axis.Vertical; l.continuous = true;
        r.axis = ScreenLaser.Axis.Vertical; r.continuous = true;
        l.SetupToCameraBounds();
        r.SetupToCameraBounds();

        // 프리뷰없이 즉시 발사 상태로 만들고 2초 동안 Y/X를 플레이어에 맞춰 이동
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
        // 프리뷰 표시
        var pv = Instantiate(sweepPreviewVFX, Vector3.zero, Quaternion.identity);
        pv.transform.position = new Vector3(sweepStart.position.x, sweepStart.position.y, sweepStart.position.z);
        Destroy(pv, sweepPreviewTime + sweepDuration + 0.5f);
        yield return new WaitForSeconds(sweepPreviewTime);

        // 히트박스 1회 스윕
        var hit = Instantiate(sweepHitboxPrefab, sweepStart.position, Quaternion.identity);
        var box = hit.GetComponent<BoxCollider>();
        var dmg = hit.GetComponent<DamageOnTrigger>(); // continuous=false, 단발
        dmg.destroyOnHit = false; // 여러 대상에 1회씩

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
        // 마커 1초 → 적 소환
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

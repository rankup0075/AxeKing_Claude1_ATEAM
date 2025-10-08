using UnityEngine;
using System;

[CreateAssetMenu(fileName = "BossConfig", menuName = "Boss/BossConfig")]
public class BossConfig : ScriptableObject
{
    [Header("공통")]
    public LayerMask playerLayer;
    public string animParamMoveSpeed = "MoveSpeed";
    public string animTriggerBasic = "BasicAttack";
    public string animTriggerSkillA = "SkillA";
    public string animTriggerSkillB = "SkillB";
    public string animTriggerSkillC = "SkillC";
    public float contactDamageInterval = 0.25f;

    [Header("기본 투사체 공통")]
    public GameObject basicProjectilePrefab;
    public float basicProjectileSpeed = 12f;
    public float basicProjectileLife = 6f;
    public float basicAttackCooldown = 2f;

    [Serializable]
    public class MoveAnim
    {
        public string idleBool = "Idle";
        public string walkBool = "Walk";
        public string runBool = "Run";
    }
    public MoveAnim moveAnim;

    // ========== 페이즈 1 ==========
    [Serializable]
    public class Phase1
    {
        [Header("이동")]
        [Range(0, 1)] public float followChance = 0.65f; // 따라감 확률
        public float moveSpeed = 3f;
        public float idleTimeMin = 0.5f;
        public float idleTimeMax = 1.2f;

        [Header("기본 공격(투사체) 확률")]
        [Range(0, 1)] public float basicAttackChance = 0.6f;

        [Header("스킬 확률")]
        [Range(0, 1)] public float explodeChance = 0.25f;
        [Range(0, 1)] public float projChance = 0.5f;
        [Range(0, 1)] public float laserChance = 0.25f; // 가장 낮게 두면 됨

        [Header("공격 주기(기본+스킬 통합)")]
        public Vector2 attackInterval = new Vector2(2f, 3f);

        //[Header("쿨타임")]
        //public Vector2 skillInterval = new Vector2(2.0f, 3.0f);

        [Header("A. 폭발(원통)")]
        public GameObject explodePreviewPrefab; // 범위 미리보기
        public GameObject explodeEffectPrefab;  // 실제 데미지 오브젝트
        public float explodeTrackSeconds = 2f;  // X만 추적
        public float explodeDelay = 1f;         // 프리뷰 → 실제
        public float explodeRadius = 1.2f;
        public float explodeHeight = 10f;       // 원통 높이
        public float explodeDamagePerTick = 10f;
        public float explodeTickInterval = 0.15f;
        public float explodeLife = 2.0f;

        [Header("B. 투사체(스킬 전용 이펙트)")]
        public GameObject skillProjectilePrefab;
        public float skillProjectileSpeed = 16f;
        public float skillProjectileLife = 6f;
        public float skillProjectileDamage = 18f;

        [Header("C. 레이저(수평, 화면 전체)")]
        public GameObject laserPreviewPrefab; // 수평 바
        public GameObject laserBeamPrefab;    // 같은 범위로 발사
        public float laserTrackSecondsY = 2f;
        public float laserDelay = 1f; // 프리뷰 → 실제
        public float laserWidth = 1.0f;
        public float laserDamageOnce = 30f;
        public float laserLife = 0.6f;

        [Header("Laser Offsets")]
        public float laserYOffset = 1.0f;
        [Header("C. 레이저 애니메이션")]
        public float laserAnimDuration = 3.15f; // 애니메이션 길이에 맞게 (초 단위)
    }
    public Phase1 phase1;

    // ========== 페이즈 2 ==========
    [Serializable]
    public class Phase2
    {
        [Header("이동")]
        public float slowSpeed = 3.2f;
        public float fastSpeed = 6.5f;
        [Range(0, 1)] public float fastApproachChance = 0.35f;
        public string slowAnimBool = "Walk";
        public string fastAnimBool = "Run";

        [Header("기본 공격(주먹→파동)")]
        public GameObject fistWavePrefab; // 투사체 아님. 전방 히트박스 확장형
        public float fistRange = 2.0f;
        public float fistWaveLength = 3.5f;
        public float fistWaveDuration = 0.25f;
        public float fistDamage = 14f;
        public float basicCooldown = 1.8f;

        [Header("스킬 쿨")]
        public Vector2 skillInterval = new Vector2(2.0f, 3.0f);

        [Header("A. 땅 내려찍기")]
        public GameObject slamEffectPrefab; // 양옆 연쇄 폭발 이펙트
        public float slamSpan = 7f; // 좌우로 터지는 총 길이
        public int slamCount = 6;   // 터짐 개수
        public float slamStepDelay = 0.08f;
        public float slamDamage = 16f;

        [Header("B. 보호막")]
        public GameObject shieldPrefab;
        public float shieldDuration = 2f;
        public float shieldTouchDamage = 10f;

        [Header("C. 순간 대쉬")]
        public GameObject dashPreviewPrefab;
        public GameObject dashTrailPrefab;
        public float dashWindup = 0.25f;
        public float dashDistance = 8f;
        public float dashTime = 0.12f; // 매우 빠름
        public float dashDamage = 30f;
        public float dashHitboxRadius = 0.8f;
    }
    public Phase2 phase2;

    // ========== 페이즈 3 ==========
    [Serializable]
    public class Phase3
    {
        [Header("이동 없음")]

        [Header("기본 공격(수직 레이저)")]
        public GameObject vLaserPreviewPrefab;
        public GameObject vLaserBeamPrefab;
        public float trackSecondsX = 2f;
        public float fireDelay = 1f;
        public float beamWidth = 1.0f;
        public float beamDamageOnce = 24f;
        public float beamLife = 0.6f;



        [Header("스킬 쿨")]
        public Vector2 skillInterval = new Vector2(1.8f, 2.6f);

        [Header("눈에서 빔 2개(지속접촉형)")]
        public GameObject eyeBeamPrefab; // 두 갈래. 트래킹 2초
        public float eyeBeamTrackSeconds = 2f;
        public float eyeBeamTickDamage = 6f;
        public float eyeBeamTickInterval = 0.1f;
        public float eyeBeamLife = 2.2f;

        [Header("땅 쓸기")]
        public GameObject sweepPreviewPrefab;
        public GameObject sweepEffectPrefab;
        public float sweepDelay = 0.5f;
        public float sweepDamageOnce = 22f;
        public float sweepLife = 0.25f;

        [Header("적 소환")]
        public GameObject summonMarkerPrefab;
        public GameObject[] enemyPrefabs;
        public Vector2Int countRange = new Vector2Int(3, 6);
        public float markerTime = 1f;
        public Vector2 spawnAreaHalfExtents = new Vector2(7, 4);
    }
    public Phase3 phase3;
}

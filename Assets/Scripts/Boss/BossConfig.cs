using UnityEngine;
using System;

[CreateAssetMenu(fileName = "BossConfig", menuName = "Boss/BossConfig")]
public class BossConfig : ScriptableObject
{
    [Header("����")]
    public LayerMask playerLayer;
    public string animParamMoveSpeed = "MoveSpeed";
    public string animTriggerBasic = "BasicAttack";
    public string animTriggerSkillA = "SkillA";
    public string animTriggerSkillB = "SkillB";
    public string animTriggerSkillC = "SkillC";
    public float contactDamageInterval = 0.25f;

    [Header("�⺻ ����ü ����")]
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

    // ========== ������ 1 ==========
    [Serializable]
    public class Phase1
    {
        [Header("�̵�")]
        [Range(0, 1)] public float followChance = 0.65f; // ���� Ȯ��
        public float moveSpeed = 3f;
        public float idleTimeMin = 0.5f;
        public float idleTimeMax = 1.2f;

        [Header("�⺻ ����(����ü) Ȯ��")]
        [Range(0, 1)] public float basicAttackChance = 0.6f;

        [Header("��ų Ȯ��")]
        [Range(0, 1)] public float explodeChance = 0.25f;
        [Range(0, 1)] public float projChance = 0.5f;
        [Range(0, 1)] public float laserChance = 0.25f; // ���� ���� �θ� ��

        [Header("���� �ֱ�(�⺻+��ų ����)")]
        public Vector2 attackInterval = new Vector2(2f, 3f);

        //[Header("��Ÿ��")]
        //public Vector2 skillInterval = new Vector2(2.0f, 3.0f);

        [Header("A. ����(����)")]
        public GameObject explodePreviewPrefab; // ���� �̸�����
        public GameObject explodeEffectPrefab;  // ���� ������ ������Ʈ
        public float explodeTrackSeconds = 2f;  // X�� ����
        public float explodeDelay = 1f;         // ������ �� ����
        public float explodeRadius = 1.2f;
        public float explodeHeight = 10f;       // ���� ����
        public float explodeDamagePerTick = 10f;
        public float explodeTickInterval = 0.15f;
        public float explodeLife = 2.0f;

        [Header("B. ����ü(��ų ���� ����Ʈ)")]
        public GameObject skillProjectilePrefab;
        public float skillProjectileSpeed = 16f;
        public float skillProjectileLife = 6f;
        public float skillProjectileDamage = 18f;

        [Header("C. ������(����, ȭ�� ��ü)")]
        public GameObject laserPreviewPrefab; // ���� ��
        public GameObject laserBeamPrefab;    // ���� ������ �߻�
        public float laserTrackSecondsY = 2f;
        public float laserDelay = 1f; // ������ �� ����
        public float laserWidth = 1.0f;
        public float laserDamageOnce = 30f;
        public float laserLife = 0.6f;

        [Header("Laser Offsets")]
        public float laserYOffset = 1.0f;
        [Header("C. ������ �ִϸ��̼�")]
        public float laserAnimDuration = 3.15f; // �ִϸ��̼� ���̿� �°� (�� ����)
    }
    public Phase1 phase1;

    // ========== ������ 2 ==========
    [Serializable]
    public class Phase2
    {
        [Header("�̵�")]
        public float slowSpeed = 3.2f;
        public float fastSpeed = 6.5f;
        [Range(0, 1)] public float fastApproachChance = 0.35f;
        public string slowAnimBool = "Walk";
        public string fastAnimBool = "Run";

        [Header("�⺻ ����(�ָԡ��ĵ�)")]
        public GameObject fistWavePrefab; // ����ü �ƴ�. ���� ��Ʈ�ڽ� Ȯ����
        public float fistRange = 2.0f;
        public float fistWaveLength = 3.5f;
        public float fistWaveDuration = 0.25f;
        public float fistDamage = 14f;
        public float basicCooldown = 1.8f;

        [Header("��ų ��")]
        public Vector2 skillInterval = new Vector2(2.0f, 3.0f);

        [Header("A. �� �������")]
        public GameObject slamEffectPrefab; // �翷 ���� ���� ����Ʈ
        public float slamSpan = 7f; // �¿�� ������ �� ����
        public int slamCount = 6;   // ���� ����
        public float slamStepDelay = 0.08f;
        public float slamDamage = 16f;

        [Header("B. ��ȣ��")]
        public GameObject shieldPrefab;
        public float shieldDuration = 2f;
        public float shieldTouchDamage = 10f;

        [Header("C. ���� �뽬")]
        public GameObject dashPreviewPrefab;
        public GameObject dashTrailPrefab;
        public float dashWindup = 0.25f;
        public float dashDistance = 8f;
        public float dashTime = 0.12f; // �ſ� ����
        public float dashDamage = 30f;
        public float dashHitboxRadius = 0.8f;
    }
    public Phase2 phase2;

    // ========== ������ 3 ==========
    [Serializable]
    public class Phase3
    {
        [Header("�̵� ����")]

        [Header("�⺻ ����(���� ������)")]
        public GameObject vLaserPreviewPrefab;
        public GameObject vLaserBeamPrefab;
        public float trackSecondsX = 2f;
        public float fireDelay = 1f;
        public float beamWidth = 1.0f;
        public float beamDamageOnce = 24f;
        public float beamLife = 0.6f;



        [Header("��ų ��")]
        public Vector2 skillInterval = new Vector2(1.8f, 2.6f);

        [Header("������ �� 2��(����������)")]
        public GameObject eyeBeamPrefab; // �� ����. Ʈ��ŷ 2��
        public float eyeBeamTrackSeconds = 2f;
        public float eyeBeamTickDamage = 6f;
        public float eyeBeamTickInterval = 0.1f;
        public float eyeBeamLife = 2.2f;

        [Header("�� ����")]
        public GameObject sweepPreviewPrefab;
        public GameObject sweepEffectPrefab;
        public float sweepDelay = 0.5f;
        public float sweepDamageOnce = 22f;
        public float sweepLife = 0.25f;

        [Header("�� ��ȯ")]
        public GameObject summonMarkerPrefab;
        public GameObject[] enemyPrefabs;
        public Vector2Int countRange = new Vector2Int(3, 6);
        public float markerTime = 1f;
        public Vector2 spawnAreaHalfExtents = new Vector2(7, 4);
    }
    public Phase3 phase3;
}

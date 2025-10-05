using UnityEngine;

[System.Serializable]
public class BossSkill
{
    public string skillName = "NewSkill";

    public enum SkillType { Projectile, AOE, Laser }
    public SkillType type = SkillType.Projectile;

    [Header("Common")]
    public int damage = 5;
    public float cooldown = 2f;
    public float castDelay = 0.2f; // ��� �� ������
    public bool interruptible = true;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float projectileLife = 5f;

    [Header("AOE")]
    public float aoeRadius = 3f;
    public float aoeDelay = 0.1f; // ���� �� ���� ������
    public enum AoeTargetMode { SpawnPoint, Boss, Player, RandomAroundPlayer, RandomAroundBoss }
    [Header("AOE Target (only for AOE)")]
    public AoeTargetMode aoeTarget = AoeTargetMode.SpawnPoint;
    public float randomRadius = 3f;

    [Header("AOE Visuals")]
    public GameObject previewVFX;    // ��� �� ���� '���' ������. ���� �� ǥ��
    public GameObject impactVFX;     // ���� ���� ����Ʈ

    [Header("AOE Height / Avoidance")]
    public float maxVerticalOffset = 1.5f; // ���� �߽ɺ��� �̸�ŭ ���� ������ ���ظ� ���� ���� (�÷��̾ ������ ���� ����)


    [Header("Laser")]
    public float laserRange = 20f;
    public float laserDuration = 1.0f; // ���� ��
    public float laserTickInterval = 0.2f;

    [Header("Visuals")]
    public Transform spawnPoint; // �������� ���� ��ų�̶�� ������

    [Header("Animation")]
    public string animationTrigger = "";   // Animator�� ���� Trigger �̸�
    public float animationLength = 0.5f;   // �ִϸ��̼ǿ��� ������ ������(�Ǵ� ������ �߻�) �������� �ð�


}

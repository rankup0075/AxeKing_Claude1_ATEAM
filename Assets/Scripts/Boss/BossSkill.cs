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
    public float castDelay = 0.2f; // 사용 전 딜레이
    public bool interruptible = true;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float projectileLife = 5f;

    [Header("AOE")]
    public float aoeRadius = 3f;
    public float aoeDelay = 0.1f; // 생성 후 폭발 딜레이
    public enum AoeTargetMode { SpawnPoint, Boss, Player, RandomAroundPlayer, RandomAroundBoss }
    [Header("AOE Target (only for AOE)")]
    public AoeTargetMode aoeTarget = AoeTargetMode.SpawnPoint;
    public float randomRadius = 3f;

    [Header("AOE Visuals")]
    public GameObject previewVFX;    // 노란 원 같은 '경고' 프리팹. 폭발 전 표시
    public GameObject impactVFX;     // 실제 폭발 이펙트

    [Header("AOE Height / Avoidance")]
    public float maxVerticalOffset = 1.5f; // 폭발 중심보다 이만큼 위에 있으면 피해를 받지 않음 (플레이어가 점프해 피할 공간)


    [Header("Laser")]
    public float laserRange = 20f;
    public float laserDuration = 1.0f; // 지속 딜
    public float laserTickInterval = 0.2f;

    [Header("Visuals")]
    public Transform spawnPoint; // 프리팹이 없는 스킬이라면 참조용

    [Header("Animation")]
    public string animationTrigger = "";   // Animator에 만든 Trigger 이름
    public float animationLength = 0.5f;   // 애니메이션에서 공격이 끝나는(또는 데미지 발생) 시점까지 시간


}

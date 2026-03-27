using UnityEngine;

public enum EnemyAttackType { None, Melee, Ranged }

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Characters/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Names")]
    public string enemyId;
    public string displayName;

    [Header("Visuals")]
    public RuntimeAnimatorController animatorController;
    public Sprite defaultSprite;

    [Header("Type")]
    public ElementType elementType = ElementType.None;

    [Header("Stats")]
    public int maxHealth = 5;
    public float moveSpeed = 2.5f;

    [Header("Movement")]
    public bool canFly = false;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;

    [Header("Detection")]
    public float aggroDistance = 6f;      // start chasing if player is in front + within this
    public float stopChaseDistance = 8f;  // lose aggro if too far (prevents jitter)
    public float faceDotThreshold = 0.25f; // how "in front" the player must be (-1..1)

    [Header("Attack")]
    public EnemyAttackType attackType = EnemyAttackType.Melee;

    public float meleeRange = 1.2f;
    public int meleeDamage = 1;
    public float meleeCooldown = 1.0f;

    public float rangedRange = 5f;
    public int rangedDamage = 1;
    public float rangedCooldown = 1.5f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    [Header("Patrol")]
    public float arriveThreshold = 0.1f; // how close to a patrol point counts as "arrived"
}

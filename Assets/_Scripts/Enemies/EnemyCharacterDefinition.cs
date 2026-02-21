using UnityEngine;

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

    [Header("Combat (optional)")]
    public int contactDamage = 1;
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float attackCooldown = 1.0f;
}

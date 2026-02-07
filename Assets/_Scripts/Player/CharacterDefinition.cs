using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Characters/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    [Header("Identity")]
    public string characterId;
    public string displayName;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Flying Parameters")]
    public bool canFly = false;
    public bool limitFlight = true;
    public float maxFlyTime;      // total flight time before you must land
    public float flyRefillRate;     // fuel per second while grounded (1 = refills in maxFlyTime seconds)
    public float flyJumpForce;     // optional: separate force while flying (or reuse jumpForce)

    [Header("Visuals / Animations")]
    public RuntimeAnimatorController animatorController;
    public Sprite defaultSprite;

    [Header("Types")]
    public ElementType elementType = ElementType.None;

    [Header("Ranged Attack")]
    public GameObject rangedProjectilePrefab;
    public float rangedCooldown = 0.5f;
    public float rangedProjectileSpeed = 10f;
    public int rangedDamage = 1;
    public float rangedHoverTime = 1f;     // <-- follows spawn point during this time
    public float rangedLifetime = 3f;      // optional

    [Header("Melee Attack")]
    public GameObject meleeHitboxPrefab;
    public float meleeCooldown = 0.4f;
    public float meleeDamage = 1;
    public float meleeLifetime = 0.5f;
}
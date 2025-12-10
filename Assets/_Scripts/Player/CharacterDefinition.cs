using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Characters/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    [Header("Identity")]
    public string characterId;            // e.g. "BaseHero", "FlyGirl"
    public string displayName;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public bool canFly = false;
    public float jumpForce = 10f;

    [Header("Visuals / Animations")]
    public RuntimeAnimatorController animatorController;
    public Sprite defaultSprite;          // optional; useful for UI portrait, etc.

    [Header("Ranged Attack")]
    public GameObject rangedProjectilePrefab;
    public float rangedCooldown = 0.5f;
    public float rangedProjectileSpeed = 10f;

    [Header("Melee Attack")]
    public GameObject meleeHitboxPrefab;
    public float meleeCooldown = 0.4f;
}
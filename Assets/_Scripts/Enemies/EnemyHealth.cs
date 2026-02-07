using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public EnemyDefinition definition;

    public int CurrentHealth { get; private set; }

    private void Awake()
    {
        if (definition == null)
        {
            Debug.LogError($"{name}: EnemyDefinition missing!");
            enabled = false;
            return;
        }

        CurrentHealth = definition.maxHealth;
    }

    public void TakeDamage(int amount, ElementType damageType = ElementType.None)
    {
        if (amount <= 0) return;

        float multiplier = GetElementMultiplier(damageType, definition.elementType);

        int finalDamage = Mathf.RoundToInt(amount * multiplier);

        // If immune (0 damage), just ignore
        if (finalDamage <= 0)
            return;

        CurrentHealth -= finalDamage;

        if (CurrentHealth <= 0)
            Die();
    }

    private float GetElementMultiplier(ElementType attackType, ElementType enemyType)
    {
        // Neutral attacks always do normal damage
        if (attackType == ElementType.None || enemyType == ElementType.None)
            return 1f;

        // Your requested rules:
        // Fire projectile: 0 damage to Fire enemy, 2x damage to Ice enemy
        if (attackType == ElementType.Fire && enemyType == ElementType.Fire) return 0f;
        if (attackType == ElementType.Fire && enemyType == ElementType.Ice) return 2f;

        // Default: normal damage for everything else (for now)
        return 1f;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
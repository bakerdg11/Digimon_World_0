using UnityEngine;

public class CharacterUnlockPickup : MonoBehaviour
{
    public CharacterDefinition characterToUnlock;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Your player should have CharacterSwap somewhere (on player or parent)
        var swap = other.GetComponentInParent<CharacterSwap>();
        if (swap == null) return;

        if (characterToUnlock == null)
        {
            Debug.LogWarning($"{name}: No characterToUnlock assigned!");
            return;
        }

        swap.AddCharacterToRoster(characterToUnlock);

        // Destroy the pickup object (not the player)
        Destroy(gameObject);
    }
}
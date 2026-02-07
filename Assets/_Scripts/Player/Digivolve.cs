using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BasePlayerController))]
public class Digivolve : MonoBehaviour
{
    private Animator anim;
    private BasePlayerController player;

    private bool isDigivolving;
    private CharacterDefinition pendingTarget; // what we will transform into when the event fires

    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<BasePlayerController>();
    }

    private void Update()
    {
        if (isDigivolving) return;

        // Digivolve forward (F)
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            var current = player.currentCharacter;
            if (current == null) return;

            // only if this character has a digivolve target
            if (!current.canDigivolve || current.digivolveTo == null)
                return;

            pendingTarget = current.digivolveTo;
            isDigivolving = true;
            anim.SetTrigger("TryDigivolve");
        }

        // Optional: Dedigivolve/backward (G)
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            var current = player.currentCharacter;
            if (current == null) return;

            if (!current.canDigivolve || current.deDigivolveTo == null)
                return;

            pendingTarget = current.deDigivolveTo;
            isDigivolving = true;
            anim.SetTrigger("TryDigivolve"); // reuse same animation, or use a different trigger if you want
        }
    }

    // Called by animation event at END of Digivolve animation
    public void DigivolveToNextLevel()
    {
        if (!isDigivolving)
            return;

        if (pendingTarget != null)
            player.ApplyCharacterDefinition(pendingTarget);

        pendingTarget = null;
        isDigivolving = false;
    }
}

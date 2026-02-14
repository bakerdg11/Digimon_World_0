using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BasePlayerController))]
public class Digivolve : MonoBehaviour
{
    private Animator anim;
    private BasePlayerController player;
    private PlayerStats stats;


    private bool isDigivolving;
    private CharacterDefinition pendingTarget;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<BasePlayerController>();
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (isDigivolving) return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            var current = player.currentCharacter;
            if (current == null) return;

            // must have a target
            if (!current.canDigivolve || current.digivolveOptions == null || current.digivolveOptions.Count == 0)
                return;

            // must be full energy
            if (stats == null || !stats.IsEnergyFull)
                return;

            pendingTarget = current.digivolveOptions[0];
            isDigivolving = true;
            anim.SetTrigger("TryDigivolve");
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            var current = player.currentCharacter;
            if (current == null) return;

            if (current.dedigivolveTo == null)
                return;

            pendingTarget = current.dedigivolveTo;
            isDigivolving = true;
            anim.SetTrigger("TryDigivolve");
        }
    }

    // Animation Event
    public void DigivolveToNextLevel()
    {
        if (!isDigivolving) return;

        if (pendingTarget != null)
            player.ApplyCharacterDefinition(pendingTarget);

        // spend energy (optional: set to 0 after digivolve)
        stats?.TrySpendEnergy(stats.MaxEnergy);

        pendingTarget = null;
        isDigivolving = false;
    }
}

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    public EnemyDefinition definition;

    private Animator anim;
    private float _flyTimeRemaining;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (definition == null)
        {
            Debug.LogError($"{name}: EnemyDefinition missing!");
            enabled = false;
            return;
        }

        if (definition.animatorController != null)
            anim.runtimeAnimatorController = definition.animatorController;
    }

    private void Update()
    {
        // Example: if they can fly and are grounded (or “resting”), refill fuel
        // (You’d decide what “grounded” means for your enemy logic)
        // This is here just to show the same pattern works.
    }
}

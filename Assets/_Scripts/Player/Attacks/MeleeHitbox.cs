using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private Transform owner;
    private Transform followPoint;
    private float lifetime;

    [SerializeField] private LayerMask hitMask;
    [SerializeField] private int damage = 1;

    private ElementType damageType = ElementType.None;

    public void Initialize(
        Transform owner,
        Transform followPoint,
        int damage,
        float lifetime,
        LayerMask hitMask,
        ElementType damageType = ElementType.None)   // <-- optional
    {
        this.owner = owner;
        this.followPoint = followPoint;
        this.damage = damage;
        this.lifetime = Mathf.Max(0.05f, lifetime);
        this.hitMask = hitMask;
        this.damageType = damageType;

        Destroy(gameObject, this.lifetime);
    }

    private void Update()
    {
        if (followPoint != null)
            transform.position = followPoint.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore owner
        if (owner != null && other.transform.IsChildOf(owner))
            return;

        // Only hit allowed layers
        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        if (other.GetComponentInParent<EnemyHealth>() is EnemyHealth health)
        {
            health.TakeDamage(damage, damageType);
            // NOTE: Do NOT destroy melee hitbox here if you want multi-hit during its lifetime
            // Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private Transform owner;
    private LayerMask hitMask;
    private Transform followPoint;
    private float lifetime;

    public void Initialize(
        Transform owner,
        Transform followPoint,
        int damage,
        float lifetime,
        LayerMask hitMask)
    {
        this.owner = owner;
        this.followPoint = followPoint;
        this.damage = damage;
        this.lifetime = Mathf.Max(0.05f, lifetime);
        this.hitMask = hitMask;

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

        if (other.TryGetComponent<EnemyHealth>(out var health))
            health.TakeDamage(damage);
    }
}

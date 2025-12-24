using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float hoverTime = 1f;
    [SerializeField] private float lifetime = 3f;

    [SerializeField] private LayerMask hitMask;

    private Rigidbody2D rb;
    private Transform owner;
    private Transform followPoint;
    private float dirX = 1f;
    private Coroutine routine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    public void Initialize(
        Transform owner,
        Transform followPoint,
        float dirX,
        float speed,
        int damage,
        float hoverTime,
        float lifetime,
        LayerMask hitMask)
    {
        this.owner = owner;
        this.followPoint = followPoint;
        this.dirX = Mathf.Sign(dirX);
        this.speed = speed;
        this.damage = damage;
        this.hoverTime = Mathf.Max(0f, hoverTime);
        this.lifetime = Mathf.Max(0.1f, lifetime);
        this.hitMask = hitMask;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HoverThenLaunch());
    }

    private IEnumerator HoverThenLaunch()
    {
        // Hover: do NOT move, just follow the spawn point
        rb.linearVelocity = Vector2.zero;

        float t = 0f;
        while (t < hoverTime)
        {
            t += Time.deltaTime;

            if (followPoint != null)
                transform.position = followPoint.position;

            yield return null;
        }

        // Launch
        rb.linearVelocity = new Vector2(dirX * speed, 0f);

        // Flip visuals if needed
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dirX < 0 ? -1f : 1f);
        transform.localScale = s;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore owner
        if (owner != null && other.transform.IsChildOf(owner))
            return;

        // Only hit layers in the mask
        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        if (other.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(damage);

        Destroy(gameObject);
    }
}

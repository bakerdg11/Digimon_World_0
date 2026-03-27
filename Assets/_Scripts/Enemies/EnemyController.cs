using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public EnemyDefinition definition;

    [Header("Patrol Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Attack Spawn Point (for ranged)")]
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Target")]
    [SerializeField] private Transform player; // assign or auto-find

    private Animator anim;
    private Rigidbody2D rb;

    private Transform _currentPatrolTarget;
    private bool _isChasing;
    private float _nextAttackTime;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        rb.freezeRotation = true;

        if (definition == null)
        {
            Debug.LogError($"{name}: EnemyDefinition missing!");
            enabled = false;
            return;
        }

        if (definition.animatorController != null)
            anim.runtimeAnimatorController = definition.animatorController;

        // Choose first patrol target
        if (pointA != null && pointB != null)
            _currentPatrolTarget = pointB;

        // Auto-find player if not assigned (assumes tag "Player")
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null || pointA == null || pointB == null)
        {
            // still allow idle anim
            anim.SetBool("IsWalking", false);
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        bool playerInFront = IsPlayerInFront();
        bool canAggro = playerInFront && distToPlayer <= definition.aggroDistance;

        // enter chase
        if (!_isChasing && canAggro)
            _isChasing = true;

        // exit chase (hysteresis)
        if (_isChasing && distToPlayer > definition.stopChaseDistance)
            _isChasing = false;

        // ATTACK decision (only if chasing and in range)
        if (_isChasing)
        {
            TryAttack(distToPlayer);
        }
    }

    private void FixedUpdate()
    {
        if (player == null || pointA == null || pointB == null) return;

        // If in attack "windup" animation, you can optionally stop movement here
        // (You can add an isAttacking bool later if you want)
        bool shouldMove = true;

        Vector2 targetPos;
        float speed;

        if (_isChasing)
        {
            targetPos = player.position;
            speed = definition.chaseSpeed;
        }
        else
        {
            targetPos = _currentPatrolTarget.position;
            speed = definition.patrolSpeed;
        }

        if (shouldMove)
        {
            MoveToward(targetPos, speed);

            // Patrol swap if arrived
            if (!_isChasing)
            {
                float d = Vector2.Distance(transform.position, targetPos);
                if (d <= definition.arriveThreshold)
                    _currentPatrolTarget = (_currentPatrolTarget == pointA) ? pointB : pointA;
            }
        }
    }

    private void MoveToward(Vector2 target, float speed)
    {
        Vector2 pos = rb.position;
        Vector2 dir = (target - pos).normalized;

        // Move only on X for a typical platformer ground enemy
        Vector2 vel = new Vector2(dir.x * speed, rb.linearVelocity.y);
        rb.linearVelocity = vel;

        // Face movement direction
        if (dir.x > 0.01f) FaceRight();
        else if (dir.x < -0.01f) FaceLeft();

        anim.SetBool("IsWalking", Mathf.Abs(vel.x) > 0.05f);
    }

    private void TryAttack(float distToPlayer)
    {
        if (Time.time < _nextAttackTime) return;

        switch (definition.attackType)
        {
            case EnemyAttackType.Melee:
                if (distToPlayer <= definition.meleeRange)
                {
                    _nextAttackTime = Time.time + definition.meleeCooldown;
                    anim.SetTrigger("Attack"); // make sure your animator has this trigger
                    // Damage is best applied via animation event / overlap box (see below)
                }
                break;

            case EnemyAttackType.Ranged:
                if (distToPlayer <= definition.rangedRange && definition.projectilePrefab != null)
                {
                    _nextAttackTime = Time.time + definition.rangedCooldown;
                    anim.SetTrigger("Attack"); // optional, or separate "Shoot"
                    ShootProjectile();
                }
                break;
        }
    }

    private void ShootProjectile()
    {
        if (definition.projectilePrefab == null) return;

        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;

        GameObject go = Instantiate(definition.projectilePrefab, spawnPos, Quaternion.identity);

        float dirX = IsFacingLeft() ? -1f : 1f;

        // Simple projectile: Rigidbody2D velocity
        if (go.TryGetComponent<Rigidbody2D>(out var prb))
        {
            prb.gravityScale = 0f;
            prb.linearVelocity = new Vector2(dirX * definition.projectileSpeed, 0f);
        }

        // If you have a custom projectile script, call Initialize(owner, dir, speed, damage, etc.)
    }

    private bool IsPlayerInFront()
    {
        Vector2 toPlayer = (player.position - transform.position);
        float dirX = toPlayer.x;

        // Our forward direction: -1 if facing left, +1 if facing right
        float forward = IsFacingLeft() ? -1f : 1f;

        // "Dot" on X axis (since 2D side scroller)
        float dot = Mathf.Sign(dirX) * forward;

        // dot = 1 means player is in front, -1 behind
        return dot >= definition.faceDotThreshold;
    }

    private bool IsFacingLeft() => transform.localScale.x < 0f;

    private void FaceLeft()
    {
        var s = transform.localScale;
        s.x = -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    private void FaceRight()
    {
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }
}
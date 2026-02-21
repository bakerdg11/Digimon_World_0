using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class BasePlayerController : MonoBehaviour
{
    [Header("Character Data")]
    public CharacterDefinition currentCharacter;

    private Rigidbody2D rb;
    private Animator anim;
    public PlayerStats playerStats;


    // New Input System
    private PlayerInputActions _input;
    private Vector2 _moveInput;
    // Cached delegates so we can unsubscribe safely
    private System.Action<InputAction.CallbackContext> _onMovePerformed;
    private System.Action<InputAction.CallbackContext> _onMoveCanceled;
    private System.Action<InputAction.CallbackContext> _onJumpPerformed;
    private System.Action<InputAction.CallbackContext> _onDashPerformed;
    private System.Action<InputAction.CallbackContext> _onMeleeAttackPerformed;
    private System.Action<InputAction.CallbackContext> _onRangedAttackPerformed;
    // Queued button presses (processed in Update)
    private bool _jumpQueued;
    private bool _dashQueued;
    private bool _meleeQueued;
    private bool _rangedQueued;


    [Header("Direction Flip")]
    private float horizontal;
    private bool facingLeft = true;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1.7f, 0.1f);
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    [Header("Wall check to prevent floating")]
    [SerializeField] private Transform frontCheck;
    [SerializeField] private Vector2 frontCheckSize = new Vector2(0.1f, 1.6f);
    [SerializeField] private LayerMask wallLayer;

    private float _flyTimeRemaining;

    [Header("Dash")]
    [SerializeField] private bool isDashing;
    private float _dashEndTime;
    private float _nextDashTime;
    public bool IsDashing => isDashing;

    [Header("Attack Spawn Points")]
    [SerializeField] private Transform meleeSpawnPoint;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private LayerMask projectileHitMask;
    private bool isAttacking;
    private float lastMeleeTime;
    private float lastRangedTime;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        var col = GetComponent<Collider2D>();
        if (col != null)
            groundCheckSize.x = col.bounds.size.x * 0.95f;

        if (_input == null)
            _input = new PlayerInputActions();

        rb.freezeRotation = true;

        ApplyCharacterDefinition(currentCharacter);

        // Prepare delegates
        _onMovePerformed = ctx => _moveInput = ctx.ReadValue<Vector2>();
        _onMoveCanceled = ctx => _moveInput = Vector2.zero;
        _onJumpPerformed = ctx => _jumpQueued = true;
        _onDashPerformed = ctx => _dashQueued = true;
        _onMeleeAttackPerformed = ctx => _meleeQueued = true;
        _onRangedAttackPerformed = ctx => _rangedQueued = true;
    }



    private void OnEnable()
    {
        _input.Enable();

        _input.Player.Move.performed += _onMovePerformed;
        _input.Player.Move.canceled += _onMoveCanceled;

        _input.Player.Jump.performed += _onJumpPerformed;
        _input.Player.Dash.performed += _onDashPerformed;

        _input.Player.MeleeAttack.performed += _onMeleeAttackPerformed;
        _input.Player.RangedAttack.performed += _onRangedAttackPerformed;
    }
    private void OnDisable()
    {
        _input.Player.Move.performed -= _onMovePerformed;
        _input.Player.Move.canceled -= _onMoveCanceled;

        _input.Player.Jump.performed -= _onJumpPerformed;
        _input.Player.Dash.performed -= _onDashPerformed;

        _input.Player.MeleeAttack.performed -= _onMeleeAttackPerformed;
        _input.Player.RangedAttack.performed -= _onRangedAttackPerformed;

        _input.Disable();
    }



    // ================================
    // UPDATE   &   FIXED UPDATE
    // ================================
    private void Update()
    {
        HandleFlip();

        horizontal = Mathf.Clamp(_moveInput.x, -1f, 1f);

        isGrounded = IsGrounded();
        anim.SetBool("IsGrounded", isGrounded);

        // Stay in jump while airborne
        bool isInAir = !isGrounded;
        anim.SetBool("IsInAir", isInAir);

        // Falling if airborne and moving downward
        bool isFalling = isInAir && rb.linearVelocity.y < -0.2f;
        anim.SetBool("IsFalling", isFalling);

        bool isWalking = Mathf.Abs(horizontal) > 0.01f;
        anim.SetBool("IsWalking", isWalking && !isAttacking);

        if (isGrounded)
        {
            // Refill fuel
            if (currentCharacter.canFly && currentCharacter.limitFlight)
            {
                float refillPerSecond = currentCharacter.maxFlyTime * currentCharacter.flyRefillRate;
                _flyTimeRemaining = Mathf.Min(currentCharacter.maxFlyTime, _flyTimeRemaining + refillPerSecond * Time.deltaTime);
            }
        }

        // End dash when time is up
        if (isDashing && Time.time >= _dashEndTime)
        {
            isDashing = false;
        }

        if (isAttacking)
        {
            horizontal = 0f;
            anim.SetBool("IsWalking", false);
        }

        if (_jumpQueued)
        {
            _jumpQueued = false;
            HandleJump();
        }

        if (_dashQueued)
        {
            _dashQueued = false;
            HandleDash();
        }

        if (_meleeQueued || _rangedQueued)
        {
            HandleAttacks();
            _meleeQueued = false;
            _rangedQueued = false;
        }


        // --------------------------------------------- Testing
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            playerStats.AddEnergy(50);
        }
    }

    private void FixedUpdate()
    {
        float targetHorizontal = horizontal * currentCharacter.walkSpeed;

        bool pushingIntoWall =
            IsTouchingWall() &&
            !isGrounded &&
            Mathf.Abs(horizontal) > 0.01f;

        if (pushingIntoWall)
            targetHorizontal = 0f;

        rb.linearVelocity = new Vector2(targetHorizontal, rb.linearVelocity.y);

        // Dash overrides normal movement
        if (isDashing)
        {
            float dir = facingLeft ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir * currentCharacter.dashSpeed, rb.linearVelocity.y);
            return;
        }

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
    }


    // ================================
    // CHARACTER DEFINITIONS - CHARACTER SWAPPING
    // ================================
    public void ApplyCharacterDefinition(CharacterDefinition def)
    {
        if (def == null)
        {
            Debug.LogWarning("CharacterDefinition is NULL!");
            return;
        }

        currentCharacter = def;

        if (def.animatorController != null)
            anim.runtimeAnimatorController = def.animatorController;

        _flyTimeRemaining = def.maxFlyTime; // start full (or 0 if you want)

        isDashing = false;

        lastMeleeTime = -999f;
        lastRangedTime = -999f;

        var stats = GetComponent<PlayerStats>();
        if (stats != null)
            stats.OnCharacterChanged(def);
    }


    // ================================
    // GROUNDED & WALL CHECKS
    // ================================
    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        if (frontCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(frontCheck.position, frontCheckSize);
        }
    }

    private bool IsTouchingWall()
    {
        if (frontCheck == null) return false;

        return Physics2D.OverlapBox(
            frontCheck.position,
            frontCheckSize,
            0f,
            wallLayer
        );
    }




    // ================================
    // FACING / FLIP
    // ================================
    private void HandleFlip()
    {
        if (horizontal > 0.01f && facingLeft)
        {
            Flip();
        }
        else if (horizontal < -0.01f && !facingLeft)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingLeft = !facingLeft;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }



    // ================================
    // JUMP / FLY
    // ================================
    private void HandleJump()
    {
        // Normal jump from ground (everyone)
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentCharacter.jumpForce);
            anim.SetBool("IsInAir", true);
            return;
        }

        // Mid-air "flap" / fly boost (only flyers)
        if (!currentCharacter.canFly)
            return;

        // If flight is limited, require fuel
        if (currentCharacter.limitFlight && _flyTimeRemaining <= 0f)
            return;

        float force = currentCharacter.flyJumpForce > 0f ? currentCharacter.flyJumpForce : currentCharacter.jumpForce;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        anim.SetBool("IsInAir", true);

        // Spend a chunk of fuel per flap (tweak this!)
        if (currentCharacter.limitFlight)
            _flyTimeRemaining -= 0.1f; // each press costs 0.15s worth of fuel
    }




    // ================================
    // DASHING
    // ================================
    private void HandleDash()
    {
        if (currentCharacter == null) return;

        // Can this character dash?
        if (!currentCharacter.canDash)
            return;

        // Cooldown gate
        if (Time.time < _nextDashTime)
            return;

        // Don't allow stacking
        if (isDashing || isAttacking)
            return;

        // Optional: don't dash into a wall
        if (currentCharacter.dashStopsAtWall && IsTouchingWall())
            return;

        isDashing = true;

        _dashEndTime = Time.time + Mathf.Max(0.02f, currentCharacter.dashDuration);
        _nextDashTime = Time.time + Mathf.Max(0f, currentCharacter.dashCooldown);

        // Optional: dash animation trigger if you add one
        // anim.SetTrigger("Dash");

        // Snap velocity immediately for responsiveness
        float dir = facingLeft ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * currentCharacter.dashSpeed, rb.linearVelocity.y);
    }





    // ================================
    // MELEE & RANGED
    // ================================
    private void HandleAttacks()
    {
        // Melee
        if (_meleeQueued && Time.time >= lastMeleeTime + currentCharacter.meleeCooldown)
        {
            lastMeleeTime = Time.time;
            DoMeleeAttack();
        }

        // Ranged
        if (_rangedQueued && Time.time >= lastRangedTime + currentCharacter.rangedCooldown)
        {
            lastRangedTime = Time.time;
            DoRangedAttack(); // no cooldown logic inside
        }
    }

    private void DoMeleeAttack()
    {
        if (currentCharacter.meleeHitboxPrefab == null) return;

        isAttacking = true;
        anim.ResetTrigger("IsAttacking");
        anim.SetTrigger("IsAttacking");   // <-- trigger FIRST

        // spawn hitbox AFTER triggering
        Vector3 basePos = meleeSpawnPoint != null ? meleeSpawnPoint.position : transform.position;
        float dir = facingLeft ? -1f : 1f;
        Vector3 spawnPos = basePos + Vector3.right * dir * 0.1f;

        GameObject hitboxGO = Instantiate(currentCharacter.meleeHitboxPrefab, spawnPos, Quaternion.identity);

        Vector3 scale = hitboxGO.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        hitboxGO.transform.localScale = scale;

        if (hitboxGO.TryGetComponent<MeleeHitbox>(out var hitbox))
        {
            hitbox.Initialize(
                owner: transform,
                followPoint: meleeSpawnPoint,
                damage: (int)currentCharacter.meleeDamage,
                lifetime: currentCharacter.meleeLifetime,
                hitMask: projectileHitMask,
                damageType: ElementType.None // <-- or currentCharacter.elementType
            );

        }
    }

    private void DoRangedAttack()
    {
        if (currentCharacter == null) return;
        if (currentCharacter.rangedProjectilePrefab == null) return;

        isAttacking = true;
        anim.ResetTrigger("IsAttacking");
        anim.SetTrigger("IsAttacking");   // <-- trigger FIRST

        float dir = facingLeft ? -1f : 1f;
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;

        GameObject projGO = Instantiate(currentCharacter.rangedProjectilePrefab, spawnPos, Quaternion.identity);

        if (projGO.TryGetComponent<Projectile>(out var proj))
        {
            proj.Initialize(
                owner: transform,
                followPoint: projectileSpawnPoint,
                dirX: dir,
                speed: currentCharacter.rangedProjectileSpeed,
                damage: currentCharacter.rangedDamage,
                hoverTime: currentCharacter.rangedHoverTime,
                lifetime: currentCharacter.rangedLifetime,
                hitMask: projectileHitMask,
                damageType: currentCharacter.elementType
            );
        }
    }

    // Called from animation event at the end of the melee attack
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }


}
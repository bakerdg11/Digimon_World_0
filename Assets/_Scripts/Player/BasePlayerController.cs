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

    // New Input System
    private PlayerInputActions _input;
    private Vector2 _moveInput;

    // Cached delegates so we can unsubscribe safely
    private System.Action<InputAction.CallbackContext> _onMovePerformed;
    private System.Action<InputAction.CallbackContext> _onMoveCanceled;
    private System.Action<InputAction.CallbackContext> _onJumpPerformed;
    private System.Action<InputAction.CallbackContext> _onMeleeAttackPerformed;
    private System.Action<InputAction.CallbackContext> _onRangedAttackPerformed;

    private float horizontal;
    private bool facingLeft = true;

    [Header("Attack Spawn Points")]
    [SerializeField] private Transform meleeSpawnPoint;
    [SerializeField] private Transform projectileSpawnPoint;

    private bool isAttacking;
    private float lastMeleeTime;
    private float lastRangedTime;

    // Queued button presses (processed in Update)
    private bool _jumpQueued;
    private bool _meleeQueued;
    private bool _rangedQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (_input == null)
            _input = new PlayerInputActions();

        rb.freezeRotation = true;

        ApplyCharacterDefinition(currentCharacter);

        // Prepare delegates
        _onMovePerformed = ctx => _moveInput = ctx.ReadValue<Vector2>();
        _onMoveCanceled = ctx => _moveInput = Vector2.zero;
        _onJumpPerformed = ctx => _jumpQueued = true;
        _onMeleeAttackPerformed = ctx => _meleeQueued = true;
        _onRangedAttackPerformed = ctx => _rangedQueued = true;
    }

    private void OnEnable()
    {
        _input.Enable();

        // Subscribe to actions
        _input.Player.Move.performed += _onMovePerformed;
        _input.Player.Move.canceled += _onMoveCanceled;

        _input.Player.Jump.performed += _onJumpPerformed;
        _input.Player.MeleeAttack.performed += _onMeleeAttackPerformed;
        _input.Player.RangedAttack.performed += _onRangedAttackPerformed;
    }

    private void OnDisable()
    {
        // Unsubscribe cleanly
        _input.Player.Move.performed -= _onMovePerformed;
        _input.Player.Move.canceled -= _onMoveCanceled;

        _input.Player.Jump.performed -= _onJumpPerformed;
        _input.Player.MeleeAttack.performed -= _onMeleeAttackPerformed;
        _input.Player.RangedAttack.performed -= _onRangedAttackPerformed;

        _input.Disable();
    }

    private void Update()
    {
        // Horizontal input from keyboard + gamepad (Move.x)
        horizontal = Mathf.Clamp(_moveInput.x, -1f, 1f);

        // === WALKING ANIMATION ===
        bool isWalking = Mathf.Abs(horizontal) > 0.01f;
        anim.SetBool("IsWalking", isWalking && !isAttacking);

        if (isAttacking)
        {
            // While attacking, stop horizontal motion
            horizontal = 0f;
            anim.SetBool("IsWalking", false);
        }

        HandleFlip();

        // Handle queued button presses
        if (_jumpQueued)
        {
            _jumpQueued = false;
            HandleJump();
        }

        if (_meleeQueued || _rangedQueued)
        {
            HandleAttacks();
            _meleeQueued = false;
            _rangedQueued = false;
        }
    }

    private void FixedUpdate()
    {
        if (isAttacking)
        {
            // Freeze horizontal movement but allow gravity
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // === CHARACTER-BASED SPEED ===
        rb.linearVelocity = new Vector2(
            horizontal * currentCharacter.walkSpeed,
            rb.linearVelocity.y
        );
    }

    // ================================
    // CHARACTER SWAPPING
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

        lastMeleeTime = -999f;
        lastRangedTime = -999f;
    }

    // ================================
    // JUMP / FLY
    // ================================
    private void HandleJump()
    {
        // Jump is now driven by the Jump input action (button press queued)
        if (currentCharacter.canFly)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                currentCharacter.jumpForce
            );
        }
        else
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    currentCharacter.jumpForce
                );
            }
        }
    }

    private bool IsGrounded()
    {
        // Placeholder. Replace with raycast / ground check later.
        return Mathf.Abs(rb.linearVelocity.y) < 0.01f;
    }

    // ================================
    // MELEE & RANGED
    // ================================
    private void HandleAttacks()
    {
        // Melee
        if (_meleeQueued &&
            Time.time >= lastMeleeTime + currentCharacter.meleeCooldown)
        {
            lastMeleeTime = Time.time;
            DoMeleeAttack();
        }

        // Ranged
        if (_rangedQueued &&
            Time.time >= lastRangedTime + currentCharacter.rangedCooldown)
        {
            lastRangedTime = Time.time;
            DoRangedAttack();
        }
    }

    private void DoMeleeAttack()
    {
        if (currentCharacter.meleeHitboxPrefab == null)
            return;

        isAttacking = true;

        // Use spawn point if assigned, otherwise fall back to offset
        Vector3 basePos = meleeSpawnPoint != null
            ? meleeSpawnPoint.position
            : transform.position;

        float dir = facingLeft ? -1f : 1f;
        Vector3 spawnPos = basePos + Vector3.right * dir * 0.1f; // tiny offset

        var hitbox = Instantiate(
            currentCharacter.meleeHitboxPrefab,
            spawnPos,
            Quaternion.identity
        );

        // Face the correct direction (optional)
        Vector3 scale = hitbox.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        hitbox.transform.localScale = scale;

        anim.SetTrigger("IsAttacking");
    }

    private void DoRangedAttack()
    {
        if (currentCharacter.rangedProjectilePrefab == null)
            return;

        isAttacking = true;

        Vector3 basePos = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position;

        float dir = facingLeft ? -1f : 1f;
        Vector3 spawnPos = basePos;

        GameObject proj = Instantiate(
            currentCharacter.rangedProjectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        if (proj.TryGetComponent<Rigidbody2D>(out var rbProj))
        {
            rbProj.linearVelocity = new Vector2(
                dir * currentCharacter.rangedProjectileSpeed,
                0f
            );
        }

        // Optional: flip projectile sprite using localScale.x like above

        anim.SetTrigger("IsAttacking");
    }

    // Called from animation event at the end of the melee attack
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
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
}
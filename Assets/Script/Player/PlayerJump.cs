using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Burst.Intrinsics.Arm;
using static Unity.VisualScripting.Member;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    // -------------------------------------
    // JUMP FEEL
    // -------------------------------------
    [Header("Jump Power")]
    [SerializeField]
    private float _jumpForce = 16f;
    [SerializeField]
    private float _jumpHoldDuration = 0.25f;
    [SerializeField]
    private float _jumpHoldForce = 40f;

    [Header("Gravity Tuning")]
    [SerializeField]
    private float _fallGravityMultiplier = 3.5f;
    [SerializeField]
    private float _lowJumpMultiplier = 2.5f;
    [SerializeField]
    private float _maxFallSpeed = 20f;

    [Header("Coyote Time & Jump Buffer")]
    [SerializeField]
    private float _coyoteTime = 0.12f;
    [SerializeField]
    private float _jumpBufferTime = 0.15f;

    [Header("Ground Detection ")]
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private Transform _groundCheck;
    [SerializeField]
    private float _groundCheckRadius = 0.1f;

    [Header("Optional: Variable Jump Height")]
    [SerializeField]
    private bool _variableJumpHeight = true;

    [Header("Optional: Apex Modifier")]
    [SerializeField]
    private bool _apexModifier = true;
    [SerializeField]
    private float _apexThreshold = 2f;
    [SerializeField]
    private float _apexGravityMultiplier = 0.4f;

    // -------------------------------------
    // ÉTAT INTERNE
    // -------------------------------------
    private Rigidbody2D _rb;
    private float _defaultGravityScale;

    private bool _hasJumped;
    private bool _isGrounded;
    private bool _isJumping;
    private bool _jumpInputHeld;

    private float _jumpHoldTimer;
    private float _coyoteTimer;
    private float _jumpBufferTimer;

    /// <summary>
    /// Brancher sur : Action "Jump" -> Phase Started
    /// </summary>
    public void OnJumpStarted()
    {
        _jumpInputHeld = true;
        _jumpBufferTimer = _jumpBufferTime;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) 
        {
            _jumpInputHeld = true;
            _jumpBufferTimer = _jumpBufferTime;
        }
        
        else if (context.canceled) 
        {
            _jumpInputHeld = false;

            // Coupe le saut si on est encore en montée (saut court)
            if (_isJumping && _rb.velocity.y > 0f)
                _isJumping = false;
        }

    }
    /// <summary>
    /// Brancher sur : Action "Jump" - > Phase Canceled
    /// </summary>
    public void OnJumpCanceled()
    {
        _jumpInputHeld = false;

        // Coupe le saut si on est encore en montée (saut court)
        if (_isJumping && _rb.velocity.y > 0f)
            _isJumping = false;
    }

    // ------------------------------------------
    // LIFECYCLE
    // -----------------------------------------

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _defaultGravityScale = _rb.gravityScale;

        if (_groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, -0.5f, 0);
            _groundCheck = go.transform;
        }
    }

    private void Update()
    {
        // - 1. Détection sol
        _isGrounded = Physics2D.OverlapCircle(
            _groundCheck.position, _groundCheckRadius, _groundLayer);

        // - 2. Coyote time
        if (_isGrounded)
            _coyoteTimer = _coyoteTime;
        else
            _coyoteTimer -= Time.deltaTime;

        // - 3. Jump buffer decay
        _jumpBufferTimer -= Time.deltaTime;

        // - 4. Déclencher le saut
        bool canJump = _coyoteTimer > 0f && !_hasJumped; ;
        bool wantsJump = _jumpBufferTimer > 0f;

        if (wantsJump && canJump)
            ExecuteJump();

        // - 5. Fin naturelle du hold
        if (_isJumping)
        {
            _jumpHoldTimer -= Time.deltaTime;
            if (_jumpHoldTimer <= 0f)
                _isJumping = false;
        }

        // - 6. Reset au sol
        if (_isGrounded)
        {
            _isJumping = false;
            _hasJumped = false;
        }
             
    }

    private void FixedUpdate()
    {
        ApplyJumpHold();
        ApplyGravityModifiers();
        ClampFallSpeed();
    }

    // --------------------------------------------
    // LOGIQUE INTERNE
    // --------------------------------------------

    /// <summary>
    /// Déclenche le saut : applique la force initiale et initialise tous les timers.
    /// </summary>
    private void ExecuteJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);  // Impulse applique la force en une seule frame (contrairement à Forcequi s'étale sur le temps)

        _isGrounded = false;
        _isJumping = true;
        _hasJumped = true;
        _jumpHoldTimer = _jumpHoldDuration;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;
    }
    /// <summary>
    /// Applique une force vers le haut tant que la touche est maintenue.
    /// C'est ce qui permet au joueur de contrôler la hauteur du saut en tenant la touche.
    /// </summary>
    private void ApplyJumpHold()
    {
        if (_isJumping && _jumpInputHeld && _rb.velocity.y > 0f)
            _rb.AddForce(Vector2.up * _jumpHoldForce * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    /// <summary>
    /// Ajuste gravityScale selon la phase du saut pour obtenir une courbe de trajectoire
    /// </summary>
    private void ApplyGravityModifiers()
    {
        float vy = _rb.velocity.y;
        // Au sommet du saut, |vy| est très faible. On réduit la gravité pour
        // créer un mini-flottement qui donne du temps au joueur pour se repositionner.
        if (_apexModifier && Mathf.Abs(vy) < _apexThreshold && !_isGrounded)
        {
            _rb.gravityScale = _defaultGravityScale * _apexGravityMultiplier;
            return;
        }

        if (vy < 0f)
            _rb.gravityScale = _defaultGravityScale * _fallGravityMultiplier; // Saut long : Gravité multipliée pour une chute plus rapide que la montée.
        else if (vy > 0f && _variableJumpHeight && !_jumpInputHeld)
            _rb.gravityScale = _defaultGravityScale * _lowJumpMultiplier; // Saut court : Freine la montée plus vite qu'en chute libre normale,
        else
            _rb.gravityScale = _defaultGravityScale;
    }

    /// <summary>
    /// Plafonne la vitesse de chute à _maxFallSpeed.
    /// Sans ce clamp, la vitesse de chute pourrait trop s'accumuler et
    /// traverser des plateformes fines.
    /// </summary>
    private void ClampFallSpeed()
    {
        if (_rb.velocity.y < -_maxFallSpeed)
            _rb.velocity = new Vector2(_rb.velocity.x, -_maxFallSpeed);
    }
}

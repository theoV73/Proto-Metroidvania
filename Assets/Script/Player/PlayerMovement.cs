using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField]
    private float _maxSpeed = 9f;
    [SerializeField]
    private float _maxAirSpeed = 8f;

    [Header("Acceleration")]
    [SerializeField]
    private float _groundAcceleration = 80f;
    [SerializeField]
    private float _airAcceleration = 50f;

    [Header("Deceleration")]
    [SerializeField]
    private float _groundDeceleration = 100f;
    [SerializeField]
    private float _airDeceleration = 20f;

    [SerializeField, Header("Turnaround")]
    private float _turnaroundBonus = 50f;

    [Header("Air Control")]
    [SerializeField,Range(0f, 1f)]
    private float _airControlFactor = 0.85f;

    [Header("Ground Detection")]
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private Transform _groundCheck;
    [SerializeField]
    private float _groundCheckRadius = 0.1f;

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private float _inputX;

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if(ctx.performed||ctx.started)
            _inputX = ctx.ReadValue<Vector2>().x;
        else if (ctx.canceled)
            _inputX = 0f;
    }
    

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_groundCheck == null)
        {
            // Cherche d'abord un GroundCheck existant (créé par PlayerJump)
            var existing = transform.Find("GroundCheck");
            if (existing != null)
            {
                _groundCheck = existing;
            }
            else //sinon en crée un
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0, -0.5f, 0);
                _groundCheck = go.transform;
            }
        }
    }

    private void Update()
    {
        // Détection sol
        _isGrounded = Physics2D.OverlapCircle(
            _groundCheck.position, _groundCheckRadius, _groundLayer);
    }

    private void FixedUpdate()
    {
        ApplyHorizontalMovement();
    }

    private void ApplyHorizontalMovement()
    {
        float currentVx = _rb.velocity.x;
        float targetSpeed = _isGrounded ? _maxSpeed : _maxAirSpeed;
        float desiredVx = _inputX * targetSpeed;

        float acceleration;

        if (Mathf.Abs(_inputX) < 0.01f)
        {
            // ── Aucun input : décélération (frottement)
            acceleration = _isGrounded ? _groundDeceleration : _airDeceleration;
        }
        else if (Mathf.Sign(_inputX) != Mathf.Sign(currentVx) && Mathf.Abs(currentVx) > 0.1f)
        {
            // ── Changement de direction : accélération + bonus turnaround
            float baseAccel = _isGrounded ? _groundAcceleration : _airAcceleration * _airControlFactor;
            acceleration = baseAccel + _turnaroundBonus;
        }
        else
        {
            // ── Accélération normale
            acceleration = _isGrounded //remplace un if + else if
                ? _groundAcceleration
                : _airAcceleration * _airControlFactor;
        }

        float newVx = Mathf.MoveTowards(currentVx, desiredVx, acceleration * Time.fixedDeltaTime);
        _rb.velocity = new Vector2(newVx, _rb.velocity.y);
    }
}

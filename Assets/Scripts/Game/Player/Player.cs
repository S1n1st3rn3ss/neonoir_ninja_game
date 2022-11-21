using System;
using System.Numerics;
using Unity.VisualScripting;

using UnityEngine;
using Vector2 = UnityEngine.Vector2;


public class Player : MonoBehaviour
{
    [SerializeField] private Hook _hook;

    public float moveSpeed;
    public float acceleration;
    public float deceleration;
    public float velocityPower;

    public float frictionAmount;

    public float jumpForce;

    public float jumpBufferTime;
    public float jumpCoyoteTime;

    public float jumpCutMultiplier;

    private Rigidbody2D _rigidbody;
    private Collider2D _collider;
    private bool _isGround;
    
    private float lastGroundedTime = 0;
    private float lastJumpTime = 0;
    private bool isJumping = false;
    private bool jumpInputReleased = true;


    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UseHook();

        UIManager.SetVelocityText(((int)(_rigidbody.velocity.magnitude * 10) / 10.0f).ToString());
    }
    
    private int UpdateMovementKeys()
    {
        if (Input.GetKey(KeyCode.A))
        {
            return -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            return 1;
        }
        return 0;
    }
    /// <summary>
    /// Применение крюка-кошки.
    /// </summary>
    private void UseHook()
    {
        if (Input.GetKeyUp(KeyCode.LeftShift) && _hook.enabled)
        {
            _hook.DestroyHook();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _hook.CreateHook();
        }
    }

    /// <summary>
    /// Проверка наличия коллизий
    /// </summary>
    /// <param name="collision2D">Детали коллизии, можно использовать позже, на данный момент исользуется для повторения старого функционала</param>
    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        _isGround = collision2D.gameObject.CompareTag("Ground");
    }
    private void OnCollisionExit2D()
    {
        _isGround = false;
    }

    private void Jump()
    {
        _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastGroundedTime = 0;
        lastJumpTime = 0;
        isJumping = true;
        jumpInputReleased = false;
        onJump();
        
    }

    public void onJump()
    {
        lastJumpTime = jumpBufferTime;
    }

    public void onJumpUp()
    {
        if (_rigidbody.velocity.y > 0 && isJumping)
        {
            _rigidbody.AddForce(Vector2.down * (_rigidbody.velocity.y * (1 - jumpCutMultiplier)), ForceMode2D.Impulse);
        }

        jumpInputReleased = false;
        lastJumpTime = 0;
    }

    private void FixedUpdate()
    {
        int moveInput = UpdateMovementKeys();
        isJumping = !_isGround;
        jumpInputReleased = !Input.GetKey(KeyCode.Space);
        
        #region Timers
        
        if (_isGround)
        {
            lastGroundedTime = jumpCoyoteTime;
        }
        else
        {
            lastGroundedTime -= Time.deltaTime;
        }
        
        if (isJumping)
        {
            lastJumpTime = jumpBufferTime;
        }
        else
        {
            lastJumpTime -= Time.deltaTime;
        }
        
        #endregion

        #region Run
        
        float targetSpeed = moveInput * moveSpeed;
        
        float speedDif = targetSpeed - _rigidbody.velocity.x;
        
        float accelerationRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration :
        deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelerationRate, velocityPower) * Mathf.Sign(speedDif);
        
        _rigidbody.AddForce(movement * Vector2.right);
        
        #endregion
        
        #region Friction

        if (lastGroundedTime > 0 && moveInput == 0)
        {
            float amount = Mathf.Min(Mathf.Abs(_rigidbody.velocity.x), Mathf.Abs(frictionAmount));

            amount *= Mathf.Sign(_rigidbody.velocity.x);
            
            _rigidbody.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        
        #endregion
        
        #region Jump

        if (lastGroundedTime > 0 && lastJumpTime > 0 && !isJumping && Input.GetKey(KeyCode.Space))
        {
            Jump();
        }
        onJumpUp();
        #endregion
        
        #region Coyote 
        
        #endregion
    }
    
}

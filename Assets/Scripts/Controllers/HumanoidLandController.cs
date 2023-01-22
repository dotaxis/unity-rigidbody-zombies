using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

public class HumanoidLandController : MonoBehaviour
{
    public Transform CameraFollow;
    [SerializeField] private HumanoidLandInput _input;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private CapsuleCollider _capsuleCollider;

    private Vector3 _playerMoveInput;

    private Vector3 _playerLookInput;
    private Vector3 _prevPlayerLookInput;
    private float _cameraPitch;
    [SerializeField] private float _playerLookInputLerpTime = 0.35f;
    
    // [Header("Shooting")]
    // [SerializeField] private Transform pfContactParticle;
    // [SerializeField] private Transform pfMuzzleFlash;
    // [SerializeField] private Transform gunTip;
    // [SerializeField] private float fireRate;
    // [SerializeField] private float bulletCooldown;
    // [SerializeField] private float bulletTimer;

    [Header("Movement")]
    [SerializeField] private float _movementMultiplier = 30.0f;
    [SerializeField] float _sprintMultiplier = 1.5f;
    [SerializeField] float rotationSpeedMultiplier = 180.0f;
    [SerializeField] float _pitchSpeedMultiplier = 180.0f;

    [Header("Ground Check")] [SerializeField]
    bool _playerIsGrounded;

    [SerializeField] [Range(0.0f, 1.5f)] float _groundCheckRadiusMultiplier = 0.9f;

    [SerializeField] [Range(-0.95f, 1.05f)]
    float _groundCheckDistance = 0.05f;

    private RaycastHit _groundCheckHit;

    /*[Header("Gravity)")]
    [SerializeField] private float _gravityFallCurrent;
    [SerializeField] private float _gravityFallMin;
    [SerializeField] private float _gravityFallMax;
    [SerializeField] [Range(-5.0f, -35.0f)] private float _gravityFallIncrementAmount = -20f;
    [SerializeField] private float _gravityFallIncrementTime;
    [SerializeField] private float _playerFallTimer;
    [SerializeField] private float _gravity;*/

    [Header("Jumping")] [SerializeField] float _initialJumpForce = 750.0f;
    [SerializeField] float _continualJumpForceMultiplier = 0.1f;
    [SerializeField] float _jumpTime = 0.175f;
    [SerializeField] float _jumpTimeCounter = 0.0f;
    [SerializeField] float _coyoteTime = 0.15f;
    [SerializeField] float _coyoteTimeCounter = 0.0f;
    [SerializeField] float _jumpBufferTime = 0.2f;
    [SerializeField] float _jumpBufferTimeCounter = 0.0f;
    [SerializeField] bool _playerIsJumping = false;
    [SerializeField] bool _jumpWasPressedLastFrame = false;
    [SerializeField] float jumpSpeedMultiplier = 0.1f;


    [SerializeField] bool _playerIsSprinting = false;
    [FormerlySerializedAs("_playerIsAiming")] public bool playerIsAiming = false;
    [SerializeField] private Rig aimRig;

    private Vector3 _mouseWorldPosition;

    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsSprinting = Animator.StringToHash("isSprinting");
    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private static readonly int VelocityZ = Animator.StringToHash("VelocityZ");
    private static readonly int VelocityX = Animator.StringToHash("VelocityX");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        SetSprintOrAim();
        aimRig.weight = Mathf.Lerp(aimRig.weight, playerIsAiming ? 1f : 0f, Time.deltaTime * 20f);
    }

    private void FixedUpdate()
    {
        _animator.SetBool(IsJumping, _playerIsJumping);
        _animator.SetBool(IsSprinting, (_playerIsSprinting && _playerMoveInput.z >= 0));
        _animator.SetBool(IsAiming, playerIsAiming);
        var velocityZ = _playerMoveInput.normalized.z;
        var velocityX = _playerMoveInput.normalized.x;
        _animator.SetFloat(VelocityZ, velocityZ, 0.1f, Time.deltaTime);
        _animator.SetFloat(VelocityX, velocityX, 0.1f, Time.deltaTime);


        _playerLookInput = GetLookInput();
        PlayerLook();
        PitchCamera();
        _playerMoveInput = GetMoveInput() * _rigidbody.mass;
        _playerIsGrounded = PlayerGroundCheck();
        _playerMoveInput.y += PlayerJump();
        _playerMoveInput = PlayerMove();
        _playerMoveInput = PlayerSprint();
        if (!_playerIsGrounded)
        {
            _playerMoveInput.x *= jumpSpeedMultiplier;
            _playerMoveInput.z *= jumpSpeedMultiplier;
        }
    //        Shoot();


        _rigidbody.AddRelativeForce(_playerMoveInput, ForceMode.Force);
    }

    private Vector3 GetLookInput()
    {
        _prevPlayerLookInput = _playerLookInput;
        _playerLookInput = new Vector3(_input.LookInput.x, _input.LookInput.y * (_input.InvertMouseY ? -1 : 1), 0.0f);
        return Vector3.Lerp(_prevPlayerLookInput, _playerLookInput * Time.deltaTime, _playerLookInputLerpTime);
    }

    private void PlayerLook()
    {
        transform.rotation = Quaternion.Euler(0.0f,
            transform.rotation.eulerAngles.y + (_playerLookInput.x * rotationSpeedMultiplier), 0.0f);
    }

    private void PitchCamera()
    {
        Vector3 rotationValues = CameraFollow.rotation.eulerAngles;
        _cameraPitch += _playerLookInput.y * _pitchSpeedMultiplier;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -89.9f, 89.9f);

        CameraFollow.rotation = Quaternion.Euler(_cameraPitch, rotationValues.y, rotationValues.z);
    }

    private Vector3 GetMoveInput()
    {
        return new Vector3(_input.MoveInput.x, 0.0f, _input.MoveInput.y);
    }

    private Vector3 PlayerMove()
    {
        return new Vector3(_playerMoveInput.x * _movementMultiplier,
            _playerMoveInput.y,
            _playerMoveInput.z * _movementMultiplier);
    }

    private Vector3 PlayerSprint()
    {
        Vector3 sprintSpeed = _playerMoveInput;
        if (_playerIsSprinting)
        {
            sprintSpeed.x *= _sprintMultiplier;
            sprintSpeed.z *= _sprintMultiplier;
        }

        return sprintSpeed;
    }

    private bool PlayerGroundCheck()
    {
        float sphereCastRadius = _capsuleCollider.radius * _groundCheckRadiusMultiplier;
        float sphereCastTravelDistance = _capsuleCollider.bounds.extents.y - sphereCastRadius + _groundCheckDistance;
        return RotaryHeart.Lib.PhysicsExtension.Physics.SphereCast(_rigidbody.position, sphereCastRadius, Vector3.down,
            out _groundCheckHit,
            sphereCastTravelDistance, RotaryHeart.Lib.PhysicsExtension.PreviewCondition.Both);
    }

    private float PlayerJump()
    {
        var calculatedJumpInput = _playerMoveInput.y;
        
        SetJumpTimeCounter();
        SetCoyoteTimeCounter();
        SetJumpBufferTimeCounter();

        if (_jumpBufferTimeCounter > 0.0f && !_playerIsJumping && _coyoteTimeCounter > 0.0f)
        {
            calculatedJumpInput = _initialJumpForce;
            _playerIsJumping = true;
            _jumpBufferTimeCounter = 0.0f;
            _coyoteTimeCounter = 0.0f;
        }
        else if (_input.JumpIsPressed && _playerIsJumping && !_playerIsGrounded && _jumpTimeCounter > 0.0f)
        {
            calculatedJumpInput = _initialJumpForce * _continualJumpForceMultiplier;
        }
        else if (_playerIsJumping && _playerIsGrounded)
        {
            _playerIsJumping = false;
        }

        return calculatedJumpInput;
    }

    private void SetJumpTimeCounter()
    {
        if
            (_playerIsJumping && !_playerIsGrounded) _jumpTimeCounter -= Time.fixedDeltaTime;
        else
            _jumpTimeCounter = _jumpTime;
    }

    private void SetCoyoteTimeCounter()
    {
        if
            (_playerIsGrounded) _coyoteTimeCounter = _coyoteTime;
        else
            _coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    private void SetJumpBufferTimeCounter()
    {
        if (!_jumpWasPressedLastFrame && _input.JumpIsPressed && _playerIsGrounded)
            _jumpBufferTimeCounter = _jumpBufferTime;
        else if (_jumpBufferTimeCounter > 0.0f)
            _jumpBufferTimeCounter -= Time.fixedDeltaTime;

        _jumpWasPressedLastFrame = _input.JumpIsPressed;
    }

    private void SetSprintOrAim()
    {
        if (_input.SprintIsPressed && _playerMoveInput.z >= 0) _playerIsSprinting = true;
        else if (!_input.SprintIsPressed || _playerMoveInput.z < 0) _playerIsSprinting = false;

        if (_input.ZoomCameraIsPressed) playerIsAiming = true;
        else if (!_input.ZoomCameraIsPressed) playerIsAiming = false;

        if (_playerIsSprinting && playerIsAiming) _playerIsSprinting = false;
    }

    

/*private float PlayerGravity()
{
    if (_playerIsGrounded)
    {
        _gravity = 0.0f;
        _gravityFallCurrent = _gravityFallMin;
    }
    else
    {
        _playerFallTimer -= Time.fixedDeltaTime;
        if (_playerFallTimer < 0.0f)
        {
            if (_gravityFallCurrent > _gravityFallMax)
                _gravityFallCurrent += _gravityFallIncrementAmount;
            _playerFallTimer = _gravityFallIncrementTime;
            _gravity = _gravityFallCurrent;
        }
    }

    return _gravity;
}*/
}
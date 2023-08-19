using System;
using UnityEngine;

[Serializable]
public partial class Movement
{
    [Header("Acceleration")]
    [SerializeField] private float _groundAcceleration = 100f;
    [SerializeField] private float _groundBaseLimit = 12f;

    [SerializeField] private float _airAcceleration = 100f;
    [SerializeField] private float _airBaseLimit = 1f;

    // [SerializeField] private float _duckAcceleration = 6f;
    // [SerializeField] private float _duckBaseLimit = 6f;
    
    [Header("Forces")]
    [SerializeField] private float _gravity = 16f;
    [SerializeField] private float _friction = 6f;
    [SerializeField] private float _jumpHeight = 6f;
    [SerializeField] private float _rampSlideLimit = 5f;

    // [Header("Collider")]
    // [SerializeField] private float _duckColliderHeight = 0.6f;
    // [SerializeField] private float _standColliderHeight = 1f;

    [Header("Movement Toggles")]
    [SerializeField] private bool _additiveJump = true;
    [SerializeField] private bool _autoJump = true;
    [SerializeField] private bool _clampGroundSpeed = false;
    [SerializeField] private bool _disableBunnyHopping = false;

    #region Global Variables
    private Transform _transform;
    private Rigidbody _rb;
    private CustomInput _customInput;
    private SimpleCollider _collision;
    private Camera _cam;

    // Input
    private Vector3 _inputDir;

    // 
    private Vector3 _vel;
    public Vector3 Velocity { get => _vel; }
    private float _currentSpeed = 0f;
    public float CurrentSpeed { get => _currentSpeed; }

    // Jump
    private bool _ableToJump = true;

    // Duck
    // private bool duringCrouch = false;

    // Boolean Properties
    private bool JumpPending => _customInput._jumpPending && _collision.OnGround;
    private bool Ducking => _customInput.IsDucking;


    #endregion
}
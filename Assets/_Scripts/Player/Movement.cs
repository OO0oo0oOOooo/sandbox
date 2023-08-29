using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))] [RequireComponent(typeof(SimpleCollider))]
public partial class Movement : MonoBehaviour
{
    #region Unity Event Functions
    private void Awake()
    {
        _transform = transform;
        _customInput = GetComponent<CustomInput>();
        _rb = GetComponent<Rigidbody>();
        _collision = GetComponent<SimpleCollider>();
        _cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        _inputDir = _customInput.InputDir;
    }
    
    private void FixedUpdate()
    {
        // Sync before changing
        _vel = _rb.velocity;

        // Measure Speed
        _currentSpeed = _rb.velocity.magnitude;

        // Clamp speed if bunnyhopping is disabled
        if (_disableBunnyHopping && _collision.OnGround)
            ClampVel(_groundBaseLimit);

        if (JumpPending)
            Jump();

        // We use air physics if moving upwards at high speed
        if (_rampSlideLimit >= 0f && _vel.y > _rampSlideLimit)
            _collision.OnGround = false;
        
        // Select movement state
        if(_collision.OnGround)
        {
            // Rotate movement vector to match ground tangent
            _inputDir = Vector3.Cross(Vector3.Cross(_collision.AverageNormal, _inputDir), _collision.AverageNormal);

            GroundAccelerate();
            ApplyFriction(_friction);
        }
        else if(!_collision.OnGround)
        {
            ApplyGravity();
            AirAccelerate();
        }

        // Apply changes
        _rb.velocity = _vel;
    }
    #endregion

    #region Acceleration
    private void GroundAccelerate()
    {
        float speedMag = Vector3.Dot(_vel, _inputDir);
        Accelerate(_inputDir, speedMag, _groundBaseLimit, _groundAcceleration);

        if (_clampGroundSpeed)
            ClampVel(_groundBaseLimit);
    }

    private void AirAccelerate()
    {
        Vector3 hVel = _vel;
        hVel.y = 0;

        float speedMag = Vector3.Dot(hVel, _inputDir);
        Accelerate(_inputDir, speedMag, _airBaseLimit, _airAcceleration);
    }

    private void Accelerate(Vector3 direction, float magnitude, float accelLimit, float accelerationType)
    {
        float addSpeed = accelLimit - magnitude;

        if (addSpeed <= 0)
            return;

        float accelSpeed = accelerationType * Time.deltaTime;
        
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        _vel += accelSpeed * direction;
    }
    #endregion

    #region Forces
    private void ApplyFriction(float friction)
    {
        _vel *= Mathf.Clamp01(1 - Time.deltaTime * friction);
    }
    
    private void ApplyGravity()
    {
        _vel.y -= _gravity * Time.deltaTime;
    }

    private void ClampVel(float limit)
    {
        if (_vel.magnitude > limit)
            _vel = _vel.normalized * limit;
    }
    #endregion
    
    #region Mechanics
    #region Jump
    private void Jump()
    {
        if (!_ableToJump)
            return;

        // Could experiment with additive jump moving down
        if (_vel.y < 0f || !_additiveJump)
            _vel.y = 0f;

        _vel.y += _jumpHeight;
        _collision.OnGround = false;

        if (!_autoJump)
            _customInput._jumpPending = false;

        StartCoroutine(JumpTimer());
    }
    private IEnumerator JumpTimer()
    {
        _ableToJump = false;
        yield return new WaitForSeconds(0.1f);
        _ableToJump = true;
    }
    #endregion

    #region Duck
    // private void Duck()
    // {
    //     if(Ducking && _collision.OnGround)
    //         ClampVel(_duckBaseLimit);
        
    //     if(!duringCrouch)
    //         StartCoroutine(ScaleCollider());

    //     // transform.Translate(Vector3.down * (3 * Time.deltaTime), Space.Self);
    // }

    // // TODO: Move to CylinderCollider
    // private IEnumerator ScaleCollider()
    // {
    //     if(!_customInput.IsDucking && _collision.SphereCastHead())
    //         yield break;

    //     duringCrouch = true;

    //     float t = 0;
    //     float totalTime = 0.2f;
    //     float targetHeight = _customInput.IsDucking ? _duckColliderHeight : _standColliderHeight;
    //     float height = _collision.Cylinder.transform.localScale.y;

    //     while (height != targetHeight)
    //     {
    //         height = Mathf.Lerp(height, targetHeight, t/totalTime);
    //         _collision.SetHeight(height);
            
    //         t += Time.deltaTime;
    //         yield return new WaitForEndOfFrame();
    //     }

    //     _collision.SetHeight(targetHeight);
    //     duringCrouch = false;
    // }
    #endregion
    #endregion
}

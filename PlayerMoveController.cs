using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    private CharacterController _characterController;
    private Animator _animator;
    public Transform playerCamera;
    public Transform standingHeightMarker;

    [Tooltip("Base walking speed")]
    public float walkSpeedPlayer = 6f;

    [Tooltip("Running boy speed")]
    public float runSpeedPlayer = 9f;

    public float jumpHeight = 1f;
    public float gravity = -9.81f;

    public float mouseSensitivity = 2f;

    public float maxLookUpAngle = 90f;

    public float maxLookDownAngle = -90f;

    [Header("CAMERA EFFECTS")]
    public bool enableCameraTilt = true;
    [Range(0f, 10f)]
    [SerializeField] private float _tiltAmount = 2f;
    [Range(1f, 20f)]
    [SerializeField] private float _tiltSmoothness = 8f;
    [Range(0f, 2f)]
    [SerializeField] private float _runTiltMultiplier = 1.2f;
    [Range(0f, 1f)]
    [SerializeField] private float _crouchTiltMultiplier = 0.5f;

    [Header("RUN + FOV SETTINGS")]
    public bool enableRunFov = true;
    public float normalFov = 60f;
    public float runFov = 70f;
    [Range(1f, 20f)]
    [SerializeField] private float _fovChangeSpeed = 8f;

    private const float _ungroundedDuration = 0.2f;


    [Header("CAMERA SETTINGS")]
    public bool enableHeadPlayer = true;
    [SerializeField] private float _walkHeadSpeed = 14f;
    private float _walkHeadAmount = 0.05f;
    private float _runHeadSpeed = 18f;
    private float _runHeadAmount = 0.03f;
    private float _headHeight;

    private LayerMask _obstacleLayerMask = ~0;
    private float _originalHeight;
    public KeyCode runKey = KeyCode.LeftShift;

    public KeyCode crouchKey = KeyCode.LeftControl;

    private float _currentMovementSpeed;

    private float _crouchHeight = 1.3f;
    private float _targetHeight;
    private float _crouchSmoothTime = 0.2f;
    private float _currentTilt;

    private bool _isGrounded;
    private bool _wasGrounded;
    private float _lastGroundedTime;
    private bool _isCrouching;
    private bool _wasRunningWhenJumped;
    private float _markerHeightOffset;
    private bool _markerInitialized;
    private Vector3 _moveVector;
    private Vector3 _velocity;

    private float _prevHeight;
    private float _newHeight;
    private float _heightDiff;
    private float _currentHeightVelocity;

    private float _cameraHeight;
    private float _newCameraHeight;
    private float _cameraBaseHeight;
    private float _defaultYPos;
    private float _cameraHeightVelocity;
    private float _targetFov;
    private float _currentFov;
    private bool _runningFov;
    private float _xRotation;
    private float _timer;
    private float _horInput;
    private float _verInput;

    private bool _isMoving;

    private bool _isRunning;

    private bool _hasMoveInput;
    private bool _isMovingNow;

    public enum MovementState
    {
        Walking,
        Running,
        Crouching,
        Jumping
    }
    private MovementState _currentMovementState = MovementState.Walking;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        InitializeController();
    }

    private void InitializeController()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _originalHeight = _characterController.height;
        _targetHeight = _originalHeight;
        _defaultYPos = playerCamera.localPosition.y;
        _cameraBaseHeight = _defaultYPos;

        if (playerCamera.GetComponent<Camera>() != null)
        {
            _currentFov = _targetFov = normalFov;
            playerCamera.GetComponent<Camera>().fieldOfView = _currentFov;
            Debug.Log("Marker initialized");
        }

        if (standingHeightMarker != null)
        {
            _markerHeightOffset = standingHeightMarker.transform.position.y - transform.position.y;
            _markerInitialized = true;
            Debug.Log("Marker initialized");
        }

        _currentMovementSpeed = walkSpeedPlayer;
    }

    private void Update()
    {   
        if (Managers.Player.PlayerAlive && !Managers.Game.onCutScene) 
        {
            _wasGrounded = _isGrounded;
            _isGrounded = _characterController.isGrounded;

            // update grounded state
            if (_isGrounded) _lastGroundedTime = Time.time;
            if (!_wasGrounded && _isGrounded) Land();

            // reset vertical velocity when grounded
            if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;

            PlayerCrouching();
            UpdateMovementState();
            PlayerMovement();
            PlayerHeightAdjust();
            PlayerCameraControl();
            PlayerCameraTilt(); // naclon
            PlayerFovChange();
            PlayerAnimations();

            if (enableHeadPlayer) PlayerMoveHead();
        }
    }

    private void PlayerMovement()
    {
        // movement input
        _horInput = Input.GetAxis("Horizontal");
        _verInput = Input.GetAxis("Vertical");

        _isMoving = Mathf.Abs(_horInput) > 0.01f || Mathf.Abs(_verInput) > 0.01f;
        _moveVector = _horInput * transform.right + _verInput * transform.forward;

        if (_moveVector.magnitude > 1f)
        {
            _moveVector.Normalize();
        }

        if (Input.GetButtonDown("Jump") && IsEffectivelyGrounded() && !_isCrouching)
        {
            _wasRunningWhenJumped = _currentMovementState == MovementState.Running;
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _currentMovementState = MovementState.Jumping;
        }

        UpdateMarkerPosition();
        // apply movement
        _characterController.Move(_moveVector * _currentMovementSpeed * Time.deltaTime);

        // apply gravity
        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void UpdateMarkerPosition()
        {
            if (standingHeightMarker != null && _markerInitialized)
            {
                standingHeightMarker.transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y + _markerHeightOffset,
                    transform.position.z
                );
                standingHeightMarker.transform.rotation = Quaternion.identity;
            }
        }

    private void PlayerCrouching()
    {
        // Crouch key pressed

        if (Input.GetKey(crouchKey))
        {
            if (!_isCrouching)
            {
                _isCrouching = true;
                _targetHeight = _crouchHeight;
            }
        }
        else
        {
            if (_isCrouching && CanStandUp())
            {
                _isCrouching = false;
                _targetHeight = _originalHeight;
            }
        }
    }

    private void  PlayerAnimations()
    {
        _hasMoveInput = (_horInput !=0 || _verInput !=0);
        _isMovingNow = _isMoving || _hasMoveInput;

        if (_currentMovementState == MovementState.Jumping)
        {
            _animator.SetBool("Jumping", true);
        }
        else
        {
            _animator.SetBool("Jumping", false);
        }

        if (_isMovingNow)
        {
            _animator.SetBool("Crouch", false);

            if (_isCrouching)
            {
                _animator.SetBool("CrouchWalk", true);
                _animator.SetBool("Walk", false);
                _animator.SetBool("Run", false);
                _animator.SetBool("Jumping", false);
            }
            else
            {
                _animator.SetBool("CrouchWalk", false);
                _animator.SetBool("Walk", !_isRunning);
                _animator.SetBool("Run", _isRunning);
                _animator.SetBool("Jumping", false);
            }

            return; // slip idle
        }

        if (_isCrouching) // when no moving
        {
            _animator.SetBool("Crouch", true);
            _animator.SetBool("CrouchWalk", false);
            _animator.SetBool("Walk", false);
            _animator.SetBool("Run", false);
            _animator.SetBool("Jumping", false);
        }
        else
        {
            _animator.SetBool("Crouch", false);
            _animator.SetBool("Walk", false);
            _animator.SetBool("Run", false);
            _animator.SetBool("CrouchWalk", false);
            _animator.SetBool("Jumping", false);
        }
    }
    
    private bool CanStandUp()
    {
        Vector3 bottom = transform.position + Vector3.up * _characterController.radius;
        Vector3 top = bottom + Vector3.up * (_originalHeight - _characterController.radius * 2f);

        return !Physics.CheckCapsule(
            bottom,
            top,
            _characterController.radius,
            _obstacleLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void UpdateMovementState()
    {
        if (!IsEffectivelyGrounded())
        {
            _currentMovementState = MovementState.Jumping;
            return;
        }
        if (_isCrouching)
        {
            _currentMovementState = MovementState.Crouching;
            _currentMovementSpeed = walkSpeedPlayer * 0.5f;
        }
        else
        {
            bool wantsToRun = Input.GetKey(runKey) && Input.GetAxis("Vertical") > 0.1f;
            _currentMovementState = wantsToRun ? MovementState.Running : MovementState.Walking;
            _currentMovementSpeed = wantsToRun ? runSpeedPlayer : walkSpeedPlayer;
        }
    }
    private void Land()
    {
        _currentMovementState = _isCrouching ? MovementState.Crouching : MovementState.Walking;
        _wasRunningWhenJumped = false;
    }

    private bool IsEffectivelyGrounded()
    {
        return _isGrounded || (Time.time - _lastGroundedTime <= _ungroundedDuration && _velocity.y <= 0);
    }

    private void PlayerHeightAdjust()
    {
        _prevHeight = _characterController.height;
        _newHeight = Mathf.SmoothDamp(_characterController.height, _targetHeight, ref _currentHeightVelocity, _crouchSmoothTime);
        _heightDiff = _newHeight - _prevHeight;
        _characterController.height = _newHeight;

        // adjst position to prevent down into ground
        if (_heightDiff > 0) _characterController.Move(Vector3.up * _heightDiff * 0.5f);

        AdjustCameraPosition();

    }

    private void AdjustCameraPosition()
    {
        _cameraHeight = _cameraBaseHeight * (_characterController.height / _originalHeight);
        _newCameraHeight = Mathf.SmoothDamp(playerCamera.localPosition.y, _cameraHeight, ref _cameraHeightVelocity, _crouchSmoothTime);
        playerCamera.localPosition = new Vector3(
            playerCamera.localPosition.x,
            _newCameraHeight,
            playerCamera.localPosition.z
        );
    }

    private void PlayerCameraControl()
    {
        transform.Rotate(0f, Input.GetAxis("Mouse X") * mouseSensitivity, 0f);

        _xRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation, maxLookDownAngle, maxLookUpAngle);
        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);


    }

    private void PlayerCameraTilt() // naclon
    {
        if (!enableCameraTilt || !IsEffectivelyGrounded())
        {
            _currentTilt = Mathf.Lerp(_currentTilt, 0f, _tiltSmoothness * Time.deltaTime);
            ApplyCameraTilt();
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
        {
            float targetTilt = -moveX * _tiltAmount;

            // Add slight tilt based on mouse movement when moving forward/backward
            if (Mathf.Abs(moveZ) > 0.1f && Mathf.Abs(moveX) < 0.1f)
                targetTilt = -Input.GetAxis("Mouse X") * _tiltAmount * 0.5f;

            // Apply modifiers
            if (_currentMovementState == MovementState.Running) targetTilt *= _runTiltMultiplier;
            if (_isCrouching) targetTilt *= _crouchTiltMultiplier;

            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, _tiltSmoothness * Time.deltaTime);
        }
        else
        {
            _currentTilt = Mathf.Lerp(_currentTilt, 0f, _tiltSmoothness * Time.deltaTime);
        }

        ApplyCameraTilt();
    }

    private void ApplyCameraTilt()
    {
        if (playerCamera == null) return;
        Vector3 rotate = playerCamera.localEulerAngles;
        playerCamera.localEulerAngles = new Vector3(rotate.x, rotate.y, _currentTilt);
    }

    private void PlayerFovChange()
    {
        if (!enableRunFov || playerCamera == null) return;

        Camera cameraPlayer = playerCamera.GetComponent<Camera>();
        _runningFov = IsEffectivelyGrounded() ? _currentMovementState == MovementState.Running : _wasRunningWhenJumped;
        _targetFov = _runningFov ? runFov : normalFov;

        // Slower FOV transition in air
        float fovMultiplier = IsEffectivelyGrounded() ? _fovChangeSpeed : _fovChangeSpeed * 0.5f;
        _currentFov = Mathf.Lerp(_currentFov, _targetFov, fovMultiplier * Time.deltaTime);
        cameraPlayer.fieldOfView = _currentFov;
    }

    private void PlayerMoveHead()
        {
            if (!IsEffectivelyGrounded())
            {
                // Smoothly return camera to base height when in air
                _headHeight = _cameraBaseHeight * (_characterController.height / _originalHeight);
                playerCamera.localPosition = new Vector3(
                    playerCamera.localPosition.x,
                    Mathf.Lerp(playerCamera.localPosition.y, _headHeight, Time.deltaTime * 12f),
                    playerCamera.localPosition.z
                );
                _timer = 0;
                return;
            }

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            float headBaseHeight = _cameraBaseHeight * (_characterController.height / _originalHeight);

            // Apply head bob when moving
            if (Mathf.Abs(moveX) > 0.15f || Mathf.Abs(moveZ) > 0.15f)
            {
                _isRunning = Input.GetKey(runKey) && !_isCrouching && moveZ > 0.1f;

                // Adjust parameters based on state
                float speedMult = _isCrouching ? 0.6f : 1f;
                float amountMult = _isCrouching ? 0.4f : 1f;

                float bobSpeed = (_isRunning ? _runHeadSpeed : _walkHeadSpeed) * speedMult;
                float bobAmount = (_isRunning ? _runHeadAmount : _walkHeadAmount) * amountMult;

                _timer += Time.deltaTime * bobSpeed;
                playerCamera.localPosition = new Vector3(
                    playerCamera.localPosition.x,
                    headBaseHeight + Mathf.Sin(_timer) * bobAmount,
                    playerCamera.localPosition.z
                );
            }
            else
            {
                // Smoothly return to base height when not moving
                _timer = 0;
                playerCamera.localPosition = new Vector3(
                    playerCamera.localPosition.x,
                    Mathf.Lerp(playerCamera.localPosition.y, headBaseHeight, Time.deltaTime * 8f),
                    playerCamera.localPosition.z
                );
            }
        }
}
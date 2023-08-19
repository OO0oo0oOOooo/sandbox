using System;
using UnityEngine;

public class CustomInput : MonoBehaviour
{
    [SerializeField] private string _xAxisInput = "Horizontal";
    [SerializeField] private string _yAxisInput = "Vertical";
    [SerializeField] private string _inputMouseX = "Mouse X";
    [SerializeField] private string _inputMouseY = "Mouse Y";

    [SerializeField] private string _jumpButton = "Jump";
    // [SerializeField] private string _lCtrl = "Left Ctrl";
    // [SerializeField] private string _lShift = "Left Shift";
    // [SerializeField] private string _escButton = "Escape";
    // [SerializeField] private string _tabButton = "Tab";

    [SerializeField] private Camera _camera;
    private Transform _transform;
    private Movement _movement;
    // private Player _player;


    [SerializeField] private float _mouseSensitivity = 1f;
    [SerializeField] private float _slideThreshold = 8f;

    
    // jump
    public bool _jumpPending = false;
    public bool _slideJumpPending = false;


    // UI Bools
    private bool _tab = false;
    private bool _esc = false;

    public bool TAB { get => _tab; }
    public bool ESC { get => _esc; }


    // Mouse Accessor
    private bool _attackPending = false;
    private bool _attack2Pending = false;
    public bool AttackPending { get => _attackPending; }
    public bool Attack2Pending { get => _attack2Pending; }


    // Bool Accessors
    private bool _isDucking = false;
    private bool _isSliding = false;
    public bool IsDucking { get => _isDucking; }
    public bool IsSliding { get => _isSliding; }


    // Input Accessors
    private Vector3 _inputRaw;
    private Vector3 _input;
    public Vector3 InputAxis { get => _input; }
    public Vector3 InputAxisRaw { get => _inputRaw; }


    // X and Z input
    public Vector3 InputRot;
    // public Vector3 InputRot { get => _inputRot; }
    private Vector3 _inputDir;
    public Vector3 InputDir { get => _inputDir; }


    // Events
    public static event Action<bool> OnTabPressed;
    public static event Action<bool> OnEscPressed;


   


    void Awake()
    {
        _transform = transform;
        // _player = GetComponent<Player>();
        _movement = GetComponent<Movement>();
    }

    private void Update()
    {
        MouseLook();
        MouseInput();

        MovementInput();

        UIInput();
    }

    private void MovementInput()
    {
        float xAxisRaw;
        float zAxisRaw;
        float xAxis;
        float zAxis;

        xAxisRaw = Input.GetAxisRaw(_xAxisInput);
        zAxisRaw = Input.GetAxisRaw(_yAxisInput);
        _inputRaw = new Vector3(xAxisRaw, 0, zAxisRaw);

        xAxis = Input.GetAxis(_xAxisInput);
        zAxis = Input.GetAxis(_yAxisInput);
        _input = new Vector3(xAxis, 0, zAxis);

        _inputDir = _transform.rotation * new Vector3(xAxisRaw, 0f, zAxisRaw).normalized;

        // Jump
        if (Input.GetButtonDown(_jumpButton))
            _jumpPending = true;

        if (Input.GetButtonUp(_jumpButton))
            _jumpPending = false;

        // Duck
        if (Input.GetKeyDown(KeyCode.LeftShift))
            _isDucking = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            _isDucking = false;

        // Slide
        if (_isDucking && _slideThreshold < _movement.CurrentSpeed)
            _isSliding = true;
        else
            _isSliding = false;
        
        // Slide Jump Input
        if(_isSliding && Input.GetButtonDown(_jumpButton))
            _slideJumpPending = true;

        if(_isSliding && Input.GetButtonUp(_jumpButton))
            _slideJumpPending = false;
    }
    
    private void MouseLook()
    {
        InputRot.y += Input.GetAxisRaw(_inputMouseX) * _mouseSensitivity;
        InputRot.x -= Input.GetAxisRaw(_inputMouseY) * _mouseSensitivity;

        // clamp
        if (InputRot.x > 90f)
            InputRot.x = 90f;
        if (InputRot.x < -90f)
            InputRot.x = -90f;
    }

    private void MouseInput()
    {
        if(Input.GetMouseButtonDown(0))
            _attackPending = true;

        if(Input.GetMouseButtonUp(0))
            _attackPending = false;


        if(Input.GetMouseButtonDown(1))
            _attack2Pending = true;

        if(Input.GetMouseButtonUp(1))
            _attack2Pending = false;
    }
    private void UIInput()
    {
        // TAB
        if (Input.GetKeyDown(KeyCode.Tab))
            OnTabPressed?.Invoke(true);
            
        if (Input.GetKeyUp(KeyCode.Tab))
            OnTabPressed?.Invoke(false);

        // ESC Toggle
        // if (Input.GetButtonDown(escButton))
        //     if(!esc)
        //         esc = true;
        //     else if(esc)
        //         esc = false;

        //     OnEscPressed?.Invoke(esc);

        if (Input.GetKeyDown(KeyCode.Tilde))
        {
            if(!_esc)
            {
                _esc = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if(_esc)
            {
                _esc = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            OnEscPressed?.Invoke(_esc);
        }
    }

    
}

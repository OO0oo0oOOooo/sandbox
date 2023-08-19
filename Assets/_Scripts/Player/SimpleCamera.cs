using UnityEngine;

//TODO CAMERA: refactor all this it is not good

public class SimpleCamera : MonoBehaviour
{
    private Transform _transform;
    private Transform _playerTransform;

    private Camera _cam;
    private CustomInput _customInput;

    private Quaternion oldRot;

    private bool _lockCamera = false;

    [SerializeField] private float turnSpeed = 100f;

    private void Awake()
    {
        _transform = transform;
        _playerTransform = transform.parent.transform;

        _cam = GetComponent<Camera>();
        _customInput = GetComponentInParent<CustomInput>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        oldRot = _transform.rotation;
    }

    private void FixedUpdate()
    {
        if(!_lockCamera)
            FirstPersonCamera();
    }

    private void FirstPersonCamera()
    {
        _transform.rotation = Quaternion.Lerp(oldRot, Quaternion.Euler(_customInput.InputRot), turnSpeed * Time.deltaTime);
        _playerTransform.rotation = Quaternion.Euler(0f, _customInput.InputRot.y, 0f);

        oldRot = _transform.rotation;
    }

    Vector3 tempRot;
    bool savedRot = false;
    // This works but i could also just use a bool in CustomInput
    public void LockCamera(bool lockState)
    {
        _lockCamera = lockState;

        if(lockState && !savedRot)
        {
            savedRot = true;
            tempRot = _customInput.InputRot;
            _customInput.InputRot = Vector3.zero;
        }

        if(!lockState && savedRot)
        {
            _customInput.InputRot = tempRot;
            savedRot = false;
        }
    }
}


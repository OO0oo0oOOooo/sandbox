using UnityEngine;

public class LightTool : MonoBehaviour
{
    ToolSelect _toolSelect;
    CustomInput _customInput;
    Transform _camTransform;

    [SerializeField] private Rigidbody _light;

    private bool _hasThrown = false;

    [SerializeField] private float _throwStrength = 10;

    // Start is called before the first frame update
    void Start()
    {
        _toolSelect = GetComponent<ToolSelect>();
        _customInput = GetComponent<CustomInput>();
        _camTransform = GetComponentInChildren<SimpleCamera>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(_toolSelect._tool != ToolSelect.Tool.Light) return;

        ThrowLight();
    }

    private void ThrowLight()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Rigidbody clone = Instantiate(_light, _camTransform.position, Quaternion.identity);
            clone.AddForce(_camTransform.forward * _throwStrength, ForceMode.Impulse);
            Destroy(clone, 30);
        }
    }
}

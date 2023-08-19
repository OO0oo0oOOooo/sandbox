using UnityEngine;

public class Dig : MonoBehaviour
{
    [SerializeField] private World _world;
    [SerializeField] private GameObject _brushHighlight;

    CustomInput _customInput;
    Camera _cam;
    ToolSelect _toolSelect;

    [SerializeField] private float _radius = 2;
    [SerializeField] private float _maxDistance = 10;

    // Start is called before the first frame update
    void Start()
    {
        _customInput = GetComponent<CustomInput>();
        _toolSelect = GetComponent<ToolSelect>();
        _cam = GetComponentInChildren<Camera>();

        _brushHighlight.transform.localScale = Vector3.one * (_radius * 2);
    }

    // Update is called once per frame
    void Update()
    {
        if(_toolSelect._tool != ToolSelect.Tool.Dig)
        {
            EnablebrushHighlight(false);
            return;
        }

        TerrainEdit();
    }

    private void TerrainEdit()
    {
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _maxDistance))
        {
            _brushHighlight.transform.position = hit.point;
            EnablebrushHighlight(true);

            if(_customInput.AttackPending)
                if (hit.transform.tag == "Terrain")
                    _world.EditTerrainSphere(hit, _radius, 0.25f);
                
            if(_customInput.Attack2Pending)
                if (hit.transform.tag == "Terrain")
                    _world.EditTerrainSphere(hit, _radius, -0.25f);
        }
        else
            EnablebrushHighlight(false);
    }

    bool _lastB = false;
    private void EnablebrushHighlight(bool b)
    {
        if(b != _lastB)
        {
            _brushHighlight.SetActive(b);
            _lastB = b;
        }
    }
}

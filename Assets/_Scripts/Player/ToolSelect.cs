using System.Collections;
using UnityEngine;

public class ToolSelect : MonoBehaviour
{
    CustomInput _customInput;
    SimpleCamera _cam;

    [SerializeField] private GameObject _wheelPanel;
    [SerializeField] private GameObject[] _wheelItem;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private GameObject _rotator;

    [SerializeField] private int _clampRange = 15;
    [SerializeField] private float offset = 0;

    public Tool _tool;
    public enum Tool
    {
        Dig,
        Light,
        Add,
        Add1,
        Add2
    }

    // Start is called before the first frame update
    void Start()
    {
        _customInput = GetComponent<CustomInput>();
        _cam = GetComponentInChildren<SimpleCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
            print("1");

        if(Input.GetKeyDown(KeyCode.Alpha2))
            print("2");

        if(Input.GetKey(KeyCode.E))
            SelectionWheel();

        if(Input.GetKeyUp(KeyCode.E))
        {
            _cam.LockCamera(false);
            _wheelPanel.SetActive(false);
            _crosshair.transform.localPosition = Vector3.zero;
            _tool = (Tool)_selection;
        }
    }

    private int _selection = 0;
    private int lastSelection;

    private Vector2 crosshairPosition;

    private void SelectionWheel()
    {
        if(!_wheelPanel.activeSelf)
            _wheelPanel.SetActive(true);

        _cam.LockCamera(true);

        crosshairPosition.x += Input.GetAxisRaw("Mouse X");
        crosshairPosition.y += Input.GetAxisRaw("Mouse Y");

        crosshairPosition = Vector3.ClampMagnitude(crosshairPosition, _clampRange);
        _crosshair.transform.localPosition = crosshairPosition * 10;

        // Deadzone
        if(crosshairPosition.magnitude < 8)
        {
            if(lastSelection != -1)
                HighlightSelection(lastSelection, Vector3.one);
            
            lastSelection = -1;
            _selection = -1;
            return;
        }

        float angleDeg = FindAngleFromPos(crosshairPosition, Vector2.zero, offset);
        _selection = DirectionIndex(angleDeg, 5);

        if(_selection != lastSelection)
        {
            if(lastSelection != -1)
                HighlightSelection(lastSelection, Vector3.one);
            
            HighlightSelection(_selection, Vector3.one * 1.2f);
            lastSelection = _selection;
        }
    }

    private float FindAngleFromPosNormalized(Vector2 pos, Vector2 center, float offset)
    {
        Vector3 a = (pos - center).normalized;

        float angDeg = Mathf.Atan2(a.y, a.x)*Mathf.Rad2Deg;
        angDeg = ((angDeg+360)-offset)%360;
        return angDeg;
    }

    private float FindAngleFromPos(Vector2 pos, Vector2 center, float offset)
    {
        float angDeg = Mathf.Atan2(pos.y - center.y, pos.x - center.x) * Mathf.Rad2Deg;
        angDeg = ((angDeg+360)-offset)%360;
        return angDeg;
    }

    private int DirectionIndex(float ang, int numSides)
    {
        float anglePerItem = 360f / numSides;
        return (int)(ang/anglePerItem);
    }

    private void HighlightSelection(int _selection, Vector3 target)
    {
        StartCoroutine(LerpScale(_selection, target));
    }

    private IEnumerator LerpScale(int _selection, Vector3 targetHeight)
    {
        float t = 0;
        float totalTime = 0.2f;
        Vector3 height = _wheelItem[_selection].transform.localScale;

        while (height != targetHeight)
        {
            height = Vector3.Lerp(height, targetHeight, t/totalTime);
            _wheelItem[_selection].transform.localScale = height;
            
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        height = Vector3.Lerp(height, targetHeight, t/totalTime);
    }
}

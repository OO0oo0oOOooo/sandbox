using UnityEngine;

public class SimpleCollider : MonoBehaviour
{
    private Transform _transform;
    public LayerMask LayerMask;

    public bool OnGround;
    public Vector3 AdverageNormal;
    public Vector3 AdveragePoint;
    public float AdverageDistance;

    [SerializeField] private Vector3 _raycastStartOffset = Vector3.up * -0.1f;
    [SerializeField] private float _raycastDistance = 1f;

    [SerializeField] private float radius = 0.5f;
    [SerializeField] private int totalRays = 6;
    [SerializeField] private int numLayers = 2;

    private void Awake()
    {
        _transform = transform;
    }

    private void Update()
    {
        GetNormal();
    }

    private void GetNormal()
    {
        Vector3 advNormal = Vector3.zero;
        Vector3 advPoint = Vector3.zero;
        float advDistance = 0f;

        int hitCount = 0;

        int remainingRays = totalRays;
        for (int layer = 0; layer < numLayers; layer++)
        {
            int numRaysInLayer;
            if(layer == 0)
                numRaysInLayer = 1;
            else
                numRaysInLayer = Mathf.CeilToInt((float)remainingRays / (numLayers - layer));


            remainingRays -= numRaysInLayer;

            // Calculate the radius of the current layer
            float currentRadius = radius * layer / (numLayers - 1);

            // Perform raycasts from points on the current layer
            for (int i = 0; i < numRaysInLayer; i++)
            {
                float angle = i * 2 * Mathf.PI / numRaysInLayer;
                float x = currentRadius * Mathf.Cos(angle);
                float z = currentRadius * Mathf.Sin(angle);

                Vector3 point = new Vector3(x, 0, z);
                point = _transform.rotation * point;

                Ray ray = new Ray((_transform.position + _raycastStartOffset) + point, point + -_transform.up);

                Debug.DrawRay(ray.origin, ray.direction * _raycastDistance, Color.red);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 1.5f, LayerMask))
                {
                    OnGround = true;
                    advNormal += hitInfo.normal;
                    advPoint += hitInfo.point;
                    advDistance += hitInfo.distance;
                    hitCount++;
                }
            }
        }

        if(advNormal == Vector3.zero)
        {
            // Debug.Log("No Ground");
            OnGround = false;
            AdverageNormal = transform.up;
            AdveragePoint = Vector3.zero;
            AdverageDistance = 1.2f;
            return;
        }

        AdverageNormal = advNormal.normalized;
        AdveragePoint = advPoint / hitCount;
        AdverageDistance = advDistance / hitCount;
    }
}
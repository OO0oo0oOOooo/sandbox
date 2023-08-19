using UnityEngine;

public class SimpleCollider : MonoBehaviour
{
    public bool OnGround;
    public Vector3 ContactNormal;
    public LayerMask LayerMask;

    [SerializeField] private Vector3 _raycastOffset = Vector3.up * 0.25f;

    private void Update()
    {
        Debug.DrawRay(transform.position + _raycastOffset, Vector3.down, Color.red);
        if(Physics.Raycast(transform.position + _raycastOffset, Vector3.down, out RaycastHit hit, 1f, LayerMask))
        {
            OnGround = true;
            ContactNormal = hit.normal;
        }
        else
        {
            OnGround = false;
            ContactNormal = Vector3.up;
        }

    }

    // [SerializeField] private float _slopeLimit = 45f;
    // private void OnCollisionStay(Collision other)
    // {
    //     foreach (ContactPoint contact in other.contacts)
    //     {
    //         if (contact.normal.y > Mathf.Sin(_slopeLimit * Mathf.Deg2Rad + Mathf.PI / 2f))
    //         {
    //             ContactNormal = contact.normal;
    //             OnGrounded = true;
    //             return;
    //         }
    //     }
    // }
}

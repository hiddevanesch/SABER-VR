using UnityEngine;
using UnityEngine.InputSystem;

public class WheelInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty rotationAxis;

    [Header("Ray Settings")]
    [SerializeField] private float maxRayDistance = 5f;

    private RemoteWheelObject selectedRemoteGraphWheel;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRayDistance, layerMask))
        {
            OnHit(hit);
        }
        else
        {
            selectedRemoteGraphWheel = null;
        }
    }

    private void OnHit(RaycastHit hit)
    {
        RemoteWheelObject newRemoteGraphWheel = hit.collider.GetComponent<RemoteWheelObject>();

        if (newRemoteGraphWheel != selectedRemoteGraphWheel)
        {
            selectedRemoteGraphWheel = newRemoteGraphWheel;
        }

        float value = rotationAxis.action.ReadValue<Vector2>().y;
        selectedRemoteGraphWheel.IncreaseRotation(value);
    }
}

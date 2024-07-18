using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableWheelObject : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    [Header("Wheel Settings")]
    [SerializeField] private float dragOnReleaseFactor = 0.98f;

    [Header("References")]
    [SerializeField] private Transform pivot;

    private float currentAngle = 0.0f;
    private float deltaAngle = 0.0f;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentAngle = GetAngle();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
        {
            if (isSelected)
            {
                float newAngle = GetAngle();

                // Calculate the difference in angle
                deltaAngle = newAngle - currentAngle;

                // Apply the difference in angle to the wheel
                transform.Rotate(Vector3.up, deltaAngle);

                currentAngle = newAngle;
            }
            else
            {
                deltaAngle *= dragOnReleaseFactor;

                transform.Rotate(Vector3.up, deltaAngle);
            }
        }
    }

    private Vector3 FindLocalPoint(Vector3 position)
    {
        return pivot.InverseTransformPoint(position).normalized;
    }

    private float ConvertToAngle(Vector3 direction)
    {
        Vector2 position2D = new Vector2(direction.z, direction.x);
        return Vector2.SignedAngle(Vector2.up, position2D);
    }

    private float GetAngle()
    {
        Vector3 direction = FindLocalPoint(interactorsSelecting[0].transform.position);
        return ConvertToAngle(direction);
    }
}
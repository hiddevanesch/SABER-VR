using UnityEngine;

public class RotateTowardsUser : MonoBehaviour
{
    void Update()
    {
        Vector3 directionFromCamera = transform.position - Camera.main.transform.position;

        float angle = Vector2.SignedAngle(Vector2.up, new Vector2(directionFromCamera.x, directionFromCamera.z));

        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}

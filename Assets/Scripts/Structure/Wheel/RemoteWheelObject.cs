using UnityEngine;

public class RemoteWheelObject : MonoBehaviour
{

    [Header("Wheel Settings")]
    [SerializeField] private float dragOnReleaseFactor = 0.9f;
    [SerializeField] private float accelerationFactor = 1f;
    [SerializeField] private float maxSpeed = 100f;

    private float deltaAngle = 0.0f;

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        deltaAngle *= Mathf.Pow(dragOnReleaseFactor, Time.deltaTime);

        float rotationAmount = deltaAngle * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationAmount);
    }

    public void IncreaseRotation(float value)
    {
        float angle = -value * accelerationFactor;
        float newDeltaAngle = (deltaAngle + angle);

        deltaAngle = Mathf.Clamp(Mathf.Lerp(deltaAngle, newDeltaAngle, Time.deltaTime), -maxSpeed, maxSpeed);

    }

    public void Show()
    {
        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }
}
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RelationLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Settings")]
    [SerializeField] private float thickness = 1f;

    private Transform startPoint;
    private Transform endPoint;
    private Color startColor;
    private Color endColor;

    public void Initialize(Transform startPoint, Transform endPoint, Color startColor)
    {
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        this.startColor = startColor;
    }

    void Start()
    {
        float hue, saturation, value;
        Color.RGBToHSV(startColor, out hue, out saturation, out value);
        //saturation = 1;
        //value = 1;
        startColor = Color.HSVToRGB(hue, saturation, value);
        hue = (hue + 0.5f) % 1;
        endColor = Color.HSVToRGB(hue, saturation, value);

        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(startColor, 0.25f), new GradientColorKey(endColor, 0.75f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
        lineRenderer.colorGradient = colorGradient;
        lineRenderer.startWidth = thickness;
        lineRenderer.endWidth = thickness;
    }

    void Update()
    {
        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, endPoint.position);
    }
}

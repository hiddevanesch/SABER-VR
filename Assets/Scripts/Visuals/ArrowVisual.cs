using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ArrowVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private Transform textPivot;

    [Header("Shader Settings")]
    [SerializeField] private Shader arrowShader;

    [Header("Arrow Settings")]
    [SerializeField] private int tipPoints = 3;
    [SerializeField] private float minWidth = 0.02f;
    [SerializeField] private float baseWidthIncrement = 0.1f;
    [SerializeField] private int curvePoints = 19;
    [SerializeField] private float startingHeightFactor = 0.2f;
    [SerializeField] private float depthHeightFactor = 0.1f;

    private Mesh mesh;
    private Material arrowMaterial;
    private float stemWidth;
    private float tipWidth;
    private Collider _startPointCollider;
    private Collider _endPointCollider;
    private int _depth;

    private bool _biDirectional = false;

    public void Initialize(Collider startPointCollider, Collider endPointCollider, int amount, bool isBiDirectional, int depth, Color startColor, Color endColor)
    {
        _biDirectional = isBiDirectional;
        _startPointCollider = startPointCollider;
        _endPointCollider = endPointCollider;
        _depth = depth;
        stemWidth = minWidth + amount * (baseWidthIncrement / Mathf.Log(amount + 1));
        tipWidth = stemWidth * 3;

        text.text = amount.ToString();

        arrowMaterial = new Material(arrowShader);
        arrowMaterial.SetColor("_startColor", startColor);
        arrowMaterial.SetColor("_endColor", endColor);
        meshRenderer.material = arrowMaterial;
    }

    private void Awake()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    public void Draw()
    {
        Vector3 startPoint = transform.InverseTransformPoint(_startPointCollider.ClosestPointOnBounds(_endPointCollider.transform.position));
        Vector3 endPoint = transform.InverseTransformPoint(_endPointCollider.ClosestPointOnBounds(_startPointCollider.transform.position));

        // Calculate control point
        float distance = Vector3.Distance(startPoint, endPoint);
        Vector3 controlPoint = (startPoint + endPoint) / 2 + Vector3.up * distance * startingHeightFactor + Vector3.up * _depth * depthHeightFactor;
        BezierCurve bezierCurve = new BezierCurve(startPoint, controlPoint, endPoint);

        // Calculate points on the Bezier curve
        Vector3[] curvePointsList = new Vector3[curvePoints];
        for (int i = 0; i < curvePoints; i++)
        {
            float t = i / (float)(curvePoints - 1);
            Vector3 point = CurveUtility.EvaluatePosition(bezierCurve, t);
            curvePointsList[i] = point;
        }

        // Set text to middle of the arrow
        Vector3 textPosition = curvePointsList[(int)Mathf.Ceil(curvePoints / 2f) - 1];
        textPosition.y += 0.001f;

        // Calculate arrow direction and perpendicular vector
        Vector3 arrowDirection = (endPoint - startPoint).normalized;
        Vector3 arrowPerpendicular = Quaternion.Euler(0, 90, 0) * arrowDirection;

        int verticesAmount = 2 * curvePoints + 1;

        // Define vertices
        Vector3[] vertices = new Vector3[verticesAmount];

        int transitionPoint = curvePoints - tipPoints;

        // Stem
        for (int i = 0; i <= transitionPoint; i++)
        {
            vertices[i * 2] = curvePointsList[i] - arrowPerpendicular * (stemWidth / 2);
            vertices[i * 2 + 1] = curvePointsList[i] + arrowPerpendicular * (stemWidth / 2);
        }

        // Tip
        for (int i = 0; i < tipPoints - 1; i++)
        {
            float inverseProgress = 1 - (float)i / (tipPoints - 1);
            int indexShift = transitionPoint + i;
            vertices[(indexShift + 1) * 2] = curvePointsList[indexShift] - arrowPerpendicular * (tipWidth / 2 * inverseProgress);
            vertices[(indexShift + 1) * 2 + 1] = curvePointsList[indexShift] + arrowPerpendicular * (tipWidth / 2 * inverseProgress);
        }

        // Tip of the tip
        vertices[verticesAmount - 1] = curvePointsList[curvePoints - 1];

        // If the arrow is bi-directional, move the vertices to the left (using the perpendicular vector) by half the tipwidth
        // as well as the text position
        if (_biDirectional)
        {
            for (int i = 0; i < verticesAmount; i++)
            {
                vertices[i] -= arrowPerpendicular * (tipWidth / 2);
            }
            textPosition -= arrowPerpendicular * (tipWidth / 2);
        }

        // Define UVs
        Vector2[] uvs = new Vector2[verticesAmount];

        // Define UVs based on the position along arrowDirection
        for (int i = 0; i < verticesAmount; i++)
        {
            float t = Vector3.Dot(vertices[i] - startPoint, arrowDirection) / (endPoint - startPoint).magnitude;
            uvs[i] = new Vector2(0.5f, t);
        }

        // Define triangles
        List<int> trianglesList = new List<int>();
        for (int i = 0; i < curvePoints - 1; i++)
        {
            int i0 = i * 2;
            int i1 = i * 2 + 1;
            int i2 = i * 2 + 2;
            int i3 = i * 2 + 3;

            // Clockwise winding
            trianglesList.Add(i0);
            trianglesList.Add(i1);
            trianglesList.Add(i2);
            trianglesList.Add(i3);
            trianglesList.Add(i2);
            trianglesList.Add(i1);
        }

        trianglesList.Add(verticesAmount - 2);
        trianglesList.Add(verticesAmount - 1);
        trianglesList.Add(verticesAmount - 3);

        // Assign vertices, triangles and UVs to the mesh
        mesh.vertices = vertices;
        mesh.triangles = trianglesList.ToArray();
        mesh.uv = uvs;

        // Recalculate bounds and normals
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Set text position
        textPivot.localPosition = textPosition;
    }
}

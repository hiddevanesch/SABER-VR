using System.Collections.Generic;
using UnityEngine;

public class DiagramCircle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject classDiagramPrefab;

    [Header("Settings")]
    [SerializeField] private float nodeHeight = 1.25f;
    [SerializeField] private float animationSmoothness = 5f;

    public static List<GameObject> activeClassDiagrams = new List<GameObject>();

    void Update()
    {
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        if (activeClassDiagrams.Count == 0)
        {
            return;
        }

        if (activeClassDiagrams.Count <= 2)
        {
            UpdatePositionsForOneOrTwoNodes();
        }
        else
        {
            UpdatePositionsForMultipleNodes();
        }
    }

    private void UpdatePositionsForOneOrTwoNodes()
    {
        // Set the position and rotation for the first node
        UpdateNodePositionAndRotation(activeClassDiagrams[0], new Vector3(0, nodeHeight, 1.5f));

        // Set the position and rotation for the second node
        if (activeClassDiagrams.Count == 2)
        {
            UpdateNodePositionAndRotation(activeClassDiagrams[1], new Vector3(0, nodeHeight, -1.5f));
        }
    }

    private void UpdatePositionsForMultipleNodes()
    {
        float totalWidth = 0f;

        foreach (GameObject node in activeClassDiagrams)
        {
            totalWidth += node.GetComponent<BoxCollider>().size.x;
        }

        // Calculate the radius based on the total width
        float circleRadius = totalWidth / (2 * Mathf.PI);

        float angle = 0;

        for (int i = 0; i < activeClassDiagrams.Count; i++)
        {
            if (i > 0)
            {
                angle += ((activeClassDiagrams[i - 1].GetComponent<BoxCollider>().size.x + activeClassDiagrams[i].GetComponent<BoxCollider>().size.x) / 2) / circleRadius;
            }

            // Calculate the position of the diagram
            float x = circleRadius * Mathf.Sin(angle);
            float z = circleRadius * Mathf.Cos(angle);
            Vector3 targetPosition = new Vector3(x, nodeHeight, z);

            // Update position for the current diagram and rotate it towards the center of the circle
            UpdateNodePositionAndRotation(activeClassDiagrams[i], targetPosition);
        }
    }

    private void UpdateNodePositionAndRotation(GameObject node, Vector3 targetPosition)
    {
        // Smoothly move the node to the target position
        node.transform.localPosition = Vector3.Lerp(node.transform.localPosition, targetPosition, Time.deltaTime * animationSmoothness);

        // Rotate the node to face the center of the circle
        Vector3 lookAtCenter = node.transform.localPosition;
        lookAtCenter.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookAtCenter);

        // Smoothly rotate the node
        node.transform.rotation = Quaternion.Slerp(node.transform.rotation, targetRotation, Time.deltaTime * animationSmoothness);
    }

    public ClassDiagram AddPackage(SoftwareComponent packageComponent, Color color)
    {
        GameObject classDiagramObject = Instantiate(classDiagramPrefab);
        ClassDiagram classDiagramNode = classDiagramObject.GetComponent<ClassDiagram>();
        classDiagramNode.Initialize(packageComponent, color);
        return classDiagramObject.GetComponent<ClassDiagram>();
    }
}

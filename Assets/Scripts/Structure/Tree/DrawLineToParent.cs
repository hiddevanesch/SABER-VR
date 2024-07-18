using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(TreeNode))]
public class DrawLineToParent : MonoBehaviour
{
    private Transform ownPoint;
    private Transform parentPoint;
    [SerializeField] private LineRenderer lineRenderer;

    private void Start()
    {
        ownPoint = GetComponent<TreeNode>().LeftAttachPoint;
        parentPoint = GetComponent<TreeNode>().Parent.RightAttachPoint;

        // Enable line renderer when DrawLineToParent is enabled
        lineRenderer.enabled = true;
    }

    void LateUpdate()
    {
        lineRenderer.SetPosition(0, ownPoint.localPosition);
        lineRenderer.SetPosition(1, transform.InverseTransformPoint(parentPoint.position));
    }
}

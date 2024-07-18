using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(DrawLineToParent))]
public class TreeNode : MonoBehaviour
{
    [SerializeField] private DrawLineToParent drawLineToParent;

    private protected TreeNode _parent;

    [SerializeField] private Transform _leftAttachPoint;
    [SerializeField] private Transform _rightAttachPoint;

    public TreeNode Parent
    {
        get => _parent;
        set => _parent = value;
    }

    public Transform LeftAttachPoint => _leftAttachPoint;

    public Transform RightAttachPoint => _rightAttachPoint;

    protected void DrawParentLine()
    {
        if (Parent != null)
        {
            drawLineToParent.enabled = true;
        }
    }
}

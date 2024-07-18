using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureNode : TreeNode, IOpenable
{
    private const float canvasOffset = 0.0001f;

    [SerializeField] private TextMeshProUGUI packageNameText;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Transform canvas;
    [SerializeField] private GameObject nestIcon;
    [SerializeField] private Material defaultUnlitMaterial;

    [Header("Settings")]
    [SerializeField] private float startingDepth = 0.005f;
    [SerializeField] private float depthPerChild = 0.0002f;

    private SoftwareComponent _packageComponent;
    private List<StructureNode> children = new List<StructureNode>();
    private ClassDiagram classDiagram;
    private Transform childrenWheel;

    public void Initialize(StructureNode parent, SoftwareComponent packageComponent, Color color)
    {
        Parent = parent;

        _packageComponent = packageComponent;

        UpdateText();
        UpdateDepth();
        SetColor(color);

        DrawParentLine();
    }

    public void AddChild(StructureNode child)
    {
        children.Add(child);
    }

    public void SetClassDiagram(ClassDiagram classDiagram)
    {
        this.classDiagram = classDiagram;
    }

    public void SetChildrenWheel(Transform childrenWheel)
    {
        this.childrenWheel = childrenWheel;
    }

    private void UpdateText()
    {
        packageNameText.text = _packageComponent.Name;
    }

    private void UpdateDepth()
    {
        float depth = startingDepth + (depthPerChild * _packageComponent.IndirectChildrenCount);
        meshRenderer.transform.localScale = new Vector3(meshRenderer.transform.localScale.x, meshRenderer.transform.localScale.y, depth);
        canvas.localPosition = new Vector3(0, 0, -((depth / 2) + canvasOffset));
    }

    private void SetColor(Color color)
    {
        Material packageMaterial = new Material(defaultUnlitMaterial);
        packageMaterial.SetColor("_BaseColor", color);

        // Set the material to the mesh
        meshRenderer.material = packageMaterial;

        // Make the text color black if the background is too bright
        if (color.r >= .5f && color.g >= .5f && color.b >= .5f)
        {
            packageNameText.color = Color.black;
            nestIcon.GetComponent<Image>().color = Color.black;
        }
    }

    public void Rotate(Transform structureTreeTransform)
    {
        // Rotate to have the same global rotation as the parent
        transform.rotation = structureTreeTransform.rotation;

        foreach (StructureNode child in children)
        {
            child.Rotate(structureTreeTransform);
        }
    }

    public void EnableNestIcon()
    {
        nestIcon.SetActive(true);
    }

    //############################//
    //                            //
    //         IOpenable          //
    //                            //
    //############################//

    private IOpenable.State _currentState = IOpenable.State.Open;

    public IOpenable.State CurrentState
    {
        get => _currentState;
        set => _currentState = value;
    }

    public void Open()
    {
        packageNameText.fontStyle = FontStyles.Underline;

        if (childrenWheel != null)
        {
            childrenWheel.gameObject.SetActive(true);
        }

        foreach (TreeNode child in children)
        {
            child.gameObject.SetActive(true);
        }

        if (classDiagram != null)
        {
            classDiagram.gameObject.SetActive(true);
        }

        CurrentState = IOpenable.State.Open;
    }

    public void Close()
    {
        packageNameText.fontStyle = FontStyles.Normal;

        foreach (TreeNode child in children)
        {
            if (child is IOpenable childOpenable)
            {
                childOpenable.Close();
            }
            child.gameObject.SetActive(false);
        }

        if (childrenWheel != null)
        {
            childrenWheel.gameObject.SetActive(false);
        }

        if (classDiagram != null)
        {
            classDiagram.gameObject.SetActive(false);
        }

        CurrentState = IOpenable.State.Closed;
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassElement : MonoBehaviour, IOpenable, ISelectable
{
    private const float canvasOffset = 0.01f;

    [Header("References")]
    [SerializeField] private RectTransform body;
    [SerializeField] private RectTransform classNameRect;
    [SerializeField] private TextMeshProUGUI classNameText;
    [SerializeField] private Transform headerMeshTransform;
    [SerializeField] private MeshRenderer headerMeshRenderer;
    [SerializeField] private Transform bodyMeshTransform;
    [SerializeField] private MeshRenderer bodyMeshRenderer;
    [SerializeField] private Transform outlineMeshTransform;
    [SerializeField] private GameObject outlineMeshGameObject;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private VerticalLayoutGroup bodyVerticalLayoutGroup;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private Material defaultUnlitMaterial;

    [Header("Prefabs")]
    [SerializeField] private GameObject methodPrefab;

    [Header("Settings")]
    [SerializeField] private float minimumWidth = 50f;
    [SerializeField] private float headerHeight = 10f;
    [SerializeField] private int bodyPadding = 2;
    [SerializeField] private float bodyColorValueIncrease = 0.1f;
    [SerializeField] private float startingDepth = 5f;
    [SerializeField] private float depthPerChild = .25f;
    [SerializeField] private float outlineSize = 2f;

    private ClassDiagram _classDiagramNodeObject;

    private SoftwareComponent _classComponent;

    private List<MethodUIElement> _methodUIElements = new List<MethodUIElement>();

    private float closedWidth;
    private float openedWidth;

    public IReadOnlyList<MethodUIElement> MethodUIElements
    {
        get => _methodUIElements.AsReadOnly();
    }

    public SoftwareComponent ClassComponent
    {
        get => _classComponent;
    }

    public Vector2 Size
    {
        get => canvas.sizeDelta;
    }

    public ClassDiagram ClassDiagramNodeObject
    {
        get => _classDiagramNodeObject;
    }

    public Color Color
    {
        get => headerMeshRenderer.material.GetColor("_BaseColor");
    }

    private void Awake()
    {
        // Set the padding of the body
        bodyVerticalLayoutGroup.padding.top = bodyPadding;
        bodyVerticalLayoutGroup.padding.bottom = bodyPadding;
        bodyVerticalLayoutGroup.padding.left = bodyPadding;
        bodyVerticalLayoutGroup.padding.right = bodyPadding;

        BehaviorGraph.OnSelectionChanged.AddListener(OnSelectionChanged);
    }

    public void Initialize(SoftwareComponent classComponent, ClassDiagram classDiagramNodeObject, Color color)
    {
        _classComponent = classComponent;
        _classDiagramNodeObject = classDiagramNodeObject;

        foreach (SoftwareComponent methodComponent in _classComponent.Children)
        {
            InstantiateMethod(methodComponent);
        }

        UpdateText();
        UpdateWidth();
        UpdateDepth();
        SetColor(color);

        Close();

        ComponentManager.components.Add(_classComponent.Id, gameObject);
    }

    private void UpdateText()
    {
        classNameText.text = _classComponent.Name;
    }

    private void UpdateWidth()
    {
        float elementWidth = 0;

        // Find the widest element
        classNameText.ForceMeshUpdate();
        elementWidth = Math.Max(elementWidth, classNameText.textBounds.size.x);

        closedWidth = Math.Max(elementWidth, minimumWidth) + bodyPadding * 2;

        foreach (MethodUIElement method in _methodUIElements)
        {
            elementWidth = Math.Max(elementWidth, method.Width);
        }

        openedWidth = Math.Max(elementWidth, minimumWidth) + bodyPadding * 2;
    }

    private void UpdateDepth()
    {
        float depth = startingDepth + (depthPerChild * _classComponent.IndirectChildrenCount);
        headerMeshTransform.localScale = new Vector3(headerMeshTransform.localScale.x, headerMeshTransform.localScale.y, depth);
        bodyMeshTransform.localScale = new Vector3(bodyMeshTransform.localScale.x, bodyMeshTransform.localScale.y, depth);
        outlineMeshTransform.localScale = new Vector3(outlineMeshTransform.localScale.x, outlineMeshTransform.localScale.y, depth);
        boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, depth);
        canvas.localPosition = new Vector3(0, 0, -((depth / 2) + canvasOffset));
    }

    private void InstantiateMethod(SoftwareComponent methodComponent)
    {
        GameObject method = Instantiate(methodPrefab, body);
        MethodUIElement methodUIElement = method.GetComponent<MethodUIElement>();

        // Initialize the method
        methodUIElement.Initialize(methodComponent);

        _methodUIElements.Add(methodUIElement);

        // Set the width of the method to fit within the class
        //RectTransform methodRectTransform = method.GetComponent<RectTransform>();
        //methodRectTransform.sizeDelta = new Vector2(minimumWidth - bodyPadding * 2, methodRectTransform.sizeDelta.y);
    }

    public void UpdateSizeAndPosition()
    {
        // Rebuild the layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(body);
        LayoutRebuilder.ForceRebuildLayoutImmediate(canvas);

        float bodyHeight = Size.y - headerHeight;

        // Set mesh size
        headerMeshTransform.localScale = new Vector3(Size.x, headerHeight, headerMeshTransform.localScale.z);
        bodyMeshTransform.localScale = new Vector3(Size.x, bodyHeight, bodyMeshTransform.localScale.z);
        outlineMeshTransform.localScale = new Vector3(Size.x + outlineSize, Size.y + outlineSize, outlineMeshTransform.localScale.z);

        // Calculate the Y position of the meshes
        float headerY = Size.y / 2 - headerHeight / 2;
        float bodyY = -Size.y / 2 + bodyHeight / 2;

        // Update the position of the meshes
        headerMeshTransform.localPosition = new Vector3(0, headerY, 0);
        bodyMeshTransform.localPosition = new Vector3(0, bodyY, 0);

        // Update the box collider size
        boxCollider.size = new Vector3(Size.x, Size.y, boxCollider.size.z);

        UpdateParentSize();
    }

    public void UpdateParentSize()
    {
        _classDiagramNodeObject.FitChildren();
    }

    private void SetColor(Color color)
    {
        Material packageHeaderMaterial = new Material(defaultUnlitMaterial);
        packageHeaderMaterial.SetColor("_BaseColor", color);

        // Set the material to the header mesh
        headerMeshRenderer.material = packageHeaderMaterial;

        float baseHue, baseSaturation, baseValue;
        Color.RGBToHSV(color, out baseHue, out baseSaturation, out baseValue);

        baseValue = Mathf.Clamp(baseValue + bodyColorValueIncrease, 0, 1);

        Color bodyColor = Color.HSVToRGB(baseHue, baseSaturation, baseValue);

        Material packageBodyMaterial = new Material(defaultUnlitMaterial);
        packageBodyMaterial.SetColor("_BaseColor", bodyColor);

        // Set the material to the body mesh
        bodyMeshRenderer.material = packageBodyMaterial;
    }

    private void OnSelectionChanged()
    {
        outlineMeshGameObject.SetActive(IsSelected);
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
        classNameText.fontStyle = FontStyles.Underline;

        foreach (MethodUIElement method in _methodUIElements)
        {
            method.gameObject.SetActive(true);
        }

        CurrentState = IOpenable.State.Open;

        // Set the width of the class
        canvas.sizeDelta = new Vector2(openedWidth, canvas.sizeDelta.y);
        classNameRect.sizeDelta = new Vector2(openedWidth, classNameRect.sizeDelta.y);
        body.sizeDelta = new Vector2(openedWidth, body.sizeDelta.y);

        UpdateSizeAndPosition();
    }

    public void Close()
    {
        classNameText.fontStyle = FontStyles.Normal;

        foreach (MethodUIElement method in _methodUIElements)
        {
            method.gameObject.SetActive(false);
        }

        CurrentState = IOpenable.State.Closed;

        // Set the width of the class
        canvas.sizeDelta = new Vector2(closedWidth, canvas.sizeDelta.y);
        classNameRect.sizeDelta = new Vector2(closedWidth, classNameRect.sizeDelta.y);
        body.sizeDelta = new Vector2(closedWidth, body.sizeDelta.y);

        UpdateSizeAndPosition();
    }

    //############################//
    //                            //
    //        ISelectable         //
    //                            //
    //############################//

    public bool IsSelected
    {
        get => BehaviorGraph.IsSelected(gameObject);
    }

    public void Select()
    {
        BehaviorGraph.AddToSelection(gameObject);
    }

    public void Deselect()
    {
        BehaviorGraph.RemoveFromSelection(gameObject);
    }
}

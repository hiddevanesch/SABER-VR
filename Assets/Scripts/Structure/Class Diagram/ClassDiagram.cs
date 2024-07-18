using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class ClassDiagram : MonoBehaviour
{
    public enum Layout
    {
        Layered,
        Initial,
    }

    [Header("References")]
    [SerializeField] private GameObject classPrefab;
    [SerializeField] private GameObject dependencyLinePrefab;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private TextMeshProUGUI packageNameText;
    [SerializeField] private RectTransform background;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private InteractableObjectReference interactableObjectReference;

    [Header("Settings")]
    [SerializeField] private float padding = 50f;
    [SerializeField] private Layout layout;

    private Dictionary<string, Node> classes = new Dictionary<string, Node>();
    private List<GameObject> edgeObjects = new List<GameObject>();
    private GeometryGraph graph = new GeometryGraph();

    private Color color;

    private void Awake()
    {
        SettingsMenu.OnRelationTypeChange.AddListener(OnRelationTypeChangedHandler);
    }

    private void OnEnable()
    {
        DiagramCircle.activeClassDiagrams.Add(gameObject);
    }

    private void OnDisable()
    {
        DiagramCircle.activeClassDiagrams.Remove(gameObject);
    }

    private void OnRelationTypeChangedHandler(object notInUseHere)
    {
        RemoveEdges();
        InstantiateRelations();
        UpdateSizes();
        GenerateLayout();
    }

    public void Initialize(SoftwareComponent packageComponent, Color color)
    {
        this.color = color;

        UpdateText(packageComponent);
        SetColor();

        InstantiateClasses(packageComponent);
        InstantiateRelations();

        GenerateLayout();
    }

    private void UpdateText(SoftwareComponent packageComponent)
    {
        packageNameText.text = packageComponent.Id;
    }

    private void SetColor()
    {
        Color opacityColor = color;
        opacityColor.a = 0.025f;
        backgroundImage.color = opacityColor;
    }

    private void GenerateLayout()
    {
        InitializeLayout();
        PositionNodes();

        FitChildren();
    }

    private void InstantiateClasses(SoftwareComponent packageComponent)
    {
        foreach (SoftwareComponent classComponent in packageComponent.Children)
        {
            GameObject classObject = Instantiate(classPrefab, background);
            ClassElement classElement = classObject.GetComponent<ClassElement>();
            classElement.Initialize(classComponent, this, color);

            Vector2 size = classElement.Size;

            Node node = new Node(CurveFactory.CreateRectangle(size.x, size.y, new Point()), classElement);

            graph.Nodes.Add(node);
            classes.Add(classComponent.Id, node);
        }
    }

    private void InstantiateRelations()
    {
        foreach (Node sourceClassNode in graph.Nodes)
        {
            ClassElement sourceClassElement = (ClassElement)sourceClassNode.UserData;
            SoftwareComponent classComponent = sourceClassElement.ClassComponent;

            switch (SettingsMenu.SelectedRelationType)
            {
                case SettingsMenu.ClassDiagramRelationType.Calls:
                    foreach (KeyValuePair<SoftwareComponent, int> invocation in classComponent.Invokes)
                    {
                        if (classes.ContainsKey(invocation.Key.Id))
                        {
                            Node targetClassNode = classes[invocation.Key.Id];
                            ClassElement targetClassElement = (ClassElement)targetClassNode.UserData;

                            graph.Edges.Add(new Edge(sourceClassNode, targetClassNode));

                            InstantiateDependencyLine(sourceClassElement, targetClassElement, color);
                        }
                    }
                    break;
                case SettingsMenu.ClassDiagramRelationType.Specializes:
                    foreach (Class specializesClassComponent in ((Class)classComponent).Specializes)
                    {
                        if (classes.ContainsKey(specializesClassComponent.Id))
                        {
                            Node targetClassNode = classes[specializesClassComponent.Id];
                            ClassElement targetClassElement = (ClassElement)targetClassNode.UserData;

                            graph.Edges.Add(new Edge(sourceClassNode, targetClassNode));

                            InstantiateDependencyLine(sourceClassElement, targetClassElement, color);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void InitializeLayout()
    {
        switch (layout)
        {
            case Layout.Layered:
                GraphUtil.ApplyLayeredLayout(graph);
                break;
            case Layout.Initial:
                GraphUtil.ApplyInitialLayout(graph);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        GraphUtil.CenterGraph(graph);
    }

    private void PositionNodes()
    {
        foreach (Node node in graph.Nodes)
        {
            ((ClassElement)node.UserData).transform.localPosition = GraphUtil.PointToVector2(node.BoundaryCurve.BoundingBox.Center);
        }
    }

    public void FitChildren()
    {
        // Calculate the bounding box that encompasses all class boxes relative to the canvas
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Node classNode in classes.Values)
        {
            ClassElement classRect = (ClassElement)classNode.UserData;

            // Get the position of the child class relative to the canvas
            Vector3 childPosition = canvas.InverseTransformPoint(classRect.transform.position);
            Vector3 childSize = classRect.Size;

            // Encapsulate the position and size of each child class within the bounds
            bounds.Encapsulate(childPosition - childSize / 2f);
            bounds.Encapsulate(childPosition + childSize / 2f);
            bounds.Encapsulate(-childPosition - childSize / 2f);
            bounds.Encapsulate(-childPosition + childSize / 2f);
        }

        // Add padding to the bounding box
        bounds.Expand(padding * 2f);

        // Set the size of the canvas RectTransform to fit all children's positions
        canvas.sizeDelta = bounds.size;

        // Update the Collider size
        _collider.size = new Vector3(bounds.size.x * canvas.localScale.x, bounds.size.y * canvas.localScale.y, 0);
    }

    private void InstantiateDependencyLine(ClassElement classElement, ClassElement targetClass, Color color)
    {
        GameObject relationLineObject = Instantiate(dependencyLinePrefab, background);
        RelationLine relationLine = relationLineObject.GetComponent<RelationLine>();

        relationLine.Initialize(classElement.GetComponent<Transform>(), targetClass.GetComponent<Transform>(), color);

        edgeObjects.Add(relationLineObject);
    }

    private void UpdateSizes()
    {
        foreach (Node node in graph.Nodes)
        {
            ClassElement classElement = (ClassElement)node.UserData;
            node.BoundaryCurve = CurveFactory.CreateRectangle(classElement.Size.x, classElement.Size.y, new Point());
        }
    }

    private void ResetEdges()
    {
        foreach (Edge edge in graph.Edges)
        {
            edge.Curve = CurveFactory.CreateTestShape(0, 0);
        }
    }

    private void RemoveEdges()
    {
        graph.Edges.Clear();

        foreach (GameObject edgeObject in edgeObjects)
        {
            Destroy(edgeObject);
        }
    }

    public void Relayout(Layout layout)
    {
        this.layout = layout;
        UpdateSizes();
        ResetEdges();
        GenerateLayout();
    }

    public Transform Canvas
    {
        get => canvas;
    }
}

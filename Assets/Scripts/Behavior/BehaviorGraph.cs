using Microsoft.Msagl.Core.DataStructures;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Util;

public class BehaviorGraph : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextAsset behaviorFile;
    [SerializeField] private StructureTree structureTree;
    [SerializeField] private GameObject behaviorNodePrefab;
    [SerializeField] private GameObject explorationBehaviorNodePrefab;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private TextMeshProUGUI pathText;
    [SerializeField] private TextMeshProUGUI traceText;

    [Header("Arrow Settings")]
    [SerializeField] private Color callStartColor = new Color(0, 1, 0);
    [SerializeField] private Color callEndColor = new Color(1, 0, 0);

    [Header("Graph Settings")]
    [SerializeField] private float graphScaleFactor = 25;

    [Header("Text Settings")]
    [SerializeField] private Color normalTextColor = new Color(1, 1, 1);
    [SerializeField] private Color errorTextColor = new Color(1, 0, 0);

    private static Set<GameObject> _selectedComponents = new Set<GameObject>();
    private Dictionary<string, BehaviorNode> _behaviorNodes = new Dictionary<string, BehaviorNode>();
    private List<ArrowVisual> arrows = new List<ArrowVisual>();

    private GeometryGraph graph = new GeometryGraph();
    Dictionary<string, Cluster> clusters = new Dictionary<string, Cluster>();

    private BehaviorParser behaviorParser;

    private static int activePathNumber = 0;
    private static int traceDepth = 1;
    private static XmlNode activeTraceNode;

    public static void AddToSelection(GameObject component)
    {
        _selectedComponents.Insert(component);
        OnSelectionChanged.Invoke();
    }

    public static void RemoveFromSelection(GameObject component)
    {
        _selectedComponents.Remove(component);
        OnSelectionChanged.Invoke();
    }

    public static void ClearSelection()
    {
        _selectedComponents.Clear();
        OnSelectionChanged.Invoke();
    }

    public static bool IsSelected(GameObject component)
    {
        return _selectedComponents.Contains(component);
    }

    public static UnityEvent OnSelectionChanged = new UnityEvent();

    private void Awake()
    {
        OnSelectionChanged.AddListener(OnSelectionChangedEvent);
        SettingsMenu.OnBehaviorModeChange.AddListener(OnBehaviorModeChange);
        SettingsMenu.OnClusteringChange.AddListener(OnClusteringChange);
        SettingsMenu.OnTraceModeChange.AddListener(OnTraceModeChange);
    }

    private void OnSelectionChangedEvent()
    {
        Build();
    }

    private void OnClusteringChange(object arg0)
    {
        Build();
    }

    private void OnBehaviorModeChange(object arg0)
    {
        UpdatePathText();
        UpdateTraceText();

        Build();
    }

    private void OnTraceModeChange(object arg0)
    {
        Build();
    }

    private void Start()
    {
        behaviorParser = new BehaviorParser(behaviorFile, structureTree.Root.Id);
    }

    public void Build()
    {
        if (behaviorParser == null)
        {
            return;
        }

        RemoveAllNodes();
        RemoveEdges();
        SpawnNodes();
        switch (SettingsMenu.SelectedBehaviorMode)
        {
            case SettingsMenu.BehaviorMode.Aggregation:
                GenerateAggregatedCalls();
                break;
            case SettingsMenu.BehaviorMode.Path:
                GeneratePath();
                GenerateClusters();
                break;
            case SettingsMenu.BehaviorMode.Trace:
                GenerateTrace();
                GenerateClusters();
                break;
        }
        InitializeLayout();
        PositionNodes();
        DrawArrows();
    }

    private void SpawnNodes()
    {
        if (SettingsMenu.SelectedTraceMode == SettingsMenu.TraceMode.Explore && SettingsMenu.SelectedBehaviorMode == SettingsMenu.BehaviorMode.Trace)
        {
            foreach (GameObject component in _selectedComponents)
            {
                SpawnExplorationNode(component, 0);
            }
        }
        else
        {
            foreach (GameObject component in _selectedComponents)
            {
                SpawnNode(component);
            }
        }
    }

    private void SpawnNode(GameObject component)
    {
        GameObject behaviorComponent = Instantiate(behaviorNodePrefab, transform);
        BehaviorNode behaviorNode = behaviorComponent.GetComponent<BehaviorNode>();
        behaviorNode.Initialize(component, structureTree.Root.Id);

        Vector2 size = new Vector2(behaviorNode.Size.x, behaviorNode.Size.z);

        Node node = new Node(CurveFactory.CreateRectangle(size.x * graphScaleFactor, size.y * graphScaleFactor, new Point()), behaviorNode);

        graph.Nodes.Add(node);
        _behaviorNodes.Add(behaviorNode.Id, behaviorNode);
    }

    private ExplorationBehaviorNode SpawnExplorationNode(GameObject component, int depth)
    {
        GameObject explorationBehaviorComponent = Instantiate(explorationBehaviorNodePrefab, transform);
        ExplorationBehaviorNode explorationBehaviorNode = explorationBehaviorComponent.GetComponent<ExplorationBehaviorNode>();
        explorationBehaviorNode.Initialize(component, structureTree.Root.Id);

        Vector2 size = new Vector2(explorationBehaviorNode.Size.x, explorationBehaviorNode.Size.z);

        Node node = new Node(CurveFactory.CreateRectangle(size.x * graphScaleFactor, size.y * graphScaleFactor, new Point()), explorationBehaviorNode);

        graph.Nodes.Add(node);

        string idString = depth > 0 ? explorationBehaviorNode.Id + depth : explorationBehaviorNode.Id;

        _behaviorNodes.Add(idString, explorationBehaviorNode);

        return explorationBehaviorNode;
    }

    private void GenerateClusters()
    {
        if (SettingsMenu.SelectedClustering == SettingsMenu.BehaviorGraphClustering.Disabled)
        {
            return;
        }

        foreach (Node node in graph.Nodes)
        {
            BehaviorNode behaviorNode = (BehaviorNode)node.UserData;
            string id = behaviorNode.Id;

            // Get the package of the node (remove last . and everything after it)
            id = id.Substring(0, id.LastIndexOf('.'));

            if (clusters.ContainsKey(id))
            {
                clusters[id].AddChild(node);
            }
            else
            {
                Cluster cluster = new Cluster();
                cluster.AddChild(node);
                clusters.Add(id, cluster);
            }
        }

        foreach (var cluster in clusters)
        {
            graph.RootCluster.AddChild(cluster.Value);
        }
    }

    private void InitializeLayout()
    {
        GraphUtil.ApplyInitialLayout(graph);
        GraphUtil.CenterGraph(graph);
    }

    private void PositionNodes()
    {
        foreach (Node node in graph.Nodes)
        {
            BehaviorNode behaviorNode = (BehaviorNode)node.UserData;
            Vector2 position = GraphUtil.PointToVector2(node.BoundingBox.Center);
            behaviorNode.transform.localPosition = new Vector3(position.x / graphScaleFactor, 0, position.y / graphScaleFactor);
        }
    }

    private void DrawArrows()
    {
        Physics.SyncTransforms();

        foreach (ArrowVisual arrow in arrows)
        {
            arrow.Draw();
        }

        if (SettingsMenu.SelectedTraceMode == SettingsMenu.TraceMode.Explore && SettingsMenu.SelectedBehaviorMode == SettingsMenu.BehaviorMode.Trace)
        {
            // Get first node in _behaviorNodes
            ExplorationBehaviorNode startNode = _behaviorNodes.Values.First() as ExplorationBehaviorNode;
            startNode.Close();
        }
    }

    private void GenerateAggregatedCalls()
    {
        foreach (var startComponent in behaviorParser.Calls)
        {
            if (_behaviorNodes.ContainsKey(startComponent.Key))
            {
                foreach (var endComponent in startComponent.Value)
                {
                    if (_behaviorNodes.ContainsKey(endComponent.Key))
                    {
                        BehaviorNode startNode = _behaviorNodes[startComponent.Key];
                        BehaviorNode endNode = _behaviorNodes[endComponent.Key];

                        Collider startCollider = startNode.Collider;
                        Collider endCollider = endNode.Collider;

                        if (behaviorParser.Calls.ContainsKey(endComponent.Key) && behaviorParser.Calls[endComponent.Key].ContainsKey(startComponent.Key))
                        {
                            GenerateArrow(startCollider, endCollider, endComponent.Value, startNode.transform, true, 0, callStartColor, callEndColor);
                        }
                        else
                        {
                            GenerateArrow(startCollider, endCollider, endComponent.Value, startNode.transform, false, 0, callStartColor, callEndColor);
                        }

                        // Find nodes in the graph
                        Node startGraphNode = graph.Nodes.FirstOrDefault(n => (BehaviorNode)n.UserData == startNode);
                        Node endGraphNode = graph.Nodes.FirstOrDefault(n => (BehaviorNode)n.UserData == endNode);

                        Edge edge = new Edge(startGraphNode, endGraphNode);
                        edge.Weight = endComponent.Value;

                        graph.Edges.Add(edge);
                    }
                }
            }
        }
    }

    private void GeneratePath()
    {
        UpdatePathText();

        // If there are no selected components, do not generate any path
        if (_behaviorNodes.Count == 0)
        {
            return;
        }

        int currentPathNumber = activePathNumber;

        // If any of the selected nodes is not in the path, increase the path number
        while (!_behaviorNodes.All(node => behaviorParser.Paths[activePathNumber].Item1.Contains(node.Key)))
        {
            IncreasePath();

            if (activePathNumber == currentPathNumber)
            {
                pathText.color = errorTextColor;
                pathText.text = "No paths found for selection";
                return;
            }
        }

        UpdatePathText();

        float colorChange = 1f / (behaviorParser.Paths[activePathNumber].Item1.Length - 1);
        Color startColor = callStartColor;
        Color endColor = startColor + new Color(colorChange, -colorChange, 0);

        for (int i = 0; i < behaviorParser.Paths[activePathNumber].Item1.Length - 1; i++)
        {
            if (!_behaviorNodes.ContainsKey(behaviorParser.Paths[activePathNumber].Item1[i]))
            {
                GameObject component = ComponentManager.components[behaviorParser.Paths[activePathNumber].Item1[i]];
                SpawnNode(component);
            }

            if (!_behaviorNodes.ContainsKey(behaviorParser.Paths[activePathNumber].Item1[i + 1]))
            {
                GameObject component = ComponentManager.components[behaviorParser.Paths[activePathNumber].Item1[i + 1]];
                SpawnNode(component);
            }

            BehaviorNode startNode = _behaviorNodes[behaviorParser.Paths[activePathNumber].Item1[i]];
            BehaviorNode endNode = _behaviorNodes[behaviorParser.Paths[activePathNumber].Item1[i + 1]];

            Collider startCollider = startNode.Collider;
            Collider endCollider = endNode.Collider;

            GenerateArrow(startCollider, endCollider, 1, startNode.transform, false, i, startColor, endColor);

            // Find nodes in the graph
            Node startGraphNode = graph.Nodes.FirstOrDefault(n => (BehaviorNode)n.UserData == startNode);
            Node endGraphNode = graph.Nodes.FirstOrDefault(n => (BehaviorNode)n.UserData == endNode);

            Edge edge = new Edge(startGraphNode, endGraphNode);
            graph.Edges.Add(edge);

            startColor = endColor;
            endColor += new Color(colorChange, -colorChange, 0);
        }
    }

    private void GenerateTrace()
    {
        UpdateTraceText();

        if (_behaviorNodes.Count != 1)
        {
            activeTraceNode = null;
            return;
        }

        string key = _behaviorNodes.Keys.First();

        if (!behaviorParser.Traces.ContainsKey(key))
        {
            activeTraceNode = null;
            return;
        }

        if (activeTraceNode == null || !behaviorParser.Traces[key].Contains(activeTraceNode))
        {
            activeTraceNode = behaviorParser.Traces[key][0];
        }

        UpdateTraceText();

        Color startColor = callStartColor;
        float colorChange;

        switch (SettingsMenu.SelectedTraceMode)
        {
            case SettingsMenu.TraceMode.Depth:
                colorChange = 1f / (traceDepth);

                DrawTraceDepth(activeTraceNode, traceDepth, 0, startColor, colorChange);
                break;
            case SettingsMenu.TraceMode.Explore:
                // Honestly, the whole code for the exploration trace is a mess
                // Nevertheless, it works and I don't want to touch it
                // - Hidde van Esch (29-5-2024)
                colorChange = 1f / (GetTraceDepth(activeTraceNode, 0));

                DrawTraceExplorationRecursively(activeTraceNode, 0, startColor, colorChange);
                break;
        }
    }

    private void DrawTraceDepth(XmlNode node, int remainingDepth, int depth, Color startColor, float colorChange)
    {
        if (remainingDepth == 0)
        {
            return;
        }

        foreach (XmlNode childNode in node.ChildNodes)
        {
            DrawTraceDepthRecursively(node, remainingDepth, depth, childNode, startColor, colorChange);
        }
    }

    private void DrawTraceDepthRecursively(XmlNode node, int remainingDepth, int depth, XmlNode childNode, Color startColor, float colorChange)
    {
        if (!_behaviorNodes.ContainsKey(childNode.Attributes["class"].Value))
        {
            GameObject component = ComponentManager.components[(childNode.Attributes["class"].Value)];
            SpawnNode(component);
        }

        BehaviorNode startNode = _behaviorNodes[node.Attributes["class"].Value];
        BehaviorNode endNode = _behaviorNodes[childNode.Attributes["class"].Value];

        Collider startCollider = startNode.Collider;
        Collider endCollider = endNode.Collider;

        int count = int.Parse(childNode.Attributes["count"].Value);

        Color endColor = startColor + new Color(colorChange, -colorChange, 0);

        GenerateArrow(startCollider, endCollider, count, startNode.transform, false, depth, startColor, endColor);

        // Find nodes in the graph
        Node startGraphNode = graph.Nodes.FirstOrDefault(n => (BehaviorNode)n.UserData == startNode);
        Node endGraphNode = graph.Nodes.FirstOrDefault(n => (BehaviorNode)n.UserData == endNode);

        Edge edge = new Edge(startGraphNode, endGraphNode);
        edge.Weight = count;

        graph.Edges.Add(edge);

        DrawTraceDepth(childNode, remainingDepth - 1, depth + 1, endColor, colorChange);
    }

    private void DrawTraceExplorationRecursively(XmlNode node, int depth, Color startColor, float colorChange)
    {
        string idString = depth > 0 ? node.Attributes["class"].Value + depth : node.Attributes["class"].Value;
        ExplorationBehaviorNode startNode = _behaviorNodes[idString] as ExplorationBehaviorNode;

        foreach (XmlNode childNode in node.ChildNodes)
        {
            string className = childNode.Attributes["class"].Value;

            string classNameDepth = className + (depth + 1);

            if (!_behaviorNodes.ContainsKey(classNameDepth))
            {
                GameObject component = ComponentManager.components[(className)];
                startNode.AddChild(SpawnExplorationNode(component, depth + 1));
            }

            ExplorationBehaviorNode endNode = _behaviorNodes[classNameDepth] as ExplorationBehaviorNode;

            startNode.AddChild(endNode);
            endNode.AddParent(startNode);

            Collider startCollider = startNode.Collider;
            Collider endCollider = endNode.Collider;

            int count = int.Parse(childNode.Attributes["count"].Value);

            Color endColor = startColor + new Color(colorChange, -colorChange, 0);

            startNode.AddArrow(GenerateArrow(startCollider, endCollider, count, startNode.transform, false, depth, startColor, endColor));

            // Find nodes in the graph
            Node startGraphNode = graph.Nodes.FirstOrDefault(n => (BehaviorNode)n.UserData == startNode);
            Node endGraphNode = graph.Nodes.FirstOrDefault(n => (BehaviorNode)n.UserData == endNode);

            Edge edge = new Edge(startGraphNode, endGraphNode);
            edge.Weight = count;

            graph.Edges.Add(edge);

            DrawTraceExplorationRecursively(childNode, depth + 1, endColor, colorChange);
        }
    }

    private static int GetTraceDepth(XmlNode node, int currentDepth)
    {
        if (!node.HasChildNodes)
            return currentDepth;

        int maxDepth = currentDepth;

        foreach (XmlNode child in node.ChildNodes)
        {
            int childDepth = GetTraceDepth(child, currentDepth + 1);
            if (childDepth > maxDepth)
                maxDepth = childDepth;
        }

        return maxDepth;
    }

    private void UpdatePathText()
    {
        if (_behaviorNodes.Count == 0)
        {
            pathText.color = errorTextColor;
            pathText.text = "No components selected";
            return;
        }

        pathText.color = normalTextColor;
        pathText.text = $"Path: {activePathNumber + 1}/{behaviorParser.Paths.Count}";
    }

    private void UpdateTraceText()
    {
        if (_behaviorNodes.Count == 0)
        {
            traceText.color = errorTextColor;
            traceText.text = "No components selected";
            return;
        }

        if (_behaviorNodes.Count > 1)
        {
            traceText.color = errorTextColor;
            traceText.text = "More than one component selected";
            return;
        }

        string key = _behaviorNodes.Keys.First();

        if (!behaviorParser.Traces.ContainsKey(key))
        {
            traceText.color = errorTextColor;
            traceText.text = "No traces found for selection";
            return;
        }

        int index = behaviorParser.Traces[_behaviorNodes.Keys.First()].IndexOf(activeTraceNode);

        traceText.color = normalTextColor;

        string textToDisplay = $"Trace: {index + 1}/{behaviorParser.Traces[_behaviorNodes.Keys.First()].Count}";

        if (SettingsMenu.SelectedTraceMode == SettingsMenu.TraceMode.Depth)
        {
            textToDisplay += $"\nDepth: {traceDepth}";
        }

        traceText.text = textToDisplay;
    }

    private void IncreasePath()
    {
        activePathNumber++;
        if (activePathNumber >= behaviorParser.Paths.Count)
        {
            activePathNumber = 0;
        }
    }

    private void DecreasePath()
    {
        activePathNumber--;
        if (activePathNumber < 0)
        {
            activePathNumber = behaviorParser.Paths.Count - 1;
        }
    }

    private void IncreasePathFresh()
    {
        IncreasePath();
        RemoveAllNodes();
        RemoveEdges();
        SpawnNodes();
    }

    private void DecreasePathFresh()
    {
        DecreasePath();
        RemoveAllNodes();
        RemoveEdges();
        SpawnNodes();
    }

    private void IncreaseTrace()
    {
        if (activeTraceNode == null)
        {
            return;
        }

        // Find the index of the active trace node
        int index = behaviorParser.Traces[_behaviorNodes.Keys.First()].IndexOf(activeTraceNode);

        if (index + 1 < behaviorParser.Traces[_behaviorNodes.Keys.First()].Count)
        {
            activeTraceNode = behaviorParser.Traces[_behaviorNodes.Keys.First()][index + 1];
        }
    }

    private void DecreaseTrace()
    {
        if (activeTraceNode == null)
        {
            return;
        }

        // Find the index of the active trace node
        int index = behaviorParser.Traces[_behaviorNodes.Keys.First()].IndexOf(activeTraceNode);

        if (index - 1 >= 0)
        {
            activeTraceNode = behaviorParser.Traces[_behaviorNodes.Keys.First()][index - 1];
        }
    }

    private void IncreaseTraceDepth()
    {
        traceDepth++;
    }

    private void DecreaseTraceDepth()
    {
        if (traceDepth > 0)
        {
            traceDepth--;
        }
    }

    public void CommandIncreasePath()
    {
        int currentPathNumber = activePathNumber;

        IncreasePathFresh();

        // If any of the selected nodes is not in the path, increase the path number
        while (!_behaviorNodes.All(node => behaviorParser.Paths[activePathNumber].Item1.Contains(node.Key)))
        {
            IncreasePathFresh();

            if (activePathNumber == currentPathNumber)
            {
                break;
            }
        }

        Build();
    }

    public void CommandDecreasePath()
    {
        int currentPathNumber = activePathNumber;

        DecreasePathFresh();

        // If any of the selected nodes is not in the path, increase the path number
        while (!_behaviorNodes.All(node => behaviorParser.Paths[activePathNumber].Item1.Contains(node.Key)))
        {
            DecreasePathFresh();

            if (activePathNumber == currentPathNumber)
            {
                break;
            }
        }

        Build();
    }

    public void CommandIncreaseTrace()
    {
        IncreaseTrace();
        Build();
    }

    public void CommandDecreaseTrace()
    {
        DecreaseTrace();
        Build();
    }

    public void CommandIncreaseTraceDepth()
    {
        IncreaseTraceDepth();
        Build();
    }

    public void CommandDecreaseTraceDepth()
    {
        DecreaseTraceDepth();
        Build();
    }

    private GameObject GenerateArrow(Collider startCollider, Collider endCollider, int amount, Transform startTransform, bool isBiDirectional, int depth, Color startColor, Color endColor)
    {
        GameObject arrow = Instantiate(arrowPrefab, startTransform);
        ArrowVisual arrowVisual = arrow.GetComponent<ArrowVisual>();
        arrowVisual.Initialize(startCollider, endCollider, amount, isBiDirectional, depth, startColor, endColor);
        arrows.Add(arrowVisual);

        return arrow;
    }

    private void RemoveEdges()
    {
        graph.Edges.Clear();
        arrows.Clear();
    }

    private void RemoveAllNodes()
    {
        foreach (var behaviorNode in _behaviorNodes)
        {
            Destroy(behaviorNode.Value.gameObject);
        }
        _behaviorNodes.Clear();
        graph.Nodes.Clear();
        graph.RootCluster.ClearClusters();
        clusters.Clear();
    }
}

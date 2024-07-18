using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class BehaviorParser
{
    private TextAsset behaviorFile;
    private XmlNode root;
    private List<Tuple<string[], int>> _paths = new List<Tuple<string[], int>>();
    private Dictionary<string, Dictionary<string, int>> _calls = new Dictionary<string, Dictionary<string, int>>();
    private Dictionary<string, List<XmlNode>> _traces = new Dictionary<string, List<XmlNode>>();
    private string hierarchyRootName;

    public List<Tuple<string[], int>> Paths
    {
        get => _paths;
    }

    public Dictionary<string, Dictionary<string, int>> Calls
    {
        get => _calls;
    }

    public Dictionary<string, List<XmlNode>> Traces
    {
        get => _traces;
    }

    public BehaviorParser(TextAsset behaviorFile, string hierarchyRootName)
    {
        this.behaviorFile = behaviorFile;
        this.hierarchyRootName = hierarchyRootName;

        Parse();

        Build();
    }

    private void Parse()
    {
        XmlDocument xmlDoc = new XmlDocument();

        try
        {
            xmlDoc.LoadXml(behaviorFile.text);

            root = xmlDoc.DocumentElement;
        }
        catch (XmlException e)
        {
            Debug.LogError("XML parsing error: " + e.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }

    private void Build()
    {
        RemoveNonHierarchyNodes(root);

        foreach (XmlNode node in root.ChildNodes)
        {
            GeneratePaths(node, new List<string>());
        }

        //OutputPathsFile();

        foreach (Tuple<string[], int> path in _paths)
        {
            GenerateCalls(path);
        }

        foreach (XmlNode node in root.ChildNodes)
        {
            GenerateTraces(node);
        }
    }

    private void RemoveNonHierarchyNodes(XmlNode node)
    {
        List<XmlNode> nodesToRemove = new List<XmlNode>();
        List<XmlNode> nodesToAppend = new List<XmlNode>();

        while (!AllChildrenHierarchyRootName(node))
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                string className = childNode.Attributes["class"].Value;
                if (!className.StartsWith(hierarchyRootName) || className.Contains("$"))
                {
                    nodesToRemove.Add(childNode);
                    foreach (XmlNode grandChildNode in childNode.ChildNodes)
                    {
                        nodesToAppend.Add(grandChildNode);
                    }
                }
            }

            foreach (XmlNode nodeToAppend in nodesToAppend)
            {
                node.AppendChild(nodeToAppend);
            }

            foreach (XmlNode nodeToRemove in nodesToRemove)
            {
                node.RemoveChild(nodeToRemove);
            }

            nodesToAppend.Clear();
            nodesToRemove.Clear();
        }

        foreach (XmlNode childNode in node.ChildNodes)
        {
            RemoveNonHierarchyNodes(childNode);
        }
    }

    private bool AllChildrenHierarchyRootName(XmlNode node)
    {
        foreach (XmlNode childNode in node.ChildNodes)
        {
            if (!childNode.Attributes["class"].Value.StartsWith(hierarchyRootName) || childNode.Attributes["class"].Value.Contains("$"))
            {
                return false;
            }
        }

        return true;
    }

    private void GenerateCalls(Tuple<string[], int> path)
    {
        string[] pathComponents = path.Item1;
        int pathAmount = path.Item2;

        for (int i = 0; i < pathComponents.Length - 1; i++)
        {
            string firstComponent = pathComponents[i];
            string secondComponent = pathComponents[i + 1];

            if (!_calls.ContainsKey(firstComponent))
            {
                _calls[firstComponent] = new Dictionary<string, int>();
            }

            if (!_calls[firstComponent].ContainsKey(secondComponent))
            {
                _calls[firstComponent][secondComponent] = pathAmount;
            }
            else
            {
                _calls[firstComponent][secondComponent] += pathAmount;
            }
        }
    }

    private void GeneratePaths(XmlNode node, List<string> currentTrace)
    {
        // Check if the current node is a leaf node
        if (node.ChildNodes.Count == 0)
        {
            // Extract class name and count
            string className = node.Attributes["class"].Value;
            int count = int.Parse(node.Attributes["count"].Value);

            // Clone the current trace list and add the method name
            List<string> newTrace = new List<string>(currentTrace);

            // Add the class name to the current trace list
            newTrace.Add(className);

            // Add the trace to the list of traces
            addPath(newTrace, count);
        }
        else
        {
            // Extract class name and method name
            string className = node.Attributes["class"].Value;

            // Add the class name to the current trace list
            currentTrace.Add(className);

            // Recursively traverse child nodes
            foreach (XmlNode childNode in node.ChildNodes)
            {
                // Pass the current trace list to the child node
                GeneratePaths(childNode, currentTrace);
            }

            // Remove the method name from the current trace list (backtrack)
            if (currentTrace.Count > 0 && currentTrace[currentTrace.Count - 1] == className)
            {
                currentTrace.RemoveAt(currentTrace.Count - 1);
            }
        }
    }

    private void GenerateTraces(XmlNode node)
    {
        // Extract class name and method name
        string className = node.Attributes["class"].Value;

        // If the current class is not a leaf, add to trace list
        if (node.ChildNodes.Count > 0)
        {
            if (!_traces.ContainsKey(className))
            {
                _traces[className] = new List<XmlNode>();
            }

            _traces[className].Add(node);
        }

        foreach (XmlNode childNode in node.ChildNodes)
        {
            GenerateTraces(childNode);
        }
    }

    private void addPath(List<string> path, int count)
    {
        if (path.Count > 1)
        {
            string[] traceArray = path.ToArray();

            // Check if the trace already exists
            for (int index = 0; index < _paths.Count; index++)
            {
                Tuple<string[], int> existingTrace = _paths[index];
                if (existingTrace.Item1.Length == traceArray.Length)
                {
                    bool equal = true;
                    for (int i = 0; i < traceArray.Length; i++)
                    {
                        if (existingTrace.Item1[i] != traceArray[i])
                        {
                            equal = false;
                            break;
                        }
                    }

                    if (equal)
                    {
                        // Create a new tuple with the updated count
                        Tuple<string[], int> updatedTrace = new Tuple<string[], int>(existingTrace.Item1, existingTrace.Item2 + count);

                        // Replace the existing tuple with the updated tuple in the traces list
                        _paths[index] = updatedTrace;
                        return;
                    }
                }
            }

            _paths.Add(new Tuple<string[], int>(traceArray, count));
        }
    }

    private void OutputPathsFile()
    {
        // Open a file stream for writing
        using (StreamWriter writer = new StreamWriter("paths.txt"))
        {
            // Write each trace to the file
            foreach (var path in _paths)
            {
                // Convert the trace to a string representation
                string pathString = string.Join(" -> ", path.Item1) + " : " + path.Item2 + "\n";

                // Write the trace string to the file
                writer.WriteLine(pathString);
            }
        }
    }
}

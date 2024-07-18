using System;
using System.Collections.Generic;
using UnityEngine;

public class StructureParser
{
    private const string CONTAINS_LABEL = "contains";
    private const string SPECIALIZES_LABEL = "specializes";
    private const string INVOKES_LABEL = "invokes";
    private const string PACKAGE_LABEL = "package";
    private const string CLASS_LABEL = "class";
    private const string ABSTRACT_LABEL = "abstract";
    private const string INTERFACE_LABEL = "interface";
    private const string METHOD_LABEL = "method";

    [Serializable]
    private class HierarchyJSON
    {
        public ElementData elements;
    }

    [Serializable]
    private class ElementData
    {
        public List<Node> nodes;
        public List<Edge> edges;
    }

    [Serializable]
    private class Node
    {
        public NodeData data;
    }

    [Serializable]
    private class Edge
    {
        public EdgeData data;
    }

    [Serializable]
    private class NodeData
    {
        public string id;
        public NodeProperties properties;
        public List<string> labels;
    }

    [Serializable]
    private class EdgeData
    {
        public string id;
        public string source;
        public EdgeProperties properties;
        public string target;
        public string label;
    }

    [Serializable]
    private class NodeProperties
    {
        public string simpleName;
        public string kind;
        //public string description;
        //public string roleStereotype;
    }

    [Serializable]
    private class EdgeProperties
    {
        public int weight;
    }

    private TextAsset hierarchyFile;

    private HierarchyJSON hierarchyJSON;

    private Dictionary<string, SoftwareComponent> _softwareComponents = new Dictionary<string, SoftwareComponent>();

    public StructureParser(TextAsset hierarchyFile)
    {
        this.hierarchyFile = hierarchyFile;

        Parse();

        Build();
    }

    public SoftwareComponent GetHierarchy()
    {
        SoftwareComponent root = FindRoot();

        return MergeRoots(root);
    }

    private void Parse()
    {
        hierarchyJSON = JsonUtility.FromJson<HierarchyJSON>(hierarchyFile.text);
    }

    private void Build()
    {
        BuildNodes();
        BuildEdges();
        LiftInvocations();
        SolveSpecializations();
        CountIndirectChildren();
    }

    private void BuildNodes()
    {
        foreach (Node node in hierarchyJSON.elements.nodes)
        {
            NodeData nodeData = node.data;
            NodeProperties properties = nodeData.properties;

            string id = nodeData.id;
            string name = properties.simpleName;

            SoftwareComponent component;

            switch (properties.kind)
            {
                case PACKAGE_LABEL:
                    component = new Package(id, name);
                    break;
                case CLASS_LABEL:
                    component = new Class(id, name);
                    break;
                case ABSTRACT_LABEL:
                    component = new AbstractClass(id, name);
                    break;
                case INTERFACE_LABEL:
                    component = new Interface(id, name);
                    break;
                case METHOD_LABEL:
                    component = new Method(id, name);
                    break;
                default:
                    continue;
            }

            _softwareComponents.Add(id, component);
        }
    }

    private void BuildEdges()
    {
        foreach (Edge edge in hierarchyJSON.elements.edges)
        {
            EdgeData edgeData = edge.data;
            string label = edgeData.label;
            string source = edgeData.source;
            string target = edgeData.target;

            if (_softwareComponents.ContainsKey(source) && _softwareComponents.ContainsKey(target))
            {
                SoftwareComponent sourceComponent = _softwareComponents[source];
                SoftwareComponent targetComponent = _softwareComponents[target];

                switch (label)
                {
                    case CONTAINS_LABEL:
                        sourceComponent.AddContainment(target);
                        sourceComponent.AddChild(_softwareComponents[target]);
                        targetComponent.Parent = sourceComponent;
                        break;
                    case INVOKES_LABEL:
                        sourceComponent.AddInvocation(targetComponent, edgeData.properties.weight);
                        break;
                    case SPECIALIZES_LABEL:
                        ((Class)sourceComponent).AddSpecialization((Class)targetComponent); // NOTE: Only classes can specialize
                        break;
                }
            }
        }
    }

    private void SolveSpecializations()
    {
        Dictionary<SoftwareComponent, Dictionary<SoftwareComponent, int>> invocationsToAdd = new Dictionary<SoftwareComponent, Dictionary<SoftwareComponent, int>>();

        // Iterate through all classes and their specializations and store the invocations
        foreach (SoftwareComponent component in _softwareComponents.Values)
        {
            if (component is Class startClass)
            {
                FindSpecializationInvocations(invocationsToAdd, startClass, startClass);
            }
        }

        // Add the invocations to the start class
        foreach (KeyValuePair<SoftwareComponent, Dictionary<SoftwareComponent, int>> entry in invocationsToAdd)
        {
            entry.Key.AddInvocations(entry.Value);
        }
    }

    private void FindSpecializationInvocations(Dictionary<SoftwareComponent, Dictionary<SoftwareComponent, int>> invocationsToAdd, Class startClass, Class currentClass)
    {
        foreach (Class specializedClass in currentClass.Specializes)
        {
            if (invocationsToAdd.ContainsKey(startClass))
            {
                foreach (KeyValuePair<SoftwareComponent, int> invocation in specializedClass.Invokes)
                {
                    if (invocationsToAdd[startClass].ContainsKey(invocation.Key))
                    {
                        invocationsToAdd[startClass][invocation.Key] += invocation.Value;
                    }
                    else
                    {
                        invocationsToAdd[startClass].Add(invocation.Key, invocation.Value);
                    }
                }
            }
            else
            {
                invocationsToAdd.Add(startClass, new Dictionary<SoftwareComponent, int>(specializedClass.Invokes));
            }

            FindSpecializationInvocations(invocationsToAdd, startClass, specializedClass);
        }
    }

    private void LiftInvocations()
    {
        foreach (SoftwareComponent component in _softwareComponents.Values)
        {
            if (component is Method invokingMethod)
            {
                foreach (KeyValuePair<SoftwareComponent, int> invocation in invokingMethod.Invokes)
                {
                    SoftwareComponent invokedComponentParent = invocation.Key.Parent;
                    int amount = invocation.Value;

                    if (component.Parent != invokedComponentParent)
                    {
                        component.Parent.AddInvocation(invokedComponentParent, amount); // Lift dependency
                    }
                }
            }
        }
    }

    private void CountIndirectChildren()
    {
        foreach (SoftwareComponent component in _softwareComponents.Values)
        {
            int count = 0;
            CountLeafs(component, ref count);
            component.IndirectChildrenCount = count;
        }
    }

    private void CountLeafs(SoftwareComponent component, ref int count)
    {
        foreach (SoftwareComponent child in component.Children)
        {
            if (child.Children.Count != 0)
            {
                CountLeafs(child, ref count);
            }
            else
            {
                count++;
            }
        }
    }

    private SoftwareComponent FindRoot()
    {
        foreach (SoftwareComponent component in _softwareComponents.Values)
        {
            if (component.Parent == null)
            {
                return component;
            }
        }

        throw new Exception("No root node found.");
    }

    private SoftwareComponent MergeRoots(SoftwareComponent root)
    {
        SoftwareComponent currentRoot = root;
        IReadOnlyList<SoftwareComponent> children = root.Children;
        while (children.Count == 1)
        {
            SoftwareComponent newRoot = children[0];
            newRoot.Name = newRoot.Id;
            currentRoot = newRoot;
            children = currentRoot.Children;
        }
        return currentRoot;
    }
}

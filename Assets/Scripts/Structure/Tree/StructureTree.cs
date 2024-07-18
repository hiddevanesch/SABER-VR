using System;
using System.Collections.Generic;
using UnityEngine;

public class StructureTree : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DiagramCircle classCircle;
    [SerializeField] private GameObject structureNodePrefab;
    [SerializeField] private GameObject wheelPrefab;
    [SerializeField] private TextAsset structureFile;

    [Header("Cam Tree Settings")]
    [SerializeField] private float nodeSize = 0.2f;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float radiusFalloff = 0.75f;
    [SerializeField] private float levelHeight = 0.75f;
    [SerializeField] private int startingDepth = 1;
    [SerializeField] private float colorSaturationDegradation = 0.1f;
    [SerializeField] private float colorVariationDegradation = 0.1f;
    [SerializeField] private bool reduceColorSaturation = true;
    [SerializeField] private bool reduceColorValue = true;

    private Color startingColor = new Color(.75f, .75f, .75f);
    private SoftwareComponent _root;
    private StructureNode rootObject;

    void Awake()
    {
        Build();
    }

    private void Update()
    {
        rootObject.Rotate(transform);
    }

    void Build()
    {
        _root = new StructureParser(structureFile).GetHierarchy();

        // TODO: Handle the case where the root is not a package
        InstantiateStructureSingular(_root, transform, 0, startingColor, 0);

        transform.Rotate(0, -90, 0);
    }

    void InstantiateClassDiagram(SoftwareComponent packageComponent, StructureNode parentPackage, Color color)
    {
        ClassDiagram classDiagramNode = classCircle.AddPackage(packageComponent, color);
        parentPackage.SetClassDiagram(classDiagramNode);
    }

    Transform InstantiateWheel(Transform parent, float radiusFactor)
    {
        GameObject wheelObject = Instantiate(wheelPrefab, parent);
        Transform wheelObjectTransform = wheelObject.transform;
        wheelObjectTransform.position += new Vector3(levelHeight, 0, 0);

        Transform wheelTransform = wheelObject.transform.Find("Wheel");

        wheelObjectTransform.parent = null;
        wheelObjectTransform.localScale = new Vector3(1, 1, 1);
        wheelObjectTransform.parent = parent;

        // Scale the wheel to match the radius
        wheelTransform.localScale = new Vector3(radiusFactor * 2, 0.25f * nodeSize, radiusFactor * 2);

        // Set the wheel as a child of the parent package
        StructureNode parentSoftwareComponent = parent.GetComponent<StructureNode>();
        parentSoftwareComponent.SetChildrenWheel(wheelObjectTransform);

        return wheelTransform.Find("Spawnpoint");
    }

    void InstantiateStructureSingular(SoftwareComponent packageComponent, Transform parent, int depth, Color color, float hueShift, StructureNode parentPackage = null)
    {
        Transform structureObject = SpawnStructureNode(parent);
        StructureNode structureNode = structureObject.GetComponent<StructureNode>();

        // Save to be able to rotate the nodes in the tree
        if (depth == 0)
        {
            rootObject = structureNode;
        }

        UpdateParentChildren(structureNode, parentPackage);

        structureNode.Initialize(parentPackage, packageComponent, color);

        PositionPackageSingular(structureObject, depth);
        InstantiateChildren(packageComponent, structureObject, depth, structureNode, color, hueShift);
    }

    void InstantiateStructureGrouped(SoftwareComponent packageComponent, Transform parent, int depth, float angle, float radiusFactor, Color color, float hueShift, StructureNode parentStructureNode = null)
    {
        Transform structureObject = SpawnStructureNode(parent);
        StructureNode structureNode = structureObject.GetComponent<StructureNode>();

        UpdateParentChildren(structureNode, parentStructureNode);

        structureNode.Initialize(parentStructureNode, packageComponent, color);

        PositionPackageGrouped(structureObject, angle, radiusFactor);
        InstantiateChildren(packageComponent, structureObject, depth, structureNode, color, hueShift);
    }

    Transform SpawnStructureNode(Transform parent)
    {
        GameObject structureObject = Instantiate(structureNodePrefab, parent);
        Transform structureObjectTransform = structureObject.transform;

        // Scale the wheel to match the radius
        structureObjectTransform.parent = null;
        structureObjectTransform.localScale = new Vector3(nodeSize, nodeSize, nodeSize);
        structureObjectTransform.parent = parent;

        return structureObjectTransform;
    }

    void UpdateParentChildren(StructureNode currentStructureNode, StructureNode parentStructureNode)
    {
        if (parentStructureNode != null)
        {
            parentStructureNode.AddChild(currentStructureNode);
        }
    }

    void PositionPackageSingular(Transform structureObject, int depth)
    {
        // Do not change the position of the root component
        if (depth > 0)
        {
            Vector3 position = new Vector3(levelHeight, 0, 0);
            structureObject.position += position;
        }
    }

    void PositionPackageGrouped(Transform componentObject, float angle, float radiusFactor)
    {
        Vector3 position = new Vector3(0, radiusFactor * Mathf.Cos(angle), radiusFactor * Mathf.Sin(angle));
        componentObject.position += position;
    }

    void InstantiateChildren(SoftwareComponent packageComponent, Transform parent, int depth, StructureNode currentStructureNode, Color color, float hueShift)
    {
        if (packageComponent.AllChildrenAreClasses())
        {
            InstantiateClassDiagram(packageComponent, currentStructureNode, color);
        }
        else if (packageComponent.AllChildrenArePackages())
        {
            currentStructureNode.EnableNestIcon();
            InstantiateNextLevel(packageComponent, parent, depth, currentStructureNode, color, hueShift);
        }
        else
        {
            throw new Exception("Package contains both classes and packages");
        }

        // Close the component if it is at the starting depth
        // This is done after construction of all children to ensure the GUI sizes are calculated correctly
        if (depth == startingDepth)
        {
            currentStructureNode.Close();
        }
    }

    private void InstantiateNextLevel(SoftwareComponent packageComponent, Transform parent, int depth, StructureNode currentStructureNode, Color color, float hueShift)
    {
        IReadOnlyList<SoftwareComponent> children = packageComponent.Children;
        int childCount = children.Count;
        if (childCount > 0)
        {
            if (childCount == 1)
            {
                InstantiateStructureSingular(children[0], parent, depth + 1, color, 0, currentStructureNode);
            }
            else
            {
                float newHueShift;
                Color[] newColors;
                if (depth == 0)
                {
                    newHueShift = childCount > 1 ? 1f / childCount : 0;
                    newColors = GenerateFirstColorVariations(color, childCount, newHueShift);
                }
                else
                {
                    newHueShift = hueShift / childCount;
                    newColors = GenerateNextColorVariations(color, childCount, newHueShift);
                }

                float radiusFactor = GetRadiusFactor(depth + 1);
                parent = InstantiateWheel(parent, radiusFactor);

                float angle = 0;
                float angleIncrement = (2 * Mathf.PI) / childCount;
                for (int i = 0; i < childCount; i++)
                {
                    float childAngle = angle + angleIncrement * i;
                    InstantiateStructureGrouped(children[i], parent, depth + 1, childAngle, radiusFactor, newColors[i], newHueShift, currentStructureNode);
                }
            }
        }
    }

    private Color[] GenerateFirstColorVariations(Color baseColor, int childCount, float hueShift)
    {
        Color[] variations = new Color[childCount];

        float baseHue, baseSaturation, baseValue;
        Color.RGBToHSV(baseColor, out baseHue, out baseSaturation, out baseValue);

        for (int i = 0; i < childCount; i++)
        {
            float variationHue = (baseHue + (hueShift * i)) % 1.0f;
            variations[i] = Color.HSVToRGB(variationHue, 1f, baseValue);
        }

        return variations;
    }

    private Color[] GenerateNextColorVariations(Color baseColor, int childCount, float hueShift)
    {
        Color[] variations = new Color[childCount];

        float baseHue, baseSaturation, baseValue;
        Color.RGBToHSV(baseColor, out baseHue, out baseSaturation, out baseValue);

        if (reduceColorSaturation)
        {
            baseSaturation = Mathf.Clamp01(baseSaturation - colorSaturationDegradation);
        }

        if (reduceColorValue)
        {
            baseValue = Mathf.Clamp01(baseValue - colorVariationDegradation);
        }

        for (int i = 0; i < childCount; i++)
        {
            float variationHue = (baseHue + (((i + 2) / 2) * hueShift * (i % 2 == 0 ? 0.5f : -0.5f))) % 1.0f;
            variations[i] = Color.HSVToRGB(variationHue, baseSaturation, baseValue);
        }

        return variations;
    }

    private float GetRadiusFactor(int depth)
    {
        return radius * (float)Math.Pow(radiusFalloff, depth);
    }

    public SoftwareComponent Root
    {
        get => _root;
    }
}

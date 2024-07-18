using TMPro;
using UnityEngine;
using Util;

public class BehaviorNode : MonoBehaviour, ITriggerable
{
    [Header("References")]
    [SerializeField] private Collider _collider;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private GameObject outlineMeshGameObject;
    [SerializeField] private Transform cubeTransform;
    [SerializeField] internal TextMeshProUGUI text;
    [SerializeField] private Material defaultUnlitMaterial;

    private ClassElement classUIElement;
    private string id;
    private string rootId;

    public Collider Collider
    {
        get => _collider;
    }

    public void Initialize(GameObject component, string rootId)
    {
        classUIElement = component.GetComponent<ClassElement>();
        SoftwareComponent softwareComponent = classUIElement.ClassComponent;
        id = softwareComponent.Id;
        this.rootId = rootId;

        UpdateText();
        SetColor(classUIElement.Color);

        if (BehaviorGraph.IsSelected(component))
        {
            outlineMeshGameObject.SetActive(true);
        }
    }

    private void UpdateText()
    {
        text.text = StringUtil.ReplaceDotsWithNewlines(id.Replace(rootId + ".", ""));
    }

    private void SetColor(Color color)
    {
        Material packageMaterial = new Material(defaultUnlitMaterial);
        packageMaterial.SetColor("_BaseColor", color);

        // Set the material to the mesh
        meshRenderer.material = packageMaterial;
    }

    public Vector3 Size
    {
        get => cubeTransform.localScale;
    }

    public string Id
    {
        get => id;
    }

    //############################//
    //                            //
    //        ITriggerable        //
    //                            //
    //############################//

    public void Trigger()
    {
        ((ITriggerable)classUIElement).Trigger();
    }
}

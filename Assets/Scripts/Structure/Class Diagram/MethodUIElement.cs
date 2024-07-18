using TMPro;
using UnityEngine;

public class MethodUIElement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI methodNameText;

    private SoftwareComponent _methodComponent;

    public void Initialize(SoftwareComponent methodComponent)
    {
        _methodComponent = methodComponent;

        methodNameText.text = "- " + _methodComponent.Name;
    }

    public SoftwareComponent MethodComponent
    {
        get => _methodComponent;
    }

    public float Width
    {
        get
        {
            methodNameText.ForceMeshUpdate();
            return methodNameText.textBounds.size.x;
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObjectReference : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Object _interactableObject;

    public IInteractable InteractableObject
    {
        get => ((GameObject)_interactableObject).GetComponent<IInteractable>();
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (_interactableObject != null && InteractableObject == null)
        {
            Debug.LogError($"{_interactableObject.name} does not implement IInteractable interface!");
            _interactableObject = null;
        }
    }
    #endif
}

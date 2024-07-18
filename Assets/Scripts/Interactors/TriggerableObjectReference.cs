using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerableObjectReference : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Object _triggerableObject;

    public ITriggerable TriggerableObject
    {
        get => ((GameObject)_triggerableObject).GetComponent<ITriggerable>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_triggerableObject != null && TriggerableObject == null)
        {
            Debug.LogError($"{_triggerableObject.name} does not implement ITriggerable interface!");
            _triggerableObject = null;
        }
    }
#endif
}

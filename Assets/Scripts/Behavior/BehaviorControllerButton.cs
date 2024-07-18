using UnityEngine;
using UnityEngine.Events;

public class BehaviorControllerButton : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private UnityEvent action;

    public void Interact()
    {
        action?.Invoke();
    }
}

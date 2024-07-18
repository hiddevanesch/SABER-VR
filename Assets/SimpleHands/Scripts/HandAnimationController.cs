using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private InputActionProperty pinchAction;
    [SerializeField] private InputActionProperty grabAction;

    void Update()
    {
        AnimatePinch();
        AnimateGrab();
    }

    void AnimatePinch()
    {
        animator.SetFloat("pinch", pinchAction.action.ReadValue<float>());
    }

    void AnimateGrab()
    {
        animator.SetFloat("grab", grabAction.action.ReadValue<float>());
    }
}

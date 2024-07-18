using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class RayInteractor : MonoBehaviour
{
    [Header("Mask Settings")]
    [SerializeField] private LayerMask interactablesLayerMask;
    [SerializeField] private LayerMask backgroundLayerMask;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty interactAction;
    [SerializeField] private InputActionProperty moveAction;

    [Header("Ray Settings")]
    [SerializeField] private float maxRayDistance = 5f;
    [SerializeField] private float lineLength = .3f;
    [SerializeField] private float thicknessFactor = 50f;
    [SerializeField] private Color defaultColorStart = new Color(1, 1, 1, 0.05f);
    [SerializeField] private Color defaultColorEnd = new Color(1, 1, 1, 0);
    [SerializeField] private Color hitColor = Color.white;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float clickSoundVolume = 1f;

    [Header("Haptic Settings")]
    [SerializeField] private HapticImpulsePlayer hapticImpulsePlayer;
    [SerializeField] private float hoverHapticIntensity = .1f;
    [SerializeField] private float hoverHapticDuration = .1f;
    [SerializeField] private float clickHapticIntensity = .25f;
    [SerializeField] private float clickHapticDuration = .1f;

    private AudioSource audioSource;
    private LineRenderer lineRenderer;

    private Collider lastHit = null;

    private InteractableObjectReference grabbedInteractableObject;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        moveAction.action.performed += OnMoveStarted;
        moveAction.action.canceled += OnMoveCanceled;
    }

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRayDistance, interactablesLayerMask))
        {
            InteractableObjectReference graphInteractableObject = hit.collider.gameObject.GetComponent<InteractableObjectReference>();

            if (graphInteractableObject != null && graphInteractableObject.InteractableObject is ClassElement)
            {
                grabbedInteractableObject = graphInteractableObject;
            }
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (grabbedInteractableObject != null)
        {
            ClassElement classUIElement = (ClassElement)grabbedInteractableObject.InteractableObject;
            grabbedInteractableObject = null;
        }
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRayDistance, interactablesLayerMask))
        {
            OnHit(hit);
        }
        else
        {
            OnNoHit();
        }
    }

    private void FixedUpdate()
    {
        if (grabbedInteractableObject != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, backgroundLayerMask))
            {
                ClassElement classUIElement = (ClassElement)grabbedInteractableObject.InteractableObject;
                if (hit.collider.transform == classUIElement.ClassDiagramNodeObject.transform)
                {
                    classUIElement.transform.position = hit.point;
                    classUIElement.UpdateParentSize();
                }
            }
        }
    }

    private void OnHit(RaycastHit hit)
    {
        if (hit.collider != lastHit)
        {
            OnHover();
            lastHit = hit.collider;
        }

        DrawRay(hit);

        HandleInteractable(hit);
        HandleTriggerable(hit);
    }

    private void HandleInteractable(RaycastHit hit)
    {
        if (interactAction.action.triggered && hit.collider.gameObject.TryGetComponent(out InteractableObjectReference interactableObjectReference))
        {
            OnClick();
            interactableObjectReference.InteractableObject.Interact();
        }
    }

    private void HandleTriggerable(RaycastHit hit)
    {
        if (triggerAction.action.triggered && hit.collider.gameObject.TryGetComponent(out TriggerableObjectReference triggerableObjectReference))
        {
            OnClick();
            triggerableObjectReference.TriggerableObject.Trigger();
        }
    }

    private void OnNoHit()
    {
        lastHit = null;

        HideRay();
    }

    private void DrawRay(RaycastHit hit)
    {
        Vector3 hitLocal = transform.InverseTransformPoint(hit.point);

        lineRenderer.SetPosition(1, hitLocal);

        lineRenderer.startColor = hitColor;
        lineRenderer.endColor = hitColor;
    }

    private void HideRay()
    {
        lineRenderer.SetPosition(1, new Vector3(0, 0, 100));

        lineRenderer.startColor = defaultColorStart;
        lineRenderer.endColor = defaultColorEnd;
    }

    private void OnHover()
    {
        hapticImpulsePlayer.SendHapticImpulse(hoverHapticIntensity, hoverHapticDuration);
    }

    private void OnClick()
    {
        audioSource.PlayOneShot(clickSound, clickSoundVolume);
        hapticImpulsePlayer.SendHapticImpulse(clickHapticIntensity, clickHapticDuration);
    }
}

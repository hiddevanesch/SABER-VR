using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class LeftHandInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HapticImpulsePlayer hapticImpulsePlayer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject structureTree;
    [SerializeField] private GameObject settingsMenu;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip toggleSound;
    [SerializeField] private float toggleSoundVolume = 1f;

    [Header("Haptic Settings")]
    [SerializeField] private float toggleHapticIntensity = .1f;
    [SerializeField] private float toggleHapticDuration = .1f;

    [Header("Controls")]
    [SerializeField] private InputActionProperty structureTreeButton;
    [SerializeField] private InputActionProperty settingsButton;

    private bool structureTreeEnabled = true;
    private bool settingsMenuEnabled = false;

    private void Awake()
    {
        structureTreeButton.action.performed += _ => TogglePackageTree();
        settingsButton.action.performed += _ => EnableSettingsMenu();
        settingsButton.action.canceled += _ => DisableSettingsMenu();
    }

    private void TogglePackageTree()
    {
        hapticImpulsePlayer.SendHapticImpulse(toggleHapticIntensity, toggleHapticDuration);
        audioSource.PlayOneShot(toggleSound, toggleSoundVolume);

        structureTreeEnabled = !structureTreeEnabled;

        UpdateStates();
    }

    private void EnableSettingsMenu()
    {
        settingsMenuEnabled = true;
        UpdateStates();
    }

    private void DisableSettingsMenu()
    {
        settingsMenuEnabled = false;
        UpdateStates();
    }

    private void UpdateStates()
    {
        if (settingsMenuEnabled)
        {
            settingsMenu.SetActive(true);
            structureTree.SetActive(false);
        }
        else if (structureTreeEnabled)
        {
            settingsMenu.SetActive(false);
            structureTree.SetActive(true);
        }
        else
        {
            settingsMenu.SetActive(false);
            structureTree.SetActive(false);
        }
    }
}

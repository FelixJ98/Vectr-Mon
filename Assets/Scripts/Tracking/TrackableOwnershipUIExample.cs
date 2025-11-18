using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Example script demonstrating how to connect a UI button to the TrackableOwnershipController
/// and how to validate ownership before performing owner-only actions.
/// 
/// Usage:
/// 1. Attach this script to a GameObject with a Button component (or assign button in Inspector)
/// 2. Assign the TrackableOwnershipController reference in the Inspector
/// 3. The button will automatically be wired to request ownership when clicked
/// 
/// For owner-only UI interactions:
/// - Use ValidateOwnership() to check ownership before executing code
/// - Use ExecuteIfOwner() wrapper for cleaner syntax
/// - Or attach OwnerOnlyUIInteractable component to automatically enable/disable UI
/// 
/// Alternatively, you can directly connect the button's onClick event to 
/// TrackableOwnershipController.RequestOwnership() in the Unity Inspector.
/// </summary>
[RequireComponent(typeof(Button))]
public class TrackableOwnershipUIExample : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The TrackableOwnershipController component attached to the trackable prefab. " +
             "If not assigned, will attempt to find it in the scene.")]
    [SerializeField]
    private TrackableOwnershipController ownershipController;

    [Header("UI Elements")]
    [Tooltip("The button that will trigger ownership request. " +
             "If not assigned, will use the Button component on this GameObject.")]
    [SerializeField]
    private Button requestOwnershipButton;

    private Button _button;

    #region Unity Lifecycle

    private void Awake()
    {
        // Get or find button component
        if (requestOwnershipButton == null)
        {
            _button = GetComponent<Button>();
        }
        else
        {
            _button = requestOwnershipButton;
        }

        if (_button == null)
        {
            DebugTag.LogError(nameof(TrackableOwnershipUIExample), $"No Button component found on {gameObject.name}.");
            enabled = false;
            return;
        }

        // Find ownership controller if not assigned
        if (ownershipController == null)
        {
            ownershipController = FindObjectOfType<TrackableOwnershipController>();
            
            if (ownershipController == null)
            {
                DebugTag.LogWarning(nameof(TrackableOwnershipUIExample), $"No TrackableOwnershipController found in scene. " +
                               "Please assign one in the Inspector.");
            }
        }
    }

    private void Start()
    {
        // Wire up button click event
        if (_button != null && ownershipController != null)
        {
            _button.onClick.AddListener(OnButtonClicked);
            DebugTag.Log(nameof(TrackableOwnershipUIExample), "Button wired to request ownership.");
        }

        // Subscribe to ownership events for logging
        if (ownershipController != null)
        {
            ownershipController.onOwnershipTransferred.AddListener(OnOwnershipTransferred);
            ownershipController.onOwnershipDenied.AddListener(OnOwnershipDenied);
            ownershipController.onOwnershipReleased.AddListener(OnOwnershipReleased);
        }
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        if (ownershipController != null)
        {
            ownershipController.onOwnershipTransferred.RemoveListener(OnOwnershipTransferred);
            ownershipController.onOwnershipDenied.RemoveListener(OnOwnershipDenied);
            ownershipController.onOwnershipReleased.RemoveListener(OnOwnershipReleased);
        }
    }

    #endregion

    #region Event Handlers

    private void OnButtonClicked()
    {
        if (ownershipController == null)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipUIExample), "Cannot request ownership: No TrackableOwnershipController assigned.");
            return;
        }

        // Request ownership
        ownershipController.RequestOwnership();
    }

    /// <summary>
    /// Example method showing how to validate ownership before performing an action.
    /// Call this from a UI button's onClick event for owner-only actions.
    /// </summary>
    public void PerformOwnerOnlyAction()
    {
        if (ownershipController == null)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipUIExample), "Cannot perform action: No TrackableOwnershipController assigned.");
            return;
        }

        // Method 1: Use ValidateOwnership() to check before executing
        if (ownershipController.ValidateOwnership("PerformOwnerOnlyAction"))
        {
            // Your owner-only code here
            DebugTag.Log(nameof(TrackableOwnershipUIExample), "Performing owner-only action!");
            // Example: Modify trackable, spawn objects, etc.
        }
    }

    /// <summary>
    /// Example method showing how to use ExecuteIfOwner() wrapper.
    /// Call this from a UI button's onClick event for owner-only actions.
    /// </summary>
    public void PerformOwnerOnlyActionWithWrapper()
    {
        if (ownershipController == null)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipUIExample), "Cannot perform action: No TrackableOwnershipController assigned.");
            return;
        }

        // Method 2: Use ExecuteIfOwner() wrapper (cleaner syntax)
        ownershipController.ExecuteIfOwner(() =>
        {
            // Your owner-only code here
            DebugTag.Log(nameof(TrackableOwnershipUIExample), "Performing owner-only action with wrapper!");
            // Example: Modify trackable, spawn objects, etc.
        }, "PerformOwnerOnlyActionWithWrapper");
    }

    private void OnOwnershipTransferred(ulong newOwnerId)
    {
        DebugTag.Log(nameof(TrackableOwnershipUIExample), $"Ownership transferred to client {newOwnerId}.");
    }

    private void OnOwnershipDenied(ulong deniedClientId)
    {
        DebugTag.Log(nameof(TrackableOwnershipUIExample), $"Ownership request denied for client {deniedClientId}.");
    }

    private void OnOwnershipReleased()
    {
        DebugTag.Log(nameof(TrackableOwnershipUIExample), "Ownership released.");
    }

    #endregion
}


using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Component that automatically enables/disables UI interactivity based on trackable ownership.
/// Attach this to UI elements (Buttons, Toggle, Slider, etc.) that should only be interactive
/// when the local client owns the associated trackable.
/// 
/// Usage:
/// 1. Attach this component to a UI element (Button, Toggle, etc.)
/// 2. Assign the TrackableOwnershipController reference
/// 3. The UI element will automatically be enabled/disabled based on ownership
/// </summary>
public class OwnerOnlyUIInteractable : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The TrackableOwnershipController component. " +
             "If not assigned, will search for it in the scene or parent hierarchy.")]
    [SerializeField]
    private TrackableOwnershipController ownershipController;

    [Header("Settings")]
    [Tooltip("If true, the UI element will be completely disabled (not just non-interactive). " +
             "If false, the element remains visible but non-interactive.")]
    [SerializeField]
    private bool disableGameObject = false;

    [Tooltip("Update interval in seconds. Lower values = more responsive but more CPU usage. " +
             "Default 0.1s (10 updates per second) is usually sufficient.")]
    [SerializeField]
    private float updateInterval = 0.1f;

    // Cached UI components
    private Selectable _selectable;
    private float _lastUpdateTime = 0f;
    private bool _lastOwnershipState = false;

    #region Unity Lifecycle

    private void Awake()
    {
        // Get Selectable component (Button, Toggle, Slider, etc. all inherit from Selectable)
        _selectable = GetComponent<Selectable>();

        if (_selectable == null)
        {
            DebugTag.LogWarning(nameof(OwnerOnlyUIInteractable), $"No Selectable component found on {gameObject.name}. " +
                           "This component requires a Selectable (Button, Toggle, Slider, etc.) to function.");
            enabled = false;
            return;
        }

        // Find ownership controller if not assigned
        if (ownershipController == null)
        {
            ownershipController = GetComponentInParent<TrackableOwnershipController>();
            
            if (ownershipController == null)
            {
                ownershipController = FindObjectOfType<TrackableOwnershipController>();
            }

            if (ownershipController == null)
            {
                DebugTag.LogWarning(nameof(OwnerOnlyUIInteractable), $"No TrackableOwnershipController found. " +
                               "Please assign one in the Inspector.");
                enabled = false;
                return;
            }
        }
    }

    private void Start()
    {
        // Subscribe to ownership events for immediate updates
        if (ownershipController != null)
        {
            ownershipController.onOwnershipTransferred.AddListener(OnOwnershipChanged);
            ownershipController.onOwnershipReleased.AddListener(OnOwnershipChanged);
        }

        // Initial update
        UpdateInteractivity();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ownershipController != null)
        {
            ownershipController.onOwnershipTransferred.RemoveListener(OnOwnershipChanged);
            ownershipController.onOwnershipReleased.RemoveListener(OnOwnershipChanged);
        }
    }

    private void Update()
    {
        // Periodic update (in case events are missed or for non-event-driven updates)
        float currentTime = Time.time;
        if (currentTime - _lastUpdateTime >= updateInterval)
        {
            _lastUpdateTime = currentTime;
            UpdateInteractivity();
        }
    }

    #endregion

    #region Event Handlers

    private void OnOwnershipChanged(ulong ownerId)
    {
        UpdateInteractivity();
    }

    private void OnOwnershipChanged()
    {
        UpdateInteractivity();
    }

    #endregion

    #region Interactivity Management

    private void UpdateInteractivity()
    {
        if (ownershipController == null || _selectable == null)
        {
            return;
        }

        bool isOwner = ownershipController.IsLocalClientOwner();
        
        // Only update if state changed (optimization)
        if (isOwner == _lastOwnershipState)
        {
            return;
        }

        _lastOwnershipState = isOwner;

        if (disableGameObject)
        {
            // Disable the entire GameObject
            gameObject.SetActive(isOwner);
        }
        else
        {
            // Keep GameObject active but disable interactivity
            _selectable.interactable = isOwner;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Manually refresh the interactivity state. Useful if ownership changes outside of events.
    /// </summary>
    public void RefreshInteractivity()
    {
        _lastOwnershipState = !_lastOwnershipState; // Force update by inverting last state
        UpdateInteractivity();
    }

    #endregion
}


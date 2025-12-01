using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// NetworkBehaviour component that allows clients to request ownership of a trackable NetworkObject
/// through UI interaction. Ownership transfer is handled server-side for security.
/// 
/// Attach this component to the trackable prefab spawned by TrackableManager.
/// Connect a UI button's onClick event to the RequestOwnership() method.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class TrackableOwnershipController : NetworkBehaviour
{
    public UnityEvent<ulong> onOwnershipTransferred;
    public UnityEvent<ulong> onOwnershipDenied;
    public UnityEvent onOwnershipReleased;

    [Header("Status Display")]
    [Tooltip("Optional: Text component to display ownership status. If assigned, will be automatically updated.")]
    [SerializeField]
    private TMP_Text statusText;

    // Network variable to track current owner (for UI display purposes)
    private NetworkVariable<ulong> _currentOwnerId = new NetworkVariable<ulong>(
        NetworkManager.ServerClientId,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Cached reference to NetworkObject for performance
    private NetworkObject _networkObject;

    // Track previous owner ID to detect ownership changes
    private ulong _previousOwnerId = NetworkManager.ServerClientId;

    // Track last update time for tick-based updates
    private float _lastStatusUpdateTime;

    #region Unity Lifecycle

    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
        
        if (_networkObject == null)
        {
            DebugTag.LogError(nameof(TrackableOwnershipController), $"NetworkObject component not found on {gameObject.name}. " +
                          "This component requires a NetworkObject to function.");
            enabled = false;
            return;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Initialize current owner ID
        _previousOwnerId = _networkObject.OwnerClientId;
        
        if (IsServer)
        {
            _currentOwnerId.Value = _networkObject.OwnerClientId;
            
            // Subscribe to client disconnect events to automatically reclaim ownership
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            }
        }

        // Subscribe to network variable changes for UI updates
        _currentOwnerId.OnValueChanged += OnOwnerIdChanged;

        // Initial status update
        UpdateStatusText();

        DebugTag.Log(nameof(TrackableOwnershipController), $"NetworkObject spawned. Initial owner: {_networkObject.OwnerClientId}");
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from events
        _currentOwnerId.OnValueChanged -= OnOwnerIdChanged;

        // Unsubscribe from client disconnect events
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        base.OnNetworkDespawn();
    }

    /// <summary>
    /// Called when this NetworkBehaviour gains ownership of the NetworkObject.
    /// </summary>
    public override void OnGainedOwnership()
    {
        base.OnGainedOwnership();
        
        ulong newOwnerId = _networkObject.OwnerClientId;
        HandleOwnershipChanged(_previousOwnerId, newOwnerId);
        _previousOwnerId = newOwnerId;
    }

    /// <summary>
    /// Called when this NetworkBehaviour loses ownership of the NetworkObject.
    /// </summary>
    public override void OnLostOwnership()
    {
        base.OnLostOwnership();
        
        ulong newOwnerId = _networkObject.OwnerClientId;
        HandleOwnershipChanged(_previousOwnerId, newOwnerId);
        _previousOwnerId = newOwnerId;
    }

    private void Update()
    {
        // Update status text based on network tick rate
        if (statusText != null && NetworkManager.Singleton != null)
        {
            uint tickRate = NetworkManager.Singleton.NetworkConfig.TickRate;
            if (tickRate > 0)
            {
                float tickInterval = 1f / tickRate;
                if (Time.time - _lastStatusUpdateTime >= tickInterval)
                {
                    UpdateStatusText();
                    _lastStatusUpdateTime = Time.time;
                }
            }
        }
    }

    #endregion

    #region Public API (Callable from UI)

    /// <summary>
    /// Public method that can be called from a UI button's onClick event.
    /// Requests ownership transfer to the local client.
    /// </summary>
    public void RequestOwnership()
    {
        if (!IsSpawned)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), "Cannot request ownership: NetworkObject is not spawned.");
            return;
        }

        if (!IsClient)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), "Cannot request ownership: Not a client.");
            return;
        }

        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        // Check if already owner
        if (_networkObject.OwnerClientId == localClientId)
        {
            DebugTag.Log(nameof(TrackableOwnershipController), $"Client {localClientId} already owns this trackable.");
            return;
        }

        // Request ownership via ServerRpc
        RequestOwnershipServerRpc();
    }

    /// <summary>
    /// Releases ownership back to the server. Can be called from UI or code.
    /// </summary>
    public void ReleaseOwnership()
    {
        if (!IsSpawned)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), "Cannot release ownership: NetworkObject is not spawned.");
            return;
        }

        if (!IsOwner)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), "Cannot release ownership: Not the current owner.");
            return;
        }

        ReleaseOwnershipServerRpc();
    }

    /// <summary>
    /// Gets the current owner's client ID. Returns ServerClientId if owned by server.
    /// </summary>
    public ulong GetCurrentOwnerId()
    {
        if (!IsSpawned)
        {
            return NetworkManager.ServerClientId;
        }

        return _networkObject.OwnerClientId;
    }

    /// <summary>
    /// Checks if the local client is the owner of this trackable.
    /// </summary>
    public bool IsLocalClientOwner()
    {
        if (!IsSpawned || !IsClient)
        {
            return false;
        }

        return _networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId;
    }

    /// <summary>
    /// Validates that the local client is the owner before executing an action.
    /// Returns true if the local client is the owner, false otherwise.
    /// Use this to gate UI interactions that should only be available to the owner.
    /// </summary>
    /// <param name="actionName">Optional name of the action being validated (for logging)</param>
    /// <returns>True if local client is owner, false otherwise</returns>
    public bool ValidateOwnership(string actionName = "action")
    {
        if (!IsSpawned)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), $"Cannot perform {actionName}: NetworkObject is not spawned.");
            return false;
        }

        if (!IsClient)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), $"Cannot perform {actionName}: Not a client.");
            return false;
        }

        if (!IsLocalClientOwner())
        {
            ulong currentOwnerId = GetCurrentOwnerId();
            DebugTag.LogWarning(nameof(TrackableOwnershipController), $"Cannot perform {actionName}: " +
                          $"Local client is not the owner. Current owner: {currentOwnerId}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Executes an action only if the local client is the owner.
    /// Use this to wrap UI button callbacks that should only work for the owner.
    /// </summary>
    /// <param name="action">The action to execute if owner</param>
    /// <param name="actionName">Optional name of the action (for logging)</param>
    public void ExecuteIfOwner(System.Action action, string actionName = "action")
    {
        if (ValidateOwnership(actionName))
        {
            action?.Invoke();
        }
    }

    /// <summary>
    /// Gets a formatted status string describing the current ownership state.
    /// Use this to display ownership status in UI elements.
    /// </summary>
    /// <returns>Status string describing the current ownership state</returns>
    public string GetStatusText()
    {
        if (!IsSpawned)
        {
            return "Status: Not Spawned";
        }

        ulong ownerId = GetCurrentOwnerId();
        bool isLocalOwner = IsLocalClientOwner();

        if (isLocalOwner)
        {
            return "Status: You Own This";
        }
        
        // Treat the server as a regular client from the perspective of other users.
        // If the server owns the object, non-server clients should see it as owned,
        // not "available".
        if (ownerId == NetworkManager.ServerClientId)
        {
            // Host / server itself will already have hit the "You Own This" branch above.
            return "Status: Owned by Host";
        }

        return $"Status: Owned by Client {ownerId}";
    }

    #endregion

    #region Server RPCs

    /// <summary>
    /// ServerRpc to request ownership transfer. Called by clients via RequestOwnership().
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        if (!IsSpawned)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), "Cannot transfer ownership: NetworkObject is not spawned.");
            return;
        }

        // Validate that the requesting client exists
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(requestingClientId))
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), $"Cannot transfer ownership: Client {requestingClientId} is not connected.");
            DenyOwnershipClientRpc(requestingClientId);
            return;
        }

        ulong currentOwnerId = _networkObject.OwnerClientId;

        // Check if already owned by requesting client
        if (currentOwnerId == requestingClientId)
        {
            DebugTag.Log(nameof(TrackableOwnershipController), $"Client {requestingClientId} already owns this trackable.");
            return;
        }

        // Check exclusive ownership (always enforced)
        if (currentOwnerId != NetworkManager.ServerClientId)
        {
            DebugTag.Log(nameof(TrackableOwnershipController), $"Ownership denied: Trackable is already owned by client {currentOwnerId}. " +
                     $"Requesting client: {requestingClientId}");
            DenyOwnershipClientRpc(requestingClientId);
            return;
        }

        // Transfer ownership (ChangeOwnership returns void, so we verify after the call)
        _networkObject.ChangeOwnership(requestingClientId);
        
        // Verify ownership was transferred successfully
        if (_networkObject.OwnerClientId == requestingClientId)
        {
            _currentOwnerId.Value = requestingClientId;
            DebugTag.Log(nameof(TrackableOwnershipController), $"Ownership transferred from client {currentOwnerId} to client {requestingClientId}.");
            
            // Notify all clients of successful transfer
            OnOwnershipTransferredClientRpc(requestingClientId);
        }
        else
        {
            DebugTag.LogError(nameof(TrackableOwnershipController), $"Failed to transfer ownership to client {requestingClientId}. " +
                          $"Current owner is still {_networkObject.OwnerClientId}.");
            DenyOwnershipClientRpc(requestingClientId);
        }
    }

    /// <summary>
    /// ServerRpc to release ownership back to the server. Called by the current owner.
    /// </summary>
    [ServerRpc(RequireOwnership = true)]
    private void ReleaseOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong releasingClientId = rpcParams.Receive.SenderClientId;
        ulong currentOwnerId = _networkObject.OwnerClientId;

        // Verify that the caller is the current owner
        if (currentOwnerId != releasingClientId)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), $"Client {releasingClientId} attempted to release ownership, " +
                           $"but current owner is {currentOwnerId}.");
            return;
        }

        // Transfer ownership back to server (ChangeOwnership returns void, so we verify after the call)
        _networkObject.ChangeOwnership(NetworkManager.ServerClientId);
        
        // Verify ownership was transferred successfully
        if (_networkObject.OwnerClientId == NetworkManager.ServerClientId)
        {
            _currentOwnerId.Value = NetworkManager.ServerClientId;
            DebugTag.Log(nameof(TrackableOwnershipController), $"Ownership released by client {releasingClientId}. " +
                     "Trackable is now owned by the server.");
            
            // Notify all clients
            OnOwnershipReleasedClientRpc();
        }
        else
        {
            DebugTag.LogError(nameof(TrackableOwnershipController), $"Failed to release ownership from client {releasingClientId}. " +
                          $"Current owner is still {_networkObject.OwnerClientId}.");
        }
    }

    #endregion

    #region Client RPCs

    /// <summary>
    /// ClientRpc to notify all clients of successful ownership transfer.
    /// </summary>
    [ClientRpc]
    private void OnOwnershipTransferredClientRpc(ulong newOwnerId)
    {
        onOwnershipTransferred?.Invoke(newOwnerId);
        UpdateStatusText();

        // Log for the new owner
        if (IsClient && NetworkManager.Singleton.LocalClientId == newOwnerId)
        {
            DebugTag.Log(nameof(TrackableOwnershipController), $"You are now the owner of trackable: {gameObject.name}");
        }
    }

    /// <summary>
    /// ClientRpc to notify requesting client that ownership was denied.
    /// </summary>
    [ClientRpc]
    private void DenyOwnershipClientRpc(ulong deniedClientId)
    {
        // Only invoke event for the denied client
        if (IsClient && NetworkManager.Singleton.LocalClientId == deniedClientId)
        {
            onOwnershipDenied?.Invoke(deniedClientId);
            UpdateStatusText();
            DebugTag.Log(nameof(TrackableOwnershipController), $"Ownership request denied for trackable: {gameObject.name}");
        }
    }

    /// <summary>
    /// ClientRpc to notify all clients that ownership was released to the server.
    /// </summary>
    [ClientRpc]
    private void OnOwnershipReleasedClientRpc()
    {
        onOwnershipReleased?.Invoke();
        UpdateStatusText();
        DebugTag.Log(nameof(TrackableOwnershipController), $"Ownership released for trackable: {gameObject.name}");
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles NetworkObject ownership change events.
    /// </summary>
    private void HandleOwnershipChanged(ulong previousOwnerId, ulong newOwnerId)
    {
        if (IsServer)
        {
            _currentOwnerId.Value = newOwnerId;
        }

        DebugTag.Log(nameof(TrackableOwnershipController), $"Ownership changed from client {previousOwnerId} to client {newOwnerId} " +
                 $"on {gameObject.name}");
    }

    /// <summary>
    /// Handles client disconnection events. Automatically reclaims ownership if the disconnected client was the owner.
    /// </summary>
    private void HandleClientDisconnected(ulong disconnectedClientId)
    {
        if (!IsServer || !IsSpawned)
        {
            return;
        }

        ulong currentOwnerId = _networkObject.OwnerClientId;

        // If the disconnected client was the owner, automatically reclaim ownership
        if (currentOwnerId == disconnectedClientId)
        {
            DebugTag.Log(nameof(TrackableOwnershipController), $"Owner (client {disconnectedClientId}) disconnected. " +
                     "Automatically reclaiming ownership to server.");

            // ChangeOwnership returns void, so we verify after the call
            _networkObject.ChangeOwnership(NetworkManager.ServerClientId);
            
            // Verify ownership was transferred successfully
            if (_networkObject.OwnerClientId == NetworkManager.ServerClientId)
            {
                _currentOwnerId.Value = NetworkManager.ServerClientId;
                OnOwnershipReleasedClientRpc();
            }
            else
            {
                DebugTag.LogError(nameof(TrackableOwnershipController), $"Failed to reclaim ownership from disconnected client {disconnectedClientId}. " +
                              $"Current owner is still {_networkObject.OwnerClientId}.");
            }
        }
    }

    /// <summary>
    /// Handles NetworkVariable value changes for owner ID.
    /// </summary>
    private void OnOwnerIdChanged(ulong previousOwnerId, ulong newOwnerId)
    {
        UpdateStatusText();
    }

    /// <summary>
    /// Updates the status text UI element if assigned.
    /// </summary>
    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = GetStatusText();
    }

    #endregion

    #region Server-Only Utilities

    /// <summary>
    /// Server-only method to force ownership transfer. Useful for administrative purposes.
    /// </summary>
    public void ForceOwnershipTransfer(ulong newOwnerId)
    {
        if (!IsServer)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), "ForceOwnershipTransfer can only be called on the server.");
            return;
        }

        if (!IsSpawned)
        {
            DebugTag.LogWarning(nameof(TrackableOwnershipController), "Cannot force ownership transfer: NetworkObject is not spawned.");
            return;
        }

        ulong currentOwnerId = _networkObject.OwnerClientId;

        if (currentOwnerId == newOwnerId)
        {
            DebugTag.Log(nameof(TrackableOwnershipController), $"Client {newOwnerId} already owns this trackable.");
            return;
        }

        // ChangeOwnership returns void, so we verify after the call
        _networkObject.ChangeOwnership(newOwnerId);
        
        // Verify ownership was transferred successfully
        if (_networkObject.OwnerClientId == newOwnerId)
        {
            _currentOwnerId.Value = newOwnerId;
            DebugTag.Log(nameof(TrackableOwnershipController), $"Server forced ownership transfer from client {currentOwnerId} to client {newOwnerId}.");
            OnOwnershipTransferredClientRpc(newOwnerId);
        }
        else
        {
            DebugTag.LogError(nameof(TrackableOwnershipController), $"Failed to force ownership transfer to client {newOwnerId}. " +
                          $"Current owner is still {_networkObject.OwnerClientId}.");
        }
    }


    #endregion
}


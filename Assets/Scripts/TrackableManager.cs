using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

/// <summary>
/// MRUK + Netcode for GameObjects (NGO) trackable manager.
/// All clients can contribute their AR trackables. Server owns all NetworkObjects.
/// Hook OnTrackableAdded/Removed from MRUK's TrackableAdded/Removed events.
/// </summary>
public class TrackableManager : NetworkBehaviour
{
    [Header("Networked Trackable Prefab")]
    [Tooltip("Prefab with a NetworkObject (and usually NetworkTransform). " +
             "Register it in NetworkManager > NetworkPrefabs.")]
    [SerializeField]
    private NetworkObject trackablePrefab;

    [Header("Events")]
    public UnityEvent<Transform> onTrackableAdded;
    public UnityEvent<Transform> onTrackableRemoved;
    public UnityEvent<string, Vector3, Quaternion> onPositionUpdated;

    [Header("Network Settings")]
    [Tooltip("Position update interval multiplier. 1 = match network tick rate, 2 = half rate, 0.5 = double rate. " +
             "Automatically calculated from NetworkManager.NetworkConfig.TickRate at runtime. " +
             "Lower values = higher precision but more network traffic.")]
    [SerializeField] private float positionUpdateIntervalMultiplier = 1.0f; // Match network tick rate by default
    
    [Tooltip("Minimum position change threshold before sending update. Lower values = higher precision. " +
             "1mm (0.001) matches ClientNetworkTransform default for sub-millimeter precision.")]
    [SerializeField] private float positionUpdateThreshold = 0.001f; // Only update if position changed by 1mm

    // Server-side: Track all trackables by payload (QR code identifier)
    private class ServerTrackableState
    {
        public NetworkObject NetObj;
        public ulong DetectingClientId; // Which client first detected this
        public Vector3 LastPos;
        public Quaternion LastRot;
        public bool LastIsTracked;
    }

    private readonly Dictionary<string, ServerTrackableState> _serverTrackables = new Dictionary<string, ServerTrackableState>();

    // Client-side: Track local MRUKTrackables we've detected
    private class ClientTrackableState
    {
        public MRUKTrackable Trackable;
        public Vector3 LastSentPos;
    }

    private readonly Dictionary<string, ClientTrackableState> _clientTrackables = new Dictionary<string, ClientTrackableState>();
    
    // Network tick-based timing
    private float _calculatedUpdateInterval = 0.0333f; // Default to 30Hz (1/30 seconds)
    private float _fixedUpdateAccumulator = 0f;

    // Client-side: Track spawned NetworkObjects by payload for position updates
    private readonly Dictionary<string, NetworkObject> _clientNetworkObjects = new Dictionary<string, NetworkObject>();

    // Server-side: Track MRUKTrackables detected by the server itself (host mode)
    private readonly Dictionary<string, MRUKTrackable> _serverLocalTrackables = new Dictionary<string, MRUKTrackable>();

    #region MRUK Events (hook these in the Inspector)

    // Called from MRUK.Settings.TrackableAdded (UnityEvent)
    public void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable == null)
        {
            Debug.LogWarning("[TrackableManager] OnTrackableAdded called with null trackable!");
            return;
        }

        // Only handle QR codes for now
        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
        {
            return;
        }

        string payload = trackable.MarkerPayloadString;
        if (string.IsNullOrEmpty(payload))
        {
            Debug.LogWarning("[TrackableManager] Trackable has empty payload!");
            return;
        }

        Debug.Log($"[TrackableManager] Client detected trackable: {payload}");

        // Store locally on client
        if (IsClient)
        {
            _clientTrackables[payload] = new ClientTrackableState
            {
                Trackable = trackable,
                LastSentPos = trackable.transform.position
            };

            if (IsServer)
            {
                // Host mode: track locally for direct updates, handle directly (no RPC)
                _serverLocalTrackables[payload] = trackable;
                HandleServerTrackableAdded(payload, trackable.transform.position, trackable.transform.rotation, NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                // Regular client: send to server via RPC
                AddTrackableServerRpc(payload, trackable.transform.position, trackable.transform.rotation);
            }
        }
        else if (IsServer)
        {
            // Server-only mode (unlikely but handle it)
            _serverLocalTrackables[payload] = trackable;
            HandleServerTrackableAdded(payload, trackable.transform.position, trackable.transform.rotation, NetworkManager.Singleton.LocalClientId);
        }

        // Invoke local event
        onTrackableAdded.Invoke(trackable.transform);
    }

    // Called from MRUK.Settings.TrackableRemoved (UnityEvent)
    public void OnTrackableRemoved(MRUKTrackable trackable)
    {
        if (trackable == null)
        {
            Debug.LogWarning("[TrackableManager] OnTrackableRemoved called with null trackable!");
            return;
        }

        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
        {
            return;
        }

        string payload = trackable.MarkerPayloadString;
        if (string.IsNullOrEmpty(payload))
        {
            return;
        }

        Debug.Log($"[TrackableManager] Client lost trackable: {payload}");

        // Remove from local tracking
        if (IsClient)
        {
            _clientTrackables.Remove(payload);
            
            if (IsServer)
            {
                // Host mode: handle directly (no RPC)
                _serverLocalTrackables.Remove(payload);
                HandleServerTrackableRemoved(payload);
            }
            else
            {
                // Regular client: notify server via RPC
                RemoveTrackableServerRpc(payload);
            }
        }
        else if (IsServer)
        {
            // Server-only mode
            _serverLocalTrackables.Remove(payload);
            HandleServerTrackableRemoved(payload);
        }

        // Invoke local event
        onTrackableRemoved.Invoke(trackable.transform);
    }

    #endregion

    #region Server RPCs (called by clients)

    [ServerRpc(RequireOwnership = false)]
    private void AddTrackableServerRpc(string payload, Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        HandleServerTrackableAdded(payload, position, rotation, clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveTrackableServerRpc(string payload, ServerRpcParams rpcParams = default)
    {
        HandleServerTrackableRemoved(payload);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTrackablePositionServerRpc(string payload, Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        if (!_serverTrackables.TryGetValue(payload, out var state))
        {
            // Trackable doesn't exist, add it (this can happen if client sends update before add)
            ulong clientId = rpcParams.Receive.SenderClientId;
            HandleServerTrackableAdded(payload, position, rotation, clientId);
            return;
        }

        // Update existing trackable position
        if (state.NetObj != null)
        {
            state.NetObj.transform.position = position;
            state.NetObj.transform.rotation = rotation;
        }

        state.LastPos = position;
        state.LastRot = rotation;

        // Broadcast position update to all clients via ClientRpc (pass NetworkObjectId for client lookup)
        ulong networkObjectId = state.NetObj != null ? state.NetObj.NetworkObjectId : 0;
        OnPositionUpdatedClientRpc(payload, position, rotation, networkObjectId);
    }

    #endregion

    #region Server-side handling

    private void HandleServerTrackableAdded(string payload, Vector3 position, Quaternion rotation, ulong detectingClientId)
    {
        // Check if already exists (multiple clients might detect the same QR code)
        if (_serverTrackables.ContainsKey(payload))
        {
            // Already exists, just update position directly (we're already on server)
            var state = _serverTrackables[payload];
            if (state.NetObj != null)
            {
                state.NetObj.transform.position = position;
                state.NetObj.transform.rotation = rotation;
            }
            state.LastPos = position;
            state.LastRot = rotation;
            ulong networkObjectId = state.NetObj != null ? state.NetObj.NetworkObjectId : 0;
            OnPositionUpdatedClientRpc(payload, position, rotation, networkObjectId);
            return;
        }

        if (trackablePrefab == null)
        {
            Debug.LogWarning("[TrackableManager] No trackablePrefab assigned!");
            return;
        }

        // Instantiate networked prefab
        NetworkObject instance = Instantiate(trackablePrefab, position, rotation);
        
        // Spawn with server ownership (no client ownership)
        instance.SpawnWithOwnership(NetworkManager.ServerClientId);

        _serverTrackables[payload] = new ServerTrackableState
        {
            NetObj = instance,
            DetectingClientId = detectingClientId,
            LastPos = position,
            LastRot = rotation,
            LastIsTracked = true
        };

        // Notify clients of the new trackable with NetworkObjectId for client-side tracking
        // Clients will receive this via the spawn event, but we also track it here for position updates
        OnTrackableSpawnedClientRpc(payload, instance.NetworkObjectId);

        Debug.Log($"[TrackableManager] Server spawned trackable: {payload} (detected by client {detectingClientId})");
    }

    private void HandleServerTrackableRemoved(string payload)
    {
        if (!_serverTrackables.TryGetValue(payload, out var state))
        {
            return;
        }

        if (state.NetObj != null && state.NetObj.IsSpawned)
        {
            state.NetObj.Despawn(true); // true = destroy the GameObject
        }

        // Notify clients to remove from tracking
        OnTrackableRemovedClientRpc(payload);

        _serverTrackables.Remove(payload);
        Debug.Log($"[TrackableManager] Server removed trackable: {payload}");
    }

    #endregion

    #region Client RPCs (called by server)

    [ClientRpc]
    private void OnTrackableSpawnedClientRpc(string payload, ulong networkObjectId)
    {
        // Track the NetworkObject on clients for position updates
        if (NetworkManager.Singleton != null && 
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            _clientNetworkObjects[payload] = netObj;
        }
    }

    [ClientRpc]
    private void OnTrackableRemovedClientRpc(string payload)
    {
        // Remove from client-side tracking
        _clientNetworkObjects.Remove(payload);
    }

    [ClientRpc]
    private void OnPositionUpdatedClientRpc(string payload, Vector3 position, Quaternion rotation, ulong networkObjectId)
    {
        // Find the NetworkObject by ID
        NetworkObject netObj = null;
        if (networkObjectId != 0 && NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out netObj))
            {
                // Update the transform on the client
                if (netObj != null)
                {
                    netObj.transform.position = position;
                    netObj.transform.rotation = rotation;
                }
            }
        }

        // Also try to find by payload if NetworkObjectId lookup failed (fallback)
        if (netObj == null && _clientNetworkObjects.TryGetValue(payload, out netObj))
        {
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.transform.position = position;
                netObj.transform.rotation = rotation;
            }
        }

        // Invoke position updated event on all clients
        onPositionUpdated.Invoke(payload, position, rotation);
    }

    #endregion

    #region Initialization

    private void Start()
    {
        // Calculate update interval based on network tick rate
        UpdateNetworkTickInterval();
    }

    /// <summary>
    /// Calculates the position update interval based on the current network tick rate.
    /// This ensures updates are synchronized with the server's network tick rate.
    /// </summary>
    private void UpdateNetworkTickInterval()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            uint tickRate = NetworkManager.Singleton.NetworkConfig.TickRate;
            if (tickRate > 0)
            {
                // Calculate interval: 1 second / tick rate * multiplier
                // For 30Hz: 1/30 * 1.0 = 0.0333 seconds
                _calculatedUpdateInterval = (1.0f / tickRate) * positionUpdateIntervalMultiplier;
                Debug.Log($"[TrackableManager] Position update interval set to {_calculatedUpdateInterval:F4}s ({tickRate}Hz * {positionUpdateIntervalMultiplier:F2})");
            }
            else
            {
                Debug.LogWarning("[TrackableManager] Network tick rate is 0, using default interval of 0.0333s (30Hz)");
                _calculatedUpdateInterval = 0.0333f;
            }
        }
        else
        {
            Debug.LogWarning("[TrackableManager] NetworkManager not available, using default interval of 0.0333s (30Hz)");
            _calculatedUpdateInterval = 0.0333f;
        }
    }

    #endregion

    #region Update loop

    /// <summary>
    /// FixedUpdate runs at a consistent rate and aligns better with network ticks.
    /// We accumulate time and send updates based on the calculated network tick interval.
    /// </summary>
    private void FixedUpdate()
    {
        // Recalculate interval if NetworkManager becomes available (handles late initialization)
        if (_calculatedUpdateInterval <= 0f && NetworkManager.Singleton != null)
        {
            UpdateNetworkTickInterval();
        }

        // Clients send position updates to server at network tick rate
        if (IsClient && _clientTrackables.Count > 0)
        {
            _fixedUpdateAccumulator += Time.fixedDeltaTime;
            
            // Send updates every calculated interval (aligned with network tick rate)
            // Use while loop to handle cases where fixedDeltaTime > interval (catch up)
            while (_fixedUpdateAccumulator >= _calculatedUpdateInterval)
            {
                _fixedUpdateAccumulator -= _calculatedUpdateInterval;
                SendPositionUpdates();
            }
        }

        // Server updates NetworkObject positions from MRUK trackables (if server also has MRUK)
        // This runs every FixedUpdate to keep server updates smooth
        if (IsServer)
        {
            UpdateServerTrackablePositions();
        }
    }

    private void SendPositionUpdates()
    {
        var toRemove = new List<string>();

        foreach (var kvp in _clientTrackables)
        {
            string payload = kvp.Key;
            ClientTrackableState state = kvp.Value;

            if (state.Trackable == null || state.Trackable.transform == null)
            {
                toRemove.Add(payload);
                continue;
            }

            Vector3 currentPosition = state.Trackable.transform.position;
            Quaternion currentRotation = state.Trackable.transform.rotation;

            // Only send update if position changed significantly
            float distance = Vector3.Distance(currentPosition, state.LastSentPos);
            if (distance < positionUpdateThreshold)
            {
                continue; // Skip if change is too small
            }

            UpdateTrackablePositionServerRpc(payload, currentPosition, currentRotation);
            state.LastSentPos = currentPosition;
        }

        // Clean up null trackables
        foreach (var payload in toRemove)
        {
            _clientTrackables.Remove(payload);
        }
    }

    private void UpdateServerTrackablePositions()
    {
        // Update positions for trackables detected by the server itself (host mode)
        // This ensures server-side MRUK trackables are synced without going through RPC
        var toRemove = new List<string>();

        foreach (var kvp in _serverLocalTrackables)
        {
            string payload = kvp.Key;
            MRUKTrackable trackable = kvp.Value;

            if (trackable == null || trackable.transform == null)
            {
                toRemove.Add(payload);
                continue;
            }

            if (!_serverTrackables.TryGetValue(payload, out var serverState))
            {
                // Server trackable not in server dictionary, skip
                continue;
            }

            Vector3 currentPosition = trackable.transform.position;
            Quaternion currentRotation = trackable.transform.rotation;

            // Only update if position changed significantly
            float distance = Vector3.Distance(currentPosition, serverState.LastPos);
            if (distance < positionUpdateThreshold)
            {
                continue; // Skip if change is too small
            }

            // Update directly on server (no RPC needed)
            if (serverState.NetObj != null)
            {
                serverState.NetObj.transform.position = currentPosition;
                serverState.NetObj.transform.rotation = currentRotation;
            }

            serverState.LastPos = currentPosition;
            serverState.LastRot = currentRotation;

            // Broadcast to all clients (pass NetworkObjectId for client lookup)
            ulong networkObjectId = serverState.NetObj != null ? serverState.NetObj.NetworkObjectId : 0;
            OnPositionUpdatedClientRpc(payload, currentPosition, currentRotation, networkObjectId);
        }

        // Clean up null trackables
        foreach (var payload in toRemove)
        {
            _serverLocalTrackables.Remove(payload);
        }
    }

    #endregion
}


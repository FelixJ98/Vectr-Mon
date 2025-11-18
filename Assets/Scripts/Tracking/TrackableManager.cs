using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// MRUK + Netcode for GameObjects (NGO) trackable manager.
/// All clients can contribute their AR trackables. Server owns all NetworkObjects.
/// Hook OnTrackableAdded/Removed from MRUK's TrackableAdded/Removed events.
/// This version delegates position synchronisation to NetworkTransform and only
/// handles spawning/despawning of trackable NetworkObjects on the server.
/// </summary>
public class TrackableManager : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject trackablePrefab;

    /// <summary>
    /// Server-side state for a spawned trackable instance keyed by its payload.
    /// </summary>
    private sealed class ServerTrackableState
    {
        public NetworkObject NetObj;
        public ulong DetectingClientId;
    }

    // Server-only: mapping from QR payload -> spawned NetworkObject state.
    private readonly Dictionary<string, ServerTrackableState> _serverTrackables =
        new Dictionary<string, ServerTrackableState>();

    #region MRUK Events (hook these in the Inspector)

    // Called from MRUK.Settings.TrackableAdded (UnityEvent)
    public void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable == null)
        {
            DebugTag.LogWarning(nameof(TrackableManager), "OnTrackableAdded called with null trackable!");
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
            DebugTag.LogWarning(nameof(TrackableManager), "Trackable has empty payload!");
            return;
        }

        DebugTag.Log(nameof(TrackableManager), $"Client detected trackable: {payload}");

        // Always route add events to the server; the server owns and spawns all NetworkObjects.
        if (!IsServer && IsClient)
        {
            // Regular client: notify server via RPC.
            AddTrackableServerRpc(payload, trackable.transform.position, trackable.transform.rotation);
        }
        else if (IsServer)
        {
            // Host / dedicated server: handle directly.
            ulong detectingClientId = NetworkManager.Singleton != null
                ? NetworkManager.Singleton.LocalClientId
                : NetworkManager.ServerClientId;

            HandleServerTrackableAdded(payload, trackable.transform.position, trackable.transform.rotation,
                detectingClientId);
        }
    }

    // Called from MRUK.Settings.TrackableRemoved (UnityEvent)
    public void OnTrackableRemoved(MRUKTrackable trackable)
    {
        if (trackable == null)
        {
            DebugTag.LogWarning(nameof(TrackableManager), "OnTrackableRemoved called with null trackable!");
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

        DebugTag.Log(nameof(TrackableManager), $"Client lost trackable: {payload}");

        if (!IsServer && IsClient)
        {
            // Regular client: notify server via RPC.
            RemoveTrackableServerRpc(payload);
        }
        else if (IsServer)
        {
            // Host / dedicated server: handle directly.
            HandleServerTrackableRemoved(payload);
        }
    }

    #endregion

    #region Server RPCs (called by clients)

    [ServerRpc(RequireOwnership = false)]
    private void AddTrackableServerRpc(string payload, Vector3 position, Quaternion rotation,
        ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        HandleServerTrackableAdded(payload, position, rotation, clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveTrackableServerRpc(string payload, ServerRpcParams rpcParams = default)
    {
        HandleServerTrackableRemoved(payload);
    }

    #endregion

    #region Server-side handling

    private void HandleServerTrackableAdded(string payload, Vector3 position, Quaternion rotation,
        ulong detectingClientId)
    {
        // If we already have a NetworkObject for this payload, just update its transform.
        if (_serverTrackables.TryGetValue(payload, out var existingState))
        {
            if (existingState.NetObj != null && existingState.NetObj.IsSpawned)
            {
                existingState.NetObj.transform.SetPositionAndRotation(position, rotation);
                DebugTag.Log(nameof(TrackableManager),
                    $"Server updated existing trackable: {payload} (detected by client {detectingClientId})");
            }

            return;
        }

        if (trackablePrefab == null)
        {
            DebugTag.LogWarning(nameof(TrackableManager), "No trackablePrefab assigned!");
            return;
        }

        // Instantiate networked prefab
        NetworkObject instance = Instantiate(trackablePrefab, position, rotation);

        // Spawn with server ownership (no client ownership). NetworkTransform on the prefab
        // will take care of synchronising transform state to all clients.
        instance.SpawnWithOwnership(NetworkManager.ServerClientId);

        _serverTrackables[payload] = new ServerTrackableState
        {
            NetObj = instance,
            DetectingClientId = detectingClientId
        };

        DebugTag.Log(nameof(TrackableManager),
            $"Server spawned trackable: {payload} (detected by client {detectingClientId})");
    }

    private void HandleServerTrackableRemoved(string payload)
    {
        if (!_serverTrackables.TryGetValue(payload, out var state))
        {
            return;
        }

        if (state.NetObj != null && state.NetObj.IsSpawned)
        {
            // true = destroy the GameObject on despawn
            state.NetObj.Despawn(true);
        }

        _serverTrackables.Remove(payload);
        DebugTag.Log(nameof(TrackableManager), $"Server removed trackable: {payload}");
    }

    #endregion
}
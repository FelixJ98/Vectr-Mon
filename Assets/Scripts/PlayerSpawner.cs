using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private TrackableManager trackableManager;
    
    private List<GameObject> spawnedPlayers = new List<GameObject>();

    private void Awake()
    {
        // Subscribe to trackable events
        trackableManager.onTrackableAdded.AddListener(TrackableAdded);
        trackableManager.onTrackableRemoved.AddListener(TrackableRemoved);
    }

    new void OnDestroy()
    {
        // Unsubscribe to trackable events
        base.OnDestroy();
        trackableManager.onTrackableAdded.AddListener(TrackableAdded);
        trackableManager.onTrackableRemoved.RemoveListener(TrackableRemoved);
    }

    void TrackableAdded(Transform trackableTransform)
    {
        Debug.Log($"[PlayerSpawner] Trackable Added: {trackableTransform.name}, isServer {IsServer}");

        // Ignore on non-server instances
        if (!IsServer)
            return;

        // Spawn a player under the added trackable
        GameObject instance = Instantiate(playerPrefab, trackableTransform);
        spawnedPlayers.Add(instance);
        
        Debug.Log($"[TrackableManager] Spawned prefab '{instance.name}' under trackable transform '{trackableTransform.name}'.");
    }

    private void TrackableRemoved(Transform trackableTransform)
    {
        Debug.Log($"[PlayerSpawner] Trackable removed: {trackableTransform.name}, isServer {IsServer}");
        
        // Ignore on non-server instances
        if (!IsServer)
            return;

        // Destroy the spawned players associated with the removed trackable
        foreach (var player in spawnedPlayers)
        {
            // Check if the player's parent is the removed trackable
            if (player.transform.parent == trackableTransform)
            {
                Destroy(player);
                Debug.Log($"[TrackableManager] Destroyed player '{player.name}' associated with removed trackable '{trackableTransform.name}'.");
            }
        }
    }
}
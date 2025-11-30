using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TrackableManager : NetworkBehaviour
{
    [SerializeField] private GameObject trackedObjectPrefab;
    NetworkList<FixedString512Bytes> keys = new NetworkList<FixedString512Bytes>();
    //public TextMeshProUGUI log;

    // Local reference for the client who spawned the object
    public GameObject spo;
    private MRUKTrackable trackable;

    private void Update()
    {
        if (spo != null && trackable != null)
        {
            // Update server with the position of this client's object
            //log.text = "IT RAN UPDATE";
            UpdateObjectLocationServerRpc(spo.GetComponent<NetworkObject>().NetworkObjectId, trackable.transform.position);
        }
    }

    public void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable == null)
        {
            Debug.LogWarning("[TrackableManager] OnTrackableAdded called with null trackable!");
            return;
        }
        this.trackable = trackable;
        Debug.Log($"[TrackableManager] Added trackable of type: {trackable.TrackableType}, GameObject: {trackable.gameObject.name}");

        if (trackable.TrackableType == OVRAnchor.TrackableType.QRCode)
        {
            string payload = trackable.MarkerPayloadString;
            Debug.Log($"[TrackableManager] Detected QR code payload: {payload}");

            if (!keys.Contains(payload))
            {
                RequestAddKeyServerRpc(payload);
                SpawnAndReturnServerRpc(trackable.transform.position, trackable.transform.rotation);
                Debug.Log($"[TrackableManager] Spawned prefab '{trackedObjectPrefab.name}' under QR code '{trackable.name}'.");
            }
            else
            {
                Debug.LogWarning("It was found?");
            }
        }
    }

    public void OnTrackableRemoved(MRUKTrackable trackable)
    {
        if (trackable == null)
        {
            Debug.LogWarning("[TrackableManager] OnTrackableRemoved called with null trackable!");
            return;
        }
        trackable = this.trackable;
        Debug.Log($"[TrackableManager] Removed trackable of type: {trackable.TrackableType}, GameObject: {trackable.gameObject.name}");
        Destroy(trackable.gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnAndReturnServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        // Spawn the object on the server
        var netObj = Instantiate(trackedObjectPrefab, position, rotation).GetComponent<NetworkObject>();
        //log = netObj.GetComponentInChildren<TextMeshProUGUI>();
        //log.text = "I BELONG TO " + rpcParams.Receive.SenderClientId;
        netObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);

        // Tell only the requesting client to assign their local 'spo'
        AssignLocalSpoClientRpc(netObj.NetworkObjectId, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId } }
        });
    }

    [ClientRpc]
    void AssignLocalSpoClientRpc(ulong networkObjectId, ClientRpcParams clientRpcParams = default)
    {
        // Runs only on the client who requested the spawn
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
        {
            spo = netObj.gameObject; // assign local reference
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateObjectLocationServerRpc(ulong networkObjectId, Vector3 position)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
        {
            netObj.transform.position = position; // update the correct object
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAddKeyServerRpc(FixedString512Bytes key, ServerRpcParams rpcParams = default)
    {
        if (!keys.Contains(key))
        {
            keys.Add(key);
        }
    }
}

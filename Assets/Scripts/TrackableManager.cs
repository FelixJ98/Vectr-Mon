using Meta.XR.MRUtilityKit;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static Oculus.Interaction.Context;

public class TrackableManager : MonoBehaviour
{
    [SerializeField] private GameObject trackedObjectPrefab;
    public GameObject spo; //SpawnedTrackedObject
    private MRUKTrackable trackable;
    private bool onceOnly = false;

    private void Update()
    {
        if (spo != null) {
            spo.transform.position = trackable.transform.position;
            spo.transform.rotation = trackable.transform.rotation;
        }
    }


    // Called by MR Utility Kit's TrackableAdded event
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

            if (trackedObjectPrefab != null)
            {
                if (!onceOnly && trackedObjectPrefab != null)
                {
                    StartCoroutine(SpawnCubeRoutine());
                }
                //GameObject instance = Instantiate(trackedObjectPrefab, trackable.transform);
                Debug.Log($"[TrackableManager] Spawned prefab '{trackedObjectPrefab.name}' under QR code '{trackable.name}'.");
            }
            else
            {
                Debug.LogWarning("[TrackableManager] trackedObjectPrefab is not assigned.");
            }
        }
    }

    // Called by MR Utility Kit's TrackableRemoved event
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

    private IEnumerator SpawnCubeRoutine()
    {
        onceOnly = true;

        // Ask the host to spawn the cube
        RequestSpawnServerRpc(trackable.transform.position, trackable.transform.rotation);

        yield return new WaitForSeconds(1f);
        onceOnly = false;
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestSpawnServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        //GameObject instance = Instantiate(trackedObjectPrefab, position, rotation);
        spo = Instantiate(trackedObjectPrefab, position, rotation);
        //instance.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        spo.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
    }
}






// Method does not work
/*
public class TrackableManager : NetworkBehaviour
{
    [SerializeField] private GameObject trackedObjectPrefab;
    private bool onceOnly = false;

    public void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable == null)
        {
            Debug.LogWarning("[TrackableManager] OnTrackableAdded called with null trackable!");
            return;
        }

        Debug.Log($"[TrackableManager] Added trackable of type: {trackable.TrackableType}, GameObject: {trackable.gameObject.name}");

        if (trackable.TrackableType == OVRAnchor.TrackableType.QRCode)
        {
            string payload = trackable.MarkerPayloadString;
            Debug.Log($"[TrackableManager] Detected QR code payload: {payload}");

            if (trackedObjectPrefab != null && !onceOnly)
            {
                StartCoroutine(SpawnAndAttachRoutine(trackable));
            }
        }
    }

    public void OnTrackableRemoved(MRUKTrackable trackable)
    {
        if (trackable == null) return;
        Debug.Log($"[TrackableManager] Removed trackable of type: {trackable.TrackableType}, GameObject: {trackable.gameObject.name}");
        Destroy(trackable.gameObject);
    }

    private IEnumerator SpawnAndAttachRoutine(MRUKTrackable trackable)
    {
        onceOnly = true;
        Vector3 pos = trackable.transform.position;
        Quaternion rot = trackable.transform.rotation;

        // Host/server spawns the object
        RequestSpawnServerRpc(pos, rot, trackable.GetInstanceID());
        yield return new WaitForSeconds(1f);
        onceOnly = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(Vector3 position, Quaternion rotation, int trackableInstanceId, ServerRpcParams rpcParams = default)
    {
        GameObject instance = Instantiate(trackedObjectPrefab, position, rotation);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);

        // Tell all clients to parent this spawned object to their local trackable
        AttachToTrackableClientRpc(netObj.NetworkObjectId, trackableInstanceId);
    }

    [ClientRpc]
    private void AttachToTrackableClientRpc(ulong spawnedObjectId, int trackableInstanceId)
    {
        var allTrackables = FindObjectsOfType<MRUKTrackable>();
        MRUKTrackable target = null;
        foreach (var t in allTrackables)
        {
            if (t.GetInstanceID() == trackableInstanceId)
            {
                target = t;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"[TrackableManager] Could not find local trackable with id {trackableInstanceId}");
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spawnedObjectId, out var netObj))
        {
            netObj.transform.SetParent(target.transform, worldPositionStays: true);
            Debug.Log($"[TrackableManager] Attached spawned prefab '{netObj.name}' to QR '{target.name}' locally.");
        }
    }
}
*/
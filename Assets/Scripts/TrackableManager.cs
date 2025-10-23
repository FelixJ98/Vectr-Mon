using Meta.XR.MRUtilityKit;
using UnityEngine;

public class TrackableManager : MonoBehaviour
{
    [SerializeField] private GameObject trackedObjectPrefab;

    // Called by MR Utility Kit's TrackableAdded event
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

            if (trackedObjectPrefab != null)
            {
                GameObject instance = Instantiate(trackedObjectPrefab, trackable.transform);
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

        Debug.Log($"[TrackableManager] Removed trackable of type: {trackable.TrackableType}, GameObject: {trackable.gameObject.name}");
        Destroy(trackable.gameObject);
    }
}
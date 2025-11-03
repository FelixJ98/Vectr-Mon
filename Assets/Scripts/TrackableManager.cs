using Meta.XR.MRUtilityKit;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class TrackableManager : NetworkBehaviour
{
    
    public UnityEvent<Transform> onTrackableAdded;
    public UnityEvent<Transform> onTrackableRemoved;
    
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

            onTrackableAdded.Invoke(trackable.transform);
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

        onTrackableRemoved.Invoke(trackable.transform);
    }
}
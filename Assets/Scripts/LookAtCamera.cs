using UnityEngine;
using UnityEngine.XR;

public class LookAtCamera : MonoBehaviour
{
    private Transform hmd;   // XR headset "center eye"

    void Start()
    {
        // Try to locate the XR HMD / Head transform    
        var xrRig = Camera.main;
        if (xrRig != null)
        {
            hmd = xrRig.transform;
        }
        else
        {
            Debug.LogWarning("VRLookAtUser: No XR camera found.");
        }
    }

    void LateUpdate()
    {
        if (hmd == null) return;

        // Direction toward the user's face
        Vector3 lookDir = hmd.position - transform.position;
        lookDir = -lookDir;

        // Keep UI upright (don’t tilt with the headset pitch/roll)
        lookDir.y = 0;

        // Apply rotation
        if (lookDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }
}

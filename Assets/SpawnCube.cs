using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SpawnCube : NetworkBehaviour
{
    [SerializeField] private GameObject theCube;
    private bool onceOnly = false;

    void Update()
    {
        // Every player can press A
        //if (!IsOwner) return;

        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            if (!onceOnly && theCube != null)
            {
                StartCoroutine(SpawnCubeRoutine());
            }
        }
    }

    private IEnumerator SpawnCubeRoutine()
    {
        onceOnly = true;

        // Ask the host to spawn the cube
        RequestSpawnServerRpc(transform.position);

        yield return new WaitForSeconds(1f);
        onceOnly = false;
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestSpawnServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
    {
        GameObject instance = Instantiate(theCube, position, Quaternion.identity);
        instance.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
    }

}

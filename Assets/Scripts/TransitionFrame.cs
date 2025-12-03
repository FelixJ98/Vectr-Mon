using Unity.Netcode;
using UnityEngine;

public class TransitionFrame : NetworkBehaviour
{
    public GameObject nextCanvas;
    public GameObject currentCanvas;

    // Called locally on button click
    public void OnButtonClick()
    {
        if (IsOwner || IsServer)
        {
            // Ask server to transition for all clients
            TransitionServerRpc();
        }
    }

    // ServerRpc called when a client clicks the button
    [ServerRpc(RequireOwnership = false)]
    private void TransitionServerRpc(ServerRpcParams rpcParams = default)
    {
        // Call a ClientRpc to update all clients
        TransitionClientRpc();
    }

    // ClientRpc runs on all clients to actually switch the canvases
    [ClientRpc]
    private void TransitionClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (currentCanvas != null)
            currentCanvas.SetActive(false);

        if (nextCanvas != null)
            nextCanvas.SetActive(true);
    }
}
using UnityEngine;
using Unity.Netcode;

public class TransitionFrame : NetworkBehaviour
{
    public GameObject nextCanvas;
    public GameObject currentCanvas;

    // Called locally when the button is clicked
    public void OnButtonClick()
    {
        if (IsOwner) // Ensure only the owner can request
        {
            RequestCanvasTransitionServerRpc();
        }
    }

    // Ask the server to switch canvases
    [ServerRpc]
    private void RequestCanvasTransitionServerRpc(ServerRpcParams rpcParams = default)
    {
        // Tell all clients to switch canvases
        ExecuteCanvasTransitionClientRpc();
    }

    // Actually perform the canvas switch on all clients
    [ClientRpc]
    private void ExecuteCanvasTransitionClientRpc(ClientRpcParams rpcParams = default)
    {
        currentCanvas.SetActive(false);
        nextCanvas.SetActive(true);
    }
}

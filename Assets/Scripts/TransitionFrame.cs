using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class TransitionFrame : NetworkBehaviour
{
    public GameObject nextCanvas;
    public GameObject currentCanvas;

    // Called locally by the player pressing the button
    public void OnButtonClick()
    {
        // Send request to server
        RequestTransitionServerRpc();
    }

    // SERVER ? validates & triggers transition for all clients
    [ServerRpc(RequireOwnership = false)]
    private void RequestTransitionServerRpc(ServerRpcParams rpcParams = default)
    {
        DoTransitionClientRpc();
    }

    // CLIENT ? actually switches the UI on each machine
    [ClientRpc]
    private void DoTransitionClientRpc()
    {
        currentCanvas.SetActive(false);
        nextCanvas.SetActive(true);
    }
}

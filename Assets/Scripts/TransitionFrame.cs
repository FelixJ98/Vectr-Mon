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
        Debug.Log("HelloTEST");
        RequestTransitionServerRpc();

    }

    // SERVER ? validates & triggers transition for all clients
    [ServerRpc(RequireOwnership = false)]
    void RequestTransitionServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("RegisteredTEST");
        DoTransitionClientRpc();
    }

    // CLIENT ? actually switches the UI on each machine
    [ClientRpc]
    void DoTransitionClientRpc()
    {
        Debug.Log("NOTEST");
        currentCanvas.SetActive(false);
        nextCanvas.SetActive(true);
        Debug.Log("YESTEST:");
        Debug.Log(currentCanvas.gameObject.activeSelf);
        Debug.Log(nextCanvas.gameObject.activeSelf);
    }
}

using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class SINGLETRANSITIONCONTROLLER : NetworkBehaviour
{
    public GameObject start;
    public GameObject Selection;
    public GameObject Panther;
    public GameObject Knight;
    public GameObject Shark;
    // Called locally by the player pressing the button

    private void Start()
    {
        Selection.SetActive(false);
        Panther.SetActive(false);
        Knight.SetActive(false);
        Shark.SetActive(false);
    }
    public void StarttoSelection()
    {
        // Send request to server
        Debug.Log("HelloTEST");
        DoStarttoSelectionServerRpc();

    }

    // SERVER ? validates & triggers transition for all clients
    [ServerRpc(RequireOwnership = false)]
    void DoStarttoSelectionServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("RegisteredTEST");
        DoStarttoSelectionClientRpc();
    }

    // CLIENT ? actually switches the UI on each machine
    [ClientRpc]
    void DoStarttoSelectionClientRpc()
    {
        Debug.Log("NOTEST");
        start.SetActive(false);
        Selection.SetActive(true);
        Debug.Log("YESTEST:");
    }

    public void SelectiontoPanther()
    {
        // Send request to server
        Debug.Log("HelloTEST");
        DoSelectiontoPantherServerRpc();

    }

    // SERVER ? validates & triggers transition for all clients
    [ServerRpc(RequireOwnership = false)]
    void DoSelectiontoPantherServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("RegisteredTEST");
        DoSelectiontoPantherClientRpc();
    }

    // CLIENT ? actually switches the UI on each machine
    [ClientRpc]
    void DoSelectiontoPantherClientRpc()
    {
        Debug.Log("NOTEST");
        Selection.SetActive(false);
        Panther.SetActive(true);
        Debug.Log("YESTEST:");
    }

    public void SelectiontoKnight()
    {
        // Send request to server
        Debug.Log("HelloTEST");
        DoSelectiontoKnightServerRpc();

    }

    // SERVER ? validates & triggers transition for all clients
    [ServerRpc(RequireOwnership = false)]
    void DoSelectiontoKnightServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("RegisteredTEST");
        DoSelectiontoKnightClientRpc();
    }

    // CLIENT ? actually switches the UI on each machine
    [ClientRpc]
    void DoSelectiontoKnightClientRpc()
    {
        Debug.Log("NOTEST");
        Selection.SetActive(false);
        Knight.SetActive(true);
        Debug.Log("YESTEST:");
    }

    public void SelectiontoShark()
    {
        // Send request to server
        Debug.Log("HelloTEST");
        DoSelectiontoSharkServerRpc();

    }

    // SERVER ? validates & triggers transition for all clients
    [ServerRpc(RequireOwnership = false)]
    void DoSelectiontoSharkServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("RegisteredTEST");
        DoSelectiontoSharkClientRpc();
    }

    // CLIENT ? actually switches the UI on each machine
    [ClientRpc]
    void DoSelectiontoSharkClientRpc()
    {
        Debug.Log("NOTEST");
        Selection.SetActive(false);
        Shark.SetActive(true);
        Debug.Log("YESTEST:");
    }
}

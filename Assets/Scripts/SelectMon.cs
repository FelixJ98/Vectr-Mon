using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class SelectMon : NetworkBehaviour
{
    [Header("Selection Scene")]
    public GameObject monsterPrefab; // MUST be a NetworkObject prefab
    public GameObject otherMon;
    public GameObject otherMon2;
    public GameObject backBtn;

    [Header("Confirmation")]
    public GameObject panel;
    public TextMeshProUGUI panelText;
    public Button checkBtn;
    public Button XBtn;

    [Header("Transition")]
    public GameObject currentCanvas;
    public GameObject nextCanvas;

    private GameObject selectedMon;

    public void Confirmation()
    {
        otherMon.SetActive(false);
        otherMon2.SetActive(false);
        backBtn.SetActive(false);

        panel.SetActive(true);
        panelText.text = "Chosen monster: " + gameObject.name;

        checkBtn.onClick.RemoveAllListeners(); // prevent double listeners
        checkBtn.onClick.AddListener(() =>
        {
            if (IsOwner) // Only owner triggers the server
                ConfirmCheckServerRpc();
        });

        XBtn.onClick.RemoveAllListeners();
        XBtn.onClick.AddListener(ConfirmX);
    }

    public void ConfirmX()
    {
        panel.SetActive(false);
        otherMon.SetActive(true);
        otherMon2.SetActive(true);
        backBtn.SetActive(true);
    }

    // SERVER RPC: called by the player who confirms
    [ServerRpc]
    private void ConfirmCheckServerRpc(ServerRpcParams rpcParams = default)
    {
        // Spawn the monster on the server (will propagate to clients)
        GameObject spawnedMon = Instantiate(monsterPrefab, nextCanvas.transform.position, nextCanvas.transform.rotation);
        spawnedMon.GetComponent<NetworkObject>().Spawn();

        // Tell all clients to switch canvas
        ExecuteCanvasTransitionClientRpc();
    }

    // CLIENT RPC: called on all clients
    [ClientRpc]
    private void ExecuteCanvasTransitionClientRpc(ClientRpcParams rpcParams = default)
    {
        currentCanvas.SetActive(false);
        nextCanvas.SetActive(true);
    }
}

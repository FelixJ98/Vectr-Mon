using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectMon : NetworkBehaviour
{
    [Header("Selection Scene")]
    public GameObject monster;
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
    private int curSelection = 0;

    // Called when X button pressed to cancel confirmation
    public void ConfirmX()
    {
        panel.SetActive(false);
        otherMon.SetActive(true);
        otherMon2.SetActive(true);
        backBtn.SetActive(true);
    }

    // Show confirmation panel
    public void Confirmation()
    {
        otherMon.SetActive(false);
        otherMon2.SetActive(false);
        backBtn.SetActive(false);

        string monsterName = monster.name;
        if (curSelection == 1) monsterName = otherMon.name;
        else if (curSelection == 2) monsterName = otherMon2.name;

        panel.SetActive(true);
        panelText.text = "Chosen monster: " + monsterName;

        checkBtn.onClick.RemoveAllListeners();
        checkBtn.onClick.AddListener(() => RequestSpawnMonsterServerRpc(curSelection));
        XBtn.onClick.RemoveAllListeners();
        XBtn.onClick.AddListener(ConfirmX);
    }

    // Called when a monster is selected
    public void SelectMonster(int i)
    {
        Debug.Log("[Vectormon] Selected Monster " + i);
        curSelection = i;
        Confirmation();
    }

    // ServerRpc called by any client to spawn a monster for all clients
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnMonsterServerRpc(int selection, ServerRpcParams rpcParams = default)
    {
        // Determine which monster to spawn
        GameObject mon = monster;
        if (selection == 1) mon = otherMon;
        else if (selection == 2) mon = otherMon2;

        // Instantiate monster on server
        var spawnedMon = Instantiate(mon, transform.position - TrackableManager.OFFSET, Quaternion.identity);
        spawnedMon.GetComponent<NetworkObject>().Spawn(); // Make it networked

        // Trigger canvas transition and assign monster on all clients
        SpawnMonsterClientRpc(spawnedMon.GetComponent<NetworkObject>().NetworkObjectId);
    }

    // ClientRpc runs on all clients to update UI and assign the spawned monster
    [ClientRpc]
    private void SpawnMonsterClientRpc(ulong networkObjectId)
    {
        // Update canvases for everyone
        panel.SetActive(false);
        currentCanvas.SetActive(false);
        nextCanvas.SetActive(true);

        // Assign spawned monster locally
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
        {
            selectedMon = netObj.gameObject;
            selectedMon.transform.rotation = Quaternion.identity;
            selectedMon.SetActive(true);
        }
    }
}

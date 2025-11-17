using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private Text statusText;

    private const float UPDATE_INTERVAL = 0.5f;
    private float updateTimer = 0f;
    private float waitingTimer = 0f;
    private bool hasShownWaitingMessage = false;

    private void Start()
    {
        if (statusText == null)
        {
            statusText = GetComponentInChildren<Text>();
        }

        if (statusText == null)
        {
            Debug.LogWarning("[ConnectionUI] No Text component found. Please assign statusText in the Inspector.");
            enabled = false;
            return;
        }

        UpdateStatusText();
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0f;
            UpdateStatusText();
        }

        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
        {
            waitingTimer += Time.deltaTime;
            if (waitingTimer > 5f && !hasShownWaitingMessage)
            {
                Debug.Log("[ConnectionUI] Waiting for matchmaking to connect... Check that Auto Matchmaking is properly configured.");
                hasShownWaitingMessage = true;
            }
        }
        else
        {
            waitingTimer = 0f;
            hasShownWaitingMessage = false;
        }
    }

    private void UpdateStatusText()
    {
        if (statusText == null) return;

        if (NetworkManager.Singleton == null)
        {
            statusText.text = "Network Manager: Not Found\nStatus: Offline";
            return;
        }

        NetworkManager netManager = NetworkManager.Singleton;
        string status = GetConnectionStatus(netManager);
        string clientInfo = GetClientInfo(netManager);
        string roomInfo = GetRoomInfo(netManager);
        string matchmakingInfo = GetMatchmakingInfo(netManager);

        statusText.text = $"{status}\n{clientInfo}\n{roomInfo}\n{matchmakingInfo}";
    }

    private string GetMatchmakingInfo(NetworkManager netManager)
    {
        if (netManager.IsListening)
        {
            return "";
        }

#if UNITY_EDITOR
        return "Info: Auto Matchmaking requires Quest device";
#else
        if (waitingTimer > 3f)
        {
            return $"Searching... ({Mathf.FloorToInt(waitingTimer)}s)";
        }
        return "Connecting...";
#endif
    }

    private string GetConnectionStatus(NetworkManager netManager)
    {
        if (!netManager.IsListening)
        {
            return "Status: Not Connected";
        }
        else if (netManager.IsServer && netManager.IsClient)
        {
            return "Status: Host (Server + Client)";
        }
        else if (netManager.IsServer)
        {
            return "Status: Server";
        }
        else if (netManager.IsClient)
        {
            return "Status: Client";
        }
        return "Status: Unknown";
    }

    private string GetClientInfo(NetworkManager netManager)
    {
        if (!netManager.IsListening)
        {
            return "Client ID: -";
        }
        return $"Client ID: {netManager.LocalClientId}";
    }

    private string GetRoomInfo(NetworkManager netManager)
    {
        if (!netManager.IsListening)
        {
            return "Players: 0";
        }

        if (!netManager.IsServer)
        {
            return "Players: Connected (Client)";
        }

        int playerCount = netManager.ConnectedClients.Count;
        string playerList = $"Players: {playerCount}";

        if (playerCount > 0 && playerCount <= 10)
        {
            playerList += " [";
            int count = 0;
            foreach (var clientId in netManager.ConnectedClientsIds)
            {
                if (count > 0) playerList += ", ";
                playerList += clientId;
                count++;
            }
            playerList += "]";
        }

        return playerList;
    }
}

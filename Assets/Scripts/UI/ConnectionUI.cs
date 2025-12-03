using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private Text statusText;

    private const float UPDATE_INTERVAL = 0.5f;
    private float updateTimer = 0f;
    private float waitingTimer = 0f;
    private bool hasShownWaitingMessage = false;

    // Cache application info to avoid repeated reflection calls
    private string cachedAppVersion;
    private string cachedBuildGUID;
    private string cachedOculusAppID;
    private uint cachedProtocolVersion;
    private bool hasLoggedConnectionInfo = false;

    private void Start()
    {
        if (statusText == null)
        {
            statusText = GetComponentInChildren<Text>();
        }

        if (statusText == null)
        {
            Debug.LogWarning("No Text component found. Please assign statusText in the Inspector.");
            enabled = false;
            return;
        }

        // Cache application information once at startup
        CacheApplicationInfo();
        
        // Log connection parameters for debugging
        LogConnectionParameters();
        
        UpdateStatusText();
    }

    private void CacheApplicationInfo()
    {
        // Get Unity application version
        cachedAppVersion = Application.version;
        
        // Get build GUID (unique per build) - this changes with each build
        cachedBuildGUID = Application.buildGUID;
        
        // Cache protocol version from NetworkManager if available
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            cachedProtocolVersion = NetworkManager.Singleton.NetworkConfig.ProtocolVersion;
        }
        
        // Try to get Oculus App ID from Resources
        try
        {
            var oculusSettings = Resources.Load<ScriptableObject>("OculusPlatformSettings");
            if (oculusSettings != null)
            {
                System.Type settingsType = oculusSettings.GetType();
                
                // Try to find the field - check both public and private (serialized fields are often private)
                FieldInfo appIdField = settingsType.GetField("ovrAppID", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (appIdField != null)
                {
                    var appIdValue = appIdField.GetValue(oculusSettings);
                    cachedOculusAppID = appIdValue?.ToString() ?? "N/A";
                }
                else
                {
                    // Try as a property instead
                    PropertyInfo appIdProperty = settingsType.GetProperty("ovrAppID", 
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (appIdProperty != null && appIdProperty.CanRead)
                    {
                        var appIdValue = appIdProperty.GetValue(oculusSettings);
                        cachedOculusAppID = appIdValue?.ToString() ?? "N/A";
                    }
                    else
                    {
                        // Try alternative field names
                        appIdField = settingsType.GetField("m_ovrAppID", 
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        
                        if (appIdField != null)
                        {
                            var appIdValue = appIdField.GetValue(oculusSettings);
                            cachedOculusAppID = appIdValue?.ToString() ?? "N/A";
                        }
                        else
                        {
                            // Log available fields for debugging (only in editor to avoid spam)
                            #if UNITY_EDITOR
                            var allFields = settingsType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            Debug.LogWarning($"Could not find ovrAppID field. Type: {settingsType.Name}. Available fields: {string.Join(", ", System.Array.ConvertAll(allFields, f => f.Name))}");
                            #endif
                            
                            // Fallback: Try to read from known value in this project
                            // This is a fallback - you can set this manually if reflection fails
                            cachedOculusAppID = "31777681461876531"; // Fallback value from OculusPlatformSettings.asset
                            Debug.Log($"Using fallback Oculus App ID: {cachedOculusAppID}");
                        }
                    }
                }
            }
            else
            {
                cachedOculusAppID = "Asset Not Found";
            }
        }
        catch (System.Exception ex)
        {
            cachedOculusAppID = $"Error: {ex.Message}";
            Debug.LogWarning($"Could not load Oculus App ID: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }

    private void LogConnectionParameters()
    {
        if (hasLoggedConnectionInfo) return;

        Debug.Log("=== Connection Parameters ===");
        Debug.Log($"Application Version: {cachedAppVersion}");
        Debug.Log($"Build GUID: {cachedBuildGUID}");
        Debug.Log($"Oculus App ID: {cachedOculusAppID}");
        Debug.Log($"Protocol Version: {cachedProtocolVersion}");
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            var config = NetworkManager.Singleton.NetworkConfig;
            Debug.Log($"NetworkConfig ProtocolVersion: {config.ProtocolVersion}");
            Debug.Log($"ForceSamePrefabs: {config.ForceSamePrefabs}");
            
            // Warning about protocol version
            if (config.ProtocolVersion == 0)
            {
                Debug.LogWarning("WARNING: Protocol Version is 0. Different builds may not be able to connect. " +
                    "Consider incrementing ProtocolVersion when making breaking network changes.");
            }
            
            // Warning about ForceSamePrefabs
            if (!config.ForceSamePrefabs)
            {
                Debug.LogWarning("WARNING: ForceSamePrefabs is disabled. Clients with different prefab versions may cause connection issues.");
            }
        }

        Debug.Log("=== End Connection Parameters ===");
        hasLoggedConnectionInfo = true;
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
                Debug.Log("Waiting for matchmaking to connect... Check that Auto Matchmaking is properly configured.");
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
        string appInfo = GetApplicationInfo(netManager);
        string protocolInfo = GetProtocolInfo(netManager);
        string transportInfo = GetTransportInfo(netManager);
        string tickRateInfo = GetTickRateInfo(netManager);

        statusText.text = $"{status}\n{clientInfo}\n{roomInfo}\n{matchmakingInfo}\n{appInfo}\n{protocolInfo}\n{transportInfo}\n{tickRateInfo}";
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

    private string GetApplicationInfo(NetworkManager netManager)
    {
        string info = $"App Version: {cachedAppVersion}";
        
        // Show build GUID (first 8 chars for readability)
        if (!string.IsNullOrEmpty(cachedBuildGUID))
        {
            string shortGUID = cachedBuildGUID.Length > 8 ? cachedBuildGUID.Substring(0, 8) : cachedBuildGUID;
            info += $"\nBuild GUID: {shortGUID}...";
        }
        
        // Show Oculus App ID
        if (!string.IsNullOrEmpty(cachedOculusAppID))
        {
            info += $"\nOculus App ID: {cachedOculusAppID}";
        }
        
        return info;
    }

    private string GetProtocolInfo(NetworkManager netManager)
    {
        if (netManager.NetworkConfig == null)
        {
            return "Protocol: Config Not Available";
        }

        uint protocolVersion = netManager.NetworkConfig.ProtocolVersion;
        string protocolText = $"Protocol Version: {protocolVersion}";
        
        // Add warning indicator if protocol version is 0
        if (protocolVersion == 0)
        {
            protocolText += " ⚠️ (May block cross-build connections)";
        }
        
        return protocolText;
    }

    private string GetTransportInfo(NetworkManager netManager)
    {
        if (netManager.NetworkConfig?.NetworkTransport == null)
        {
            return "Transport: Not Available";
        }

        var transport = netManager.NetworkConfig.NetworkTransport;
        string transportType = transport.GetType().Name;
        
        // Try to get connection data if available
        string connectionInfo = "";
        try
        {
            // Use reflection to access ConnectionData if it exists
            var connectionDataField = transport.GetType().GetField("ConnectionData", BindingFlags.Public | BindingFlags.Instance);
            if (connectionDataField != null)
            {
                var connectionData = connectionDataField.GetValue(transport);
                if (connectionData != null)
                {
                    var addressField = connectionData.GetType().GetField("Address", BindingFlags.Public | BindingFlags.Instance);
                    var portField = connectionData.GetType().GetField("Port", BindingFlags.Public | BindingFlags.Instance);
                    
                    if (addressField != null && portField != null)
                    {
                        string address = addressField.GetValue(connectionData)?.ToString() ?? "N/A";
                        string port = portField.GetValue(connectionData)?.ToString() ?? "N/A";
                        connectionInfo = $" ({address}:{port})";
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Could not read transport connection data: {ex.Message}");
        }

        return $"Transport: {transportType}{connectionInfo}";
    }

    /// <summary>
    /// Gets the current network tick rate information for display.
    /// </summary>
    private string GetTickRateInfo(NetworkManager netManager)
    {
        if (netManager == null || netManager.NetworkConfig == null)
        {
            return "Tick Rate: Not Available";
        }
        
        uint tickRate = netManager.NetworkConfig.TickRate;
        
        if (tickRate == 0)
        {
            return "Tick Rate: Not Available";
        }
        
        string tickRateText = $"Tick Rate: {tickRate} Hz";
        
        // Add recommendation indicator for VR
        if (tickRate < 30)
        {
            tickRateText += " ⚠️ (Very Low for VR, recommend 30+ Hz)";
        }
        else if (tickRate >= 30 && tickRate < 60)
        {
            tickRateText += " ✓ (Acceptable for VR, 60+ Hz recommended)";
        }
        else if (tickRate >= 60 && tickRate < 90)
        {
            tickRateText += " ✓ (Good for VR)";
        }
        else if (tickRate >= 90)
        {
            tickRateText += " ✓✓ (Optimal for VR)";
        }
        
        return tickRateText;
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

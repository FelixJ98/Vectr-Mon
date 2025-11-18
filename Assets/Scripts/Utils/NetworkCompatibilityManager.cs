using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages network compatibility settings to ensure builds can connect to each other.
/// This script enforces a shared compatibility version and can override Protocol Version
/// to ensure cross-build connectivity.
/// </summary>
public class NetworkCompatibilityManager : MonoBehaviour
{
    [Header("Compatibility Settings")]
    [Tooltip("Custom compatibility version string. Set this to the same value across all builds to ensure compatibility.")]
    [SerializeField] private string compatibilityVersion = "1.0.0";
    
    [Tooltip("Force Protocol Version. If enabled, will override NetworkConfig ProtocolVersion at runtime.")]
    [SerializeField] private bool forceProtocolVersion = true;
    
    [Tooltip("Protocol Version to enforce. All builds should use the same value.")]
    [SerializeField] private ushort enforcedProtocolVersion = 1;
    
    [Tooltip("Log compatibility information at startup")]
    [SerializeField] private bool logCompatibilityInfo = true;
    
    private static NetworkCompatibilityManager instance;
    public static NetworkCompatibilityManager Instance => instance;
    
    public string CompatibilityVersion => compatibilityVersion;
    public ushort EnforcedProtocolVersion => enforcedProtocolVersion;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            DebugTag.LogWarning(nameof(NetworkCompatibilityManager), "Multiple instances detected. Destroying duplicate.");
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        ApplyCompatibilitySettings();
    }
    
    /// <summary>
    /// Applies compatibility settings to the NetworkManager.
    /// This ensures all builds use the same protocol version for connectivity.
    /// </summary>
    private void ApplyCompatibilitySettings()
    {
        if (NetworkManager.Singleton == null)
        {
            DebugTag.LogWarning(nameof(NetworkCompatibilityManager), "NetworkManager.Singleton is null. Cannot apply compatibility settings.");
            return;
        }
        
        if (NetworkManager.Singleton.NetworkConfig == null)
        {
            DebugTag.LogWarning(nameof(NetworkCompatibilityManager), "NetworkConfig is null. Cannot apply compatibility settings.");
            return;
        }
        
        var config = NetworkManager.Singleton.NetworkConfig;
        ushort originalProtocolVersion = config.ProtocolVersion;
        
        // Force protocol version if enabled
        if (forceProtocolVersion)
        {
            config.ProtocolVersion = enforcedProtocolVersion;
            
            if (logCompatibilityInfo)
            {
                DebugTag.Log(nameof(NetworkCompatibilityManager), $"Protocol Version overridden: {originalProtocolVersion} -> {enforcedProtocolVersion}");
            }
        }
        
        if (logCompatibilityInfo)
        {
            LogCompatibilityInfo();
        }
    }
    
    /// <summary>
    /// Logs all compatibility information for debugging.
    /// </summary>
    private void LogCompatibilityInfo()
    {
        DebugTag.Log(nameof(NetworkCompatibilityManager), "=== Compatibility Information ===");
        DebugTag.Log(nameof(NetworkCompatibilityManager), $"Compatibility Version: {compatibilityVersion}");
        DebugTag.Log(nameof(NetworkCompatibilityManager), $"Force Protocol Version: {forceProtocolVersion}");
        DebugTag.Log(nameof(NetworkCompatibilityManager), $"Enforced Protocol Version: {enforcedProtocolVersion}");
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            var config = NetworkManager.Singleton.NetworkConfig;
            DebugTag.Log(nameof(NetworkCompatibilityManager), $"Current NetworkConfig ProtocolVersion: {config.ProtocolVersion}");
            DebugTag.Log(nameof(NetworkCompatibilityManager), $"Application Build GUID: {Application.buildGUID}");
            DebugTag.Log(nameof(NetworkCompatibilityManager), $"Application Version: {Application.version}");
        }
        
        DebugTag.Log(nameof(NetworkCompatibilityManager), "=== End Compatibility Information ===");
    }
    
    /// <summary>
    /// Validates that the compatibility version matches between this instance and a remote instance.
    /// </summary>
    /// <param name="remoteCompatibilityVersion">The compatibility version from the remote client</param>
    /// <returns>True if versions match, false otherwise</returns>
    public bool ValidateCompatibilityVersion(string remoteCompatibilityVersion)
    {
        bool matches = compatibilityVersion == remoteCompatibilityVersion;
        
        if (!matches)
        {
            DebugTag.LogWarning(nameof(NetworkCompatibilityManager), $"Compatibility version mismatch! " +
                $"Local: {compatibilityVersion}, Remote: {remoteCompatibilityVersion}");
        }
        
        return matches;
    }
    
    /// <summary>
    /// Sets the compatibility version at runtime.
    /// </summary>
    /// <param name="version">The compatibility version string</param>
    public void SetCompatibilityVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            DebugTag.LogWarning(nameof(NetworkCompatibilityManager), "Cannot set empty compatibility version.");
            return;
        }
        
        compatibilityVersion = version;
        DebugTag.Log(nameof(NetworkCompatibilityManager), $"Compatibility version set to: {compatibilityVersion}");
    }
    
    /// <summary>
    /// Sets the enforced protocol version at runtime.
    /// </summary>
    /// <param name="protocolVersion">The protocol version to enforce</param>
    public void SetEnforcedProtocolVersion(ushort protocolVersion)
    {
        enforcedProtocolVersion = protocolVersion;
        
        if (forceProtocolVersion && NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            NetworkManager.Singleton.NetworkConfig.ProtocolVersion = enforcedProtocolVersion;
            DebugTag.Log(nameof(NetworkCompatibilityManager), $"Protocol version set to: {enforcedProtocolVersion}");
        }
    }
    
    /// <summary>
    /// Gets a formatted compatibility report for display in UI.
    /// </summary>
    public string GetCompatibilityReport()
    {
        string report = $"Compatibility Version: {compatibilityVersion}\n";
        report += $"Protocol Version: {enforcedProtocolVersion}";
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            ushort currentProtocol = NetworkManager.Singleton.NetworkConfig.ProtocolVersion;
            if (currentProtocol != enforcedProtocolVersion && forceProtocolVersion)
            {
                report += $" (Enforced: {enforcedProtocolVersion})";
            }
        }
        
        return report;
    }
}


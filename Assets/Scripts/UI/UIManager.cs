using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

/// <summary>
/// UIManager - Central controller for managing UI flow in the Meta Quest game.
/// Flow: Main Menu → Character Selection → BattleField Menu
/// Attach this to a persistent GameObject in the scene.
/// Assign the three canvas GameObjects in the Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Canvas References")]
    [Tooltip("Main Menu Canvas - First screen shown when game starts")]
    public GameObject mainMenuCanvas;

    [Tooltip("Character Selection Canvas - Player chooses their character")]
    public GameObject selectionCanvas;

    [Tooltip("BattleField Menu Canvas - Combat interface")]
    public GameObject battleFieldMenuCanvas;

    [Header("Character Spawning")]
    [Tooltip("Character A prefab to instantiate when character selection is confirmed. Must have a NetworkObject component.")]
    [SerializeField]
    private NetworkObject characterA;

    [Tooltip("Spawn position offset relative to this GameObject's transform. If not set, will spawn at this transform's position.")]
    [SerializeField]
    private Vector3 spawnPositionOffset = Vector3.zero;

    [Tooltip("Spawn rotation offset relative to this GameObject's transform rotation. If not set, will use this transform's rotation.")]
    [SerializeField]
    private Vector3 spawnRotationOffset = Vector3.zero;


    // Singleton pattern for easy access from other scripts
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        // Implement the Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Optionally persist across scenes
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Initial state: Start with the Main Menu visible
        ShowMainMenu();
    }

    private void Update()
    {
        // Rotate active canvas to look at the main camera
        if (Camera.main != null)
        {
            UpdateCanvasRotation();
        }
    }

    // ===== Public Canvas Control Methods =====
    // Other scripts can call these via UIManager.Instance.ShowMainMenu() etc.

    /// <summary>
    /// Show Main Menu - First screen when game starts
    /// </summary>
    public void ShowMainMenu()
    {
        SetActiveCanvas(mainMenuCanvas);
        DebugTag.Log(nameof(UIManager), "Main Menu displayed");
    }

    /// <summary>
    /// Show Character Selection Menu
    /// Called when player clicks "Start" or "Play" from Main Menu
    /// </summary>
    public void ShowSelectionMenu()
    {
        SetActiveCanvas(selectionCanvas);
        DebugTag.Log(nameof(UIManager), "Character Selection displayed");
    }

    /// <summary>
    /// Show BattleField Menu
    /// Called after character selection is complete
    /// </summary>
    public void ShowBattleFieldMenu()
    {
        SetActiveCanvas(battleFieldMenuCanvas);
        DebugTag.Log(nameof(UIManager), "BattleField Menu displayed");
    }

    // ===== Helper Methods =====

    /// <summary>
    /// Helper to activate one canvas and deactivate all others
    /// </summary>
    private void SetActiveCanvas(GameObject activeCanvas)
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(mainMenuCanvas == activeCanvas);

        if (selectionCanvas != null)
            selectionCanvas.SetActive(selectionCanvas == activeCanvas);

        if (battleFieldMenuCanvas != null)
            battleFieldMenuCanvas.SetActive(battleFieldMenuCanvas == activeCanvas);
    }

    // ===== Button Handler Methods =====
    // Wire these to UI buttons in Unity Inspector

    /// <summary>
    /// Called by "Start/Play" button on Main Menu
    /// Transitions to Character Selection
    /// </summary>
    public void OnStartButtonClicked()
    {
        ShowSelectionMenu();
    }

    /// <summary>
    /// Called when character selection is confirmed
    /// Transitions to BattleField Menu and spawns the selected character as a NetworkObject
    /// </summary>
    public void OnCharacterConfirmed()
    {
        ShowBattleFieldMenu();
        
        // Spawn character as NetworkObject
        SpawnCharacterA();
    }

    /// <summary>
    /// Spawns Character A as a networked object. Only the server can spawn NetworkObjects.
    /// </summary>
    private void SpawnCharacterA()
    {
        // Check if NetworkManager is available
        if (NetworkManager.Singleton == null)
        {
            DebugTag.LogWarning(nameof(UIManager), "Cannot spawn character: NetworkManager.Singleton is null. " +
                           "Ensure NetworkManager is present in the scene.");
            return;
        }

        // Only server can spawn NetworkObjects
        if (!NetworkManager.Singleton.IsServer)
        {
            DebugTag.LogWarning(nameof(UIManager), "Cannot spawn character: Only the server can spawn NetworkObjects. " +
                           $"Current role: {(NetworkManager.Singleton.IsClient ? "Client" : "Not Connected")}");
            return;
        }

        // Validate character prefab
        if (characterA == null)
        {
            DebugTag.LogWarning(nameof(UIManager), "Cannot spawn character: characterA prefab is not assigned. " +
                           "Please assign a NetworkObject prefab in the Inspector.");
            return;
        }

        // Calculate world position relative to this transform
        Vector3 worldSpawnPosition = transform.TransformPoint(spawnPositionOffset);
        
        // Calculate world rotation relative to this transform
        Quaternion worldSpawnRotation = transform.rotation * Quaternion.Euler(spawnRotationOffset);

        // Instantiate the character prefab at the calculated world position and rotation
        NetworkObject instance = Instantiate(characterA, worldSpawnPosition, worldSpawnRotation);
        
        // Spawn with server ownership (no client ownership)
        instance.SpawnWithOwnership(NetworkManager.ServerClientId);

        DebugTag.Log(nameof(UIManager), $"Character A spawned at world position {worldSpawnPosition} (local offset: {spawnPositionOffset}) " +
                   $"with rotation {worldSpawnRotation.eulerAngles} (local offset: {spawnRotationOffset}). " +
                   $"NetworkObjectId: {instance.NetworkObjectId}");
    }

    /// <summary>
    /// Called by "Back" buttons to return to Main Menu
    /// </summary>
    public void OnBackToMainMenu()
    {
        ShowMainMenu();
    }

    // ===== Utility Methods for Other Developers =====

    /// <summary>
    /// Check which canvas is currently active
    /// </summary>
    public bool IsMainMenuActive() => mainMenuCanvas != null && mainMenuCanvas.activeSelf;
    public bool IsSelectionActive() => selectionCanvas != null && selectionCanvas.activeSelf;
    public bool IsBattleFieldActive() => battleFieldMenuCanvas != null && battleFieldMenuCanvas.activeSelf;

    /// <summary>
    /// Rotates the active canvas to look at the main camera
    /// </summary>
    private void UpdateCanvasRotation()
    {
        GameObject activeCanvas = GetActiveCanvas();
        if (activeCanvas == null) return;

        Transform canvasTransform = activeCanvas.transform;
        Transform cameraTransform = Camera.main.transform;

        // Rotate to face the camera
        canvasTransform.LookAt(cameraTransform.position);
        canvasTransform.Rotate(0, 180, 0); // Flip to face camera
    }

    /// <summary>
    /// Gets the currently active canvas
    /// </summary>
    private GameObject GetActiveCanvas()
    {
        if (mainMenuCanvas != null && mainMenuCanvas.activeSelf)
            return mainMenuCanvas;
        if (selectionCanvas != null && selectionCanvas.activeSelf)
            return selectionCanvas;
        if (battleFieldMenuCanvas != null && battleFieldMenuCanvas.activeSelf)
            return battleFieldMenuCanvas;
        return null;
    }
}
using UnityEngine;

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

    // Singleton pattern for easy access from other scripts
    public UIManager Instance { get; private set; }

    //private void Awake()
    //{
    //    // Implement the Singleton pattern
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }

    //    Instance = this;
    //    // Optionally persist across scenes
    //    // DontDestroyOnLoad(gameObject);
    //}

    private void Start()
    {
        // Initial state: Start with the Main Menu visible
        ShowMainMenu();
    }

    // ===== Public Canvas Control Methods =====
    // Other scripts can call these via UIManager.Instance.ShowMainMenu() etc.

    /// <summary>
    /// Show Main Menu - First screen when game starts
    /// </summary>
    public void ShowMainMenu()
    {
        SetActiveCanvas(mainMenuCanvas);
        Debug.Log("UIManager: Main Menu displayed");
    }

    /// <summary>
    /// Show Character Selection Menu
    /// Called when player clicks "Start" or "Play" from Main Menu
    /// </summary>
    public void ShowSelectionMenu()
    {
        SetActiveCanvas(selectionCanvas);
        Debug.Log("UIManager: Character Selection displayed");
    }

    /// <summary>
    /// Show BattleField Menu
    /// Called after character selection is complete
    /// </summary>
    public void ShowBattleFieldMenu()
    {
        SetActiveCanvas(battleFieldMenuCanvas);
        Debug.Log("UIManager: BattleField Menu displayed");
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
    /// Transitions to BattleField Menu
    /// TODO: Pass selected character data to battle system
    /// </summary>
    public void OnCharacterConfirmed()
    {
        ShowBattleFieldMenu();
        // TODO: Initialize battle with selected characters
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
}
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BattleFieldMenu
/// - Shows a UI panel in front of the player's camera at startup (for MR/VR like Meta Quest).
/// - Clean, modular structure for later expansion (Attack / Grab / Block logic and button handlers).
/// - Keep this script on a scene object and assign the Panel (RectTransform/Canvas) in the Inspector.
/// </summary>
public class BattleFieldMenu : MonoBehaviour
{
    [Header("References")]
    [Tooltip("World-space panel or any RectTransform under a Canvas to show in front of the player.")]
    [SerializeField] private RectTransform panel;

    [Tooltip("Optional override for the camera transform (e.g., XR Camera). If not set, uses Camera.main.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Placement")]
    [Tooltip("Distance (meters) in front of the camera to place the panel.")]
    [SerializeField] private float distanceFromCamera = 1.0f;

    [Tooltip("Local offset applied after positioning in front of the camera (meters). Useful for slight vertical offsets.")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, -0.05f, 0f);

    [Tooltip("Rotate the panel so it faces the camera (yaw-only).")]
    [SerializeField] private bool faceCamera = true;

    [Header("Behavior")]
    [Tooltip("Automatically show and position the panel on Start.")]
    [SerializeField] private bool showOnStart = true;

    // Simple RPS-like choices scaffold for future expansion
    public enum Choice { None, Attack, Grab, Block }

    private Choice _playerChoice = Choice.None;

    private void Awake()
    {
        // Resolve camera transform if not assigned
        if (cameraTransform == null)
        {
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
            else
            {
                // Fallback: try to find any camera
                var anyCam = FindAnyObjectByType<Camera>();
                if (anyCam != null) cameraTransform = anyCam.transform;
            }
        }
    }

    private void Start()
    {
        if (showOnStart)
        {
            ShowPanelInFront();
        }
    }

    /// <summary>
    /// Shows and positions the panel in front of the camera.
    /// </summary>
    public void ShowPanelInFront()
    {
        if (panel == null)
        {
            DebugTag.LogWarning(nameof(BattleFieldMenu), "Panel reference is not set. Assign a RectTransform in the Inspector.");
            return;
        }

        // Ensure active
        if (!panel.gameObject.activeSelf)
            panel.gameObject.SetActive(true);

        PositionAndFacePanel();
    }

    /// <summary>
    /// Hides the panel.
    /// </summary>
    public void HidePanel()
    {
        if (panel != null && panel.gameObject.activeSelf)
        {
            panel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles panel visibility.
    /// </summary>
    public void TogglePanel()
    {
        if (panel == null) return;
        panel.gameObject.SetActive(!panel.gameObject.activeSelf);
        if (panel.gameObject.activeSelf)
        {
            PositionAndFacePanel();
        }
    }

    private void PositionAndFacePanel()
    {
        if (panel == null || cameraTransform == null) return;

        // Horizontal forward (ignore pitch/roll) for stable placement
        Vector3 forwardFlat = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        if (forwardFlat.sqrMagnitude < 0.0001f)
        {
            forwardFlat = cameraTransform.forward;
        }

        Vector3 targetPos = cameraTransform.position + forwardFlat * Mathf.Max(0.1f, distanceFromCamera) + positionOffset;
        panel.position = targetPos;

        if (faceCamera)
        {
            // Face the camera with yaw-only look
            Vector3 toCameraFlat = Vector3.ProjectOnPlane(cameraTransform.position - panel.position, Vector3.up);
            if (toCameraFlat.sqrMagnitude > 0.0001f)
            {
                panel.rotation = Quaternion.LookRotation(toCameraFlat.normalized, Vector3.up);
            }
            else
            {
                panel.forward = -forwardFlat; // fallback
            }
        }
    }

    // ===== Combat Action Handlers =====
    // Rock-Paper-Scissors Logic: Attack beats Grab, Grab beats Block, Block beats Attack

    /// <summary>
    /// Called when the Attack button is clicked.
    /// Attack beats Grab.
    /// </summary>
    public void OnAttackClicked()
    {
        DebugTag.Log(nameof(BattleFieldMenu), "ATTACK button clicked! Attack beats Grab.");
        _playerChoice = Choice.Attack;
        // TODO: Call BattleSystem.Attack() or notify BattleManager
    }

    /// <summary>
    /// Called when the Grab button is clicked.
    /// Grab beats Block.
    /// </summary>
    public void OnGrabClicked()
    {
        DebugTag.Log(nameof(BattleFieldMenu), "GRAB button clicked! Grab beats Block.");
        _playerChoice = Choice.Grab;
        // TODO: Call BattleSystem.Grab() or notify BattleManager
    }

    /// <summary>
    /// Called when the Block button is clicked.
    /// Block beats Attack.
    /// </summary>
    public void OnBlockClicked()
    {
        DebugTag.Log(nameof(BattleFieldMenu), "BLOCK button clicked! Block beats Attack.");
        _playerChoice = Choice.Block;
        // TODO: Call BattleSystem.Block() or notify BattleManager
    }

    /// <summary>
    /// Compares the player's choice with the opponent's choice.
    /// Returns: 1 = player wins, 0 = draw, -1 = player loses.
    /// Attack beats Grab, Grab beats Block, Block beats Attack.
    /// </summary>
    public static int CompareChoices(Choice player, Choice opponent)
    {
        if (player == opponent) return 0;
        if (player == Choice.None || opponent == Choice.None) return 0;

        switch (player)
        {
            case Choice.Attack:
                return opponent == Choice.Grab ? 1 : -1; // Attack beats Grab, loses to Block
            case Choice.Grab:
                return opponent == Choice.Block ? 1 : -1; // Grab beats Block, loses to Attack
            case Choice.Block:
                return opponent == Choice.Attack ? 1 : -1; // Block beats Attack, loses to Grab
            default:
                return 0;
        }
    }
}

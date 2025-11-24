using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BattleUIController: Updates UI, listens for player input, and communicates with BattleManager and BattleFieldMenu.
/// Attach to a UI GameObject.
/// </summary>
public class BattleUIController : MonoBehaviour
{
    public BattleManager battleManager;
    public BattleFieldMenu battleFieldMenu;
    public Text player1HealthText;
    public Text player2HealthText;
    public Text resultText;

    public int localPlayerIndex = 1; // 1 or 2 (for multiplayer/AI extension)

    private void Start()
    {
        UpdateHealthUI();
        if (battleManager != null)
        {
            battleManager.player1.onHealthChanged.AddListener(UpdateHealthUI);
            battleManager.player2.onHealthChanged.AddListener(UpdateHealthUI);
            battleManager.onBattleEnd += OnBattleEnd;
        }
    }

    public void OnAttackButton()
    {
        battleManager.PlayerSelectMove(localPlayerIndex, BattleFieldMenu.Choice.Attack);
    }
    public void OnGrabButton()
    {
        battleManager.PlayerSelectMove(localPlayerIndex, BattleFieldMenu.Choice.Grab);
    }
    public void OnBlockButton()
    {
        battleManager.PlayerSelectMove(localPlayerIndex, BattleFieldMenu.Choice.Block);
    }

    public void UpdateHealthUI()
    {
        if (player1HealthText) player1HealthText.text = $"P1 HP: {battleManager.player1.currentHealth}";
        if (player2HealthText) player2HealthText.text = $"P2 HP: {battleManager.player2.currentHealth}";
    }

    public void OnBattleEnd(int winnerIndex)
    {
        if (resultText)
        {
            resultText.text = $"Player {winnerIndex} Wins!";
        }
        // Optionally disable input/buttons here
    }
}

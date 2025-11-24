using UnityEngine;
using System.Collections;

/// <summary>
/// BattleManager: Handles player turns, move resolution, health updates, and win condition.
/// Attach to a central GameObject in the scene.
/// </summary>
public class BattleManager : MonoBehaviour
{
    public Character player1;
    public Character player2;

    public BattleFieldMenu.Choice player1Choice = BattleFieldMenu.Choice.None;
    public BattleFieldMenu.Choice player2Choice = BattleFieldMenu.Choice.None;

    public bool roundInProgress = false;

    public delegate void OnBattleEnd(int winnerIndex);
    public event OnBattleEnd onBattleEnd;

    public void PlayerSelectMove(int playerIndex, BattleFieldMenu.Choice choice)
    {
        if (playerIndex == 1) player1Choice = choice;
        else if (playerIndex == 2) player2Choice = choice;

        if (player1Choice != BattleFieldMenu.Choice.None && player2Choice != BattleFieldMenu.Choice.None)
        {
            StartCoroutine(ResolveRound());
        }
    }

    private IEnumerator ResolveRound()
    {
        roundInProgress = true;
        // Play move animations
        player1.PlayMoveAnimation(player1Choice);
        player2.PlayMoveAnimation(player2Choice);
        yield return new WaitForSeconds(1.0f); // Wait for animation

        int result = BattleFieldMenu.CompareChoices(player1Choice, player2Choice);
        if (result == 1)
        {
            player2.TakeDamage(player1.attackPower);
        }
        else if (result == -1)
        {
            player1.TakeDamage(player2.attackPower);
        }
        // else draw, no damage

        // Check for win
        if (player1.currentHealth <= 0)
        {
            onBattleEnd?.Invoke(2);
        }
        else if (player2.currentHealth <= 0)
        {
            onBattleEnd?.Invoke(1);
        }

        // Reset for next round
        player1Choice = BattleFieldMenu.Choice.None;
        player2Choice = BattleFieldMenu.Choice.None;
        roundInProgress = false;
    }

    public void ResetBattle()
    {
        player1.ResetCharacter();
        player2.ResetCharacter();
        player1Choice = BattleFieldMenu.Choice.None;
        player2Choice = BattleFieldMenu.Choice.None;
        roundInProgress = false;
    }
}

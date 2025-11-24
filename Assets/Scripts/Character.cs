using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Character: Holds stats, health, and animation logic for a battle participant.
/// Attach this to each player/enemy GameObject.
/// </summary>
public class Character : MonoBehaviour
{
    [Header("Stats")]
    public string characterName;
    public int maxHealth = 100;
    public int currentHealth;
    public int attackPower = 20;

    [Header("Model & Animation")]
    public GameObject modelPrefab; // Assign 3D model prefab in Inspector
    public Animator animator;      // Assign Animator in Inspector (from modelPrefab)

    [Header("Events")]
    public UnityEvent onHealthChanged;
    public UnityEvent onDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        onHealthChanged?.Invoke();
        if (currentHealth <= 0)
        {
            onDeath?.Invoke();
        }
    }

    public void PlayMoveAnimation(BattleFieldMenu.Choice move)
    {
        if (animator == null) return;
        switch (move)
        {
            case BattleFieldMenu.Choice.Attack:
                animator.SetTrigger("Attack");
                break;
            case BattleFieldMenu.Choice.Grab:
                animator.SetTrigger("Grab");
                break;
            case BattleFieldMenu.Choice.Block:
                animator.SetTrigger("Block");
                break;
        }
    }

    public void ResetCharacter()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke();
    }
}

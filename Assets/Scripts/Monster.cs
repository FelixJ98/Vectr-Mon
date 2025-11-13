using UnityEngine;

public class Monster : MonoBehaviour
{
    [Tooltip("ID for debugging / identification")]
    public int monID = 0;

    [Tooltip("Prefab to instantiate when this choice is selected (the real in-world monster).")]
    public GameObject monsterPrefab;

    // Reference to the parent manager (auto-resolved on Awake)
    private SelectMon manager;

    private bool chosen = false;

    private void Awake()
    {
        // Try to find the manager in parents (assumes UI choices are children of the manager prefab)
        manager = GetComponentInParent<SelectMon>();
        if (manager == null)
        {
            Debug.LogWarning($"Monster (id {monID}) couldn't find MonsterSelectManager in parents.");
        }
    }

    // This method will be called either via trigger or a UnityEvent from the poke component.
    // Keep it public so you can hook it up directly to poke's UnityEvent if needed.
    public void Select()
    {
        if (chosen) return;
        chosen = true;

        Debug.Log($"Monster choice {monID} selected.");

        // Notify manager (safe null-check)
        if (manager != null)
        {
            manager.OnChoiceSelected(monID, monsterPrefab, this.gameObject);
        }
        else
        {
            // fallback: deactivate this choice and store chosen prefab for other logic
            gameObject.SetActive(false);
        }
    }

    // If you prefer Collider trigger based selection (older approach), call Select() from OnTriggerEnter
    private void OnTriggerEnter(Collider other)
    {
        // optionally check tag/layer of "other" if you only want specific colliders to select
        Select();
    }
}
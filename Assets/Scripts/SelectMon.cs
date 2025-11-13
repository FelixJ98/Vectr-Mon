using UnityEngine;

public class SelectMon : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject[] monsters;

    public static GameObject SelectedMonster { get; private set; }

    // Called by XR button
    public void SelectMonster(int index)
    {
        if (index < 0 || index >= monsters.Length)
        {
            Debug.LogWarning("Invalid monster index!");
            return;
        }

        SelectedMonster = monsters[index];
        Debug.Log($"Selected Monster: {SelectedMonster.name}");
    }

}
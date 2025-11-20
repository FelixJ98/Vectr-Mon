using UnityEngine;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private UIManager uiManager;

    private GameObject[] selections = new GameObject[3];
    private int count = 0;

    public void AddMonster(GameObject monster)
    {
        if (count >= 3) return;

        selections[count] = monster;
        count++;

        UpdateDisplay();

        if (count == 3)
            uiManager.ShowFinalScreen();
    }

    private void UpdateDisplay()
    {
        displayText.text = $"1. {(selections[0] ? selections[0].name : "---")}\n" +
                          $"2. {(selections[1] ? selections[1].name : "---")}\n" +
                          $"3. {(selections[2] ? selections[2].name : "---")}";
    }

    public GameObject[] GetSelections() => selections;
}
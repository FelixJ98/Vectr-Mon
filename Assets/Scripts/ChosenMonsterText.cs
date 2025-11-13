using UnityEngine;
using TMPro;

public class ChosenMonsterText : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    private void OnEnable()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (SelectMon.SelectedMonster != null)
        {
            displayText.text = $"Player Chose: {SelectMon.SelectedMonster.name}";
        }
        else
        {
            displayText.text = "Player Chose: (none)";
        }
    }
}

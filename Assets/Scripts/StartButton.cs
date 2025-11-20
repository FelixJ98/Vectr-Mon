using UnityEngine;

public class StartButton : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    public void OnButtonClicked()
    {
        Debug.Log("=== START BUTTON CLICKED ===");

        if (uiManager == null)
        {
            Debug.LogError("UIManager is NULL!");
            return;
        }

        Debug.Log("Calling ShowMonsterSelect...");
        uiManager.ShowMonsterSelect();
    }
}
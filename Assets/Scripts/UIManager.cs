using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject monsterSelect;
    [SerializeField] private GameObject finalScreen;

    private void Start() => ShowMainMenu();

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        monsterSelect.SetActive(false);
        finalScreen.SetActive(false);
    }

    public void ShowMonsterSelect()
    {
        Debug.Log("=== ShowMonsterSelect CALLED ===");
        Debug.Log($"MainMenu: {mainMenu}, MonsterSelect: {monsterSelect}, FinalScreen: {finalScreen}");

        mainMenu.SetActive(false);
        monsterSelect.SetActive(true);
        finalScreen.SetActive(false);
    }

    public void ShowFinalScreen()
    {
        mainMenu.SetActive(false);
        monsterSelect.SetActive(false);
        finalScreen.SetActive(true);
    }
}
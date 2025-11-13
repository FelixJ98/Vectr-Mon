using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Pages")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject monsterSelect;
    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        monsterSelect.SetActive(false);
    }

    public void ShowMonsterSelect()
    {
        mainMenu.SetActive(false);
        monsterSelect.SetActive(true);
    }

    public void ShowBattleUI()
    {
        mainMenu.SetActive(false);
        monsterSelect.SetActive(false);
    }
}

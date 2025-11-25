using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // for Test mesh printing
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using Button = UnityEngine.UI.Button; 

public class SelectMon : MonoBehaviour
{
    public GameObject monster;
    public GameObject otherMon;
    public GameObject otherMon2;
    public GameObject backBtn;
    public GameObject panel;
    public TextMeshProUGUI panelText;
    public Button checkBtn;
    public Button XBtn;
    public GameObject currentCanvas;
    public GameObject nextCanvas;
    GameObject selectedMon;

    // Chooses selected monster
    public void ConfirmCheck() // Check button pressed
    {
        SelectMonster();
    }

    // Unselects monster
    public void ConfirmX() // X button pressed
    {
        // remove confirmation panel and reenable selection buttons
        panel.SetActive(false);
        otherMon.SetActive(true);
        otherMon2.SetActive(true);
        backBtn.SetActive(true);
    }

    // Shows monster confirmation
    public void Confirmation()
    {
        // Disable other selection buttons
        otherMon.SetActive(false);
        otherMon2.SetActive(false);
        backBtn.SetActive(false);

        // show confirmation screen
        panel.SetActive(true);
        panelText.text = "Chosen monster: " + gameObject.name;

        checkBtn.onClick.AddListener(ConfirmCheck); // wait for check
        XBtn.onClick.AddListener(ConfirmX); // wait for X
    }

   // Occurrs when monster is selected and is ready to move to battle scene
    public void SelectMonster()
    {
        currentCanvas.SetActive(false); // disable current canvas
        nextCanvas.SetActive(true); // enable next canvas
        selectedMon = Instantiate(monster, nextCanvas.transform.position, nextCanvas.transform.rotation); // spawn monster in next canvas
    }
}

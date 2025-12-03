using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // for Test mesh printing
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using Button = UnityEngine.UI.Button; 

public class SelectMon : MonoBehaviour
{
    [Header("Selection Scene")]
    public GameObject monster;
    public GameObject otherMon;
    public GameObject otherMon2;
    public GameObject backBtn;
    [Header("Confirmation")]
    public GameObject panel;
    public TextMeshProUGUI panelText;
    public Button checkBtn;
    public Button XBtn;
    [Header("Transition")]
    public GameObject currentCanvas;
    public GameObject nextCanvas;
    GameObject selectedMon;

    private int curSelection = 0;

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
        string monsterName = monster.name;
        if (curSelection == 1)
        {
            monsterName = otherMon.name;
        }
        else if (curSelection == 2)
        {
            monsterName = otherMon2.name;
        }
        panel.SetActive(true);
        panelText.text = "Chosen monster: " + gameObject.name;

        checkBtn.onClick.AddListener(SpawnMonster); // wait for check
        XBtn.onClick.AddListener(ConfirmX); // wait for X
    }

   // Occurrs when monster is selected and is ready to move to battle scene
    public void SelectMonster(int i)
    {
        Debug.Log("[Vectormon] Selected Monster " + i);
         
        curSelection = i;
        Confirmation();
    }

    public void SpawnMonster()
    {
        Debug.Log("[Vectormon] Spawning Monster");

        panel.SetActive(false);
        currentCanvas.SetActive(false); // disable current canvas
        nextCanvas.SetActive(true); // enable next canvas

        GameObject mon = monster;
        if (curSelection == 1)
        {
            mon = otherMon;
        }
        else if (curSelection == 2)
        {
            mon = otherMon2;
        }
        
        selectedMon = Instantiate(mon, transform.position - TrackableManager.OFFSET, Quaternion.identity, transform.parent); // spawn monster in next canvas
        selectedMon.transform.rotation = Quaternion.identity;
        selectedMon.gameObject.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class SelectMon : MonoBehaviour
{
    public GameObject monster;
    public GameObject currentCanvas;
    public GameObject nextCanvas;
    GameObject selectedMon;

    // swaps scene and spawns monster when clicked
    public void OnButtonClick()
    {
        currentCanvas.SetActive(false); // deactivate selection scene
        nextCanvas.SetActive(true); // activate battle scene
        selectedMon = Instantiate(monster, nextCanvas.transform.position, nextCanvas.transform.rotation); // spawn selected monster
    }
}

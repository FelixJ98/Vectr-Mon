using Unity.VisualScripting;
using UnityEngine;

public class TransitionFrame : MonoBehaviour
{
    public GameObject nextCanvas;
    public GameObject currentCanvas;
    
    // Transition to next canvas
    public void OnButtonClick()
    {
        currentCanvas.SetActive(false);
        nextCanvas.SetActive(true);
    }


}

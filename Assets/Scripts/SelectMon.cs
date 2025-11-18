using UnityEngine;
using UnityEngine.UI;

public class SelectMon : MonoBehaviour
{
    public GameObject monster;
    public Transform canvasTransform;
    GameObject selectedMon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnButtonClick()
    {
        selectedMon = Instantiate(monster, canvasTransform.position, canvasTransform.rotation);
    }
}

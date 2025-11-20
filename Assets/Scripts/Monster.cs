using UnityEngine;

public class Monster : MonoBehaviour
{
    public int monChosen = 0;
    
    // Select monster with finger and disable it from view
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered Event");
        monChosen = 1;
        gameObject.SetActive(false);
    }
}

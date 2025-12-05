using UnityEngine;

public class begoneScript : MonoBehaviour
{
    //if you're looking at this... don't ask it's unity netcode related IT IS CRUCIAL THAT IT IS HERE
    void Awake()
    {
        gameObject.SetActive(false);
    }

}

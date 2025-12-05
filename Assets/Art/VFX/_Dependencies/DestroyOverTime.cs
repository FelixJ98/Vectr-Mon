using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{

    private float delay = 2f;

    void Start()
    {
        Destroy(gameObject, delay);
    }
}

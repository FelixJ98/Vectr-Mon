using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SpawnCube : MonoBehaviour
{
    public GameObject theCube = null;
    private bool ONCEONLY = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.RawButton.A))
        {
            Debug.LogWarning("FIRED");
            if (theCube != null && !ONCEONLY)
            {
                StartCoroutine(spawnCube());
            }
        }
    }

    public IEnumerator spawnCube()
    {
        ONCEONLY = true;
        //Instantiate(theCube, this.transform.position, Quaternion.Euler(0, 0, 0));
        var instance = Instantiate(theCube, this.transform.position, Quaternion.Euler(0, 0, 0));
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
        Debug.LogWarning("FIRED2");
        yield return new WaitForSeconds(1f);
        ONCEONLY = false;
        Debug.LogWarning("Refreshed");
    }
}

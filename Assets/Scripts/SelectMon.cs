using UnityEngine;

public class SelectMon : MonoBehaviour
{
    [SerializeField] private GameObject mon1;
    [SerializeField] private GameObject mon2;
    [SerializeField] private GameObject mon3;
    private int Selected = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Selected == 1)
        {
        }
        else
        { 
            SelectMonster();
        }
    }

    // Checks canvas for player collision with instantiated monsters
    void SelectMonster()
    {
        // Selected monster 1
        if (mon1.GetComponent<Monster>().monChosen == 1)
        {
            // clear canvas
            mon1.SetActive(false);
            mon2.SetActive(false);
            mon3.SetActive(false);
            
            // Instantiate chosen monster
            GameObject CMon = Instantiate(mon1.GetComponent<Monster>().monObj, Transform, Quaternion.identity);
        }
        
        // Selected monster 2
        if (mon2.GetComponent<Monster>().monChosen == 1)
        {
            // clear canvas
            mon1.SetActive(false);
            mon2.SetActive(false);
            mon3.SetActive(false);
            
            // Instantiate chosen monster
            GameObject CMon = Instantiate(mon2.GetComponent<Monster>().monObj, Transform , Quaternion.identity);
        }
        
        // Selected monster 3
        if (mon3.GetComponent<Monster>().monChosen == 1)
        {
            // clear canvas
            mon1.SetActive(false);
            mon2.SetActive(false);
            mon3.SetActive(false);
            
            // Instantiate chosen monster
            GameObject CMon = Instantiate(mon3.GetComponent<Monster>().monObj, Transform, Quaternion.identity);
        }
    }
}

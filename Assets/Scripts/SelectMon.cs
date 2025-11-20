using Meta.WitAi;
using UnityEngine;

public class SelectMon : MonoBehaviour
{
    // Variables to handle selection instances
    static public GameObject parentObject;
    private Transform childOneLoc;
    private GameObject choiceOne;
    private Monster PikamComp;

    private Transform childTwoLoc;
    private GameObject choiceTwo;
    Monster LemonComp;
    
    private Transform childThreeLoc;
    private GameObject choiceThree;
    private Monster shibaComp;
    
    // Variables for creating new instance(s)
    [SerializeField] private GameObject mon1;
    [SerializeField] private GameObject mon2;
    [SerializeField] private GameObject mon3;
    [SerializeField] private Transform canvasTransform;
    private int _selected = 0;

    void start()
    {
        parentObject = gameObject;
        childOneLoc = parentObject.transform.Find("Pikam");
        choiceOne = childOneLoc.gameObject;
        PikamComp = choiceOne.GetComponentInChildren<Monster>();
        childTwoLoc = parentObject.transform.Find("LEMONSHARK");
        choiceTwo = childTwoLoc.gameObject;
        LemonComp = choiceTwo.GetComponentInChildren<Monster>();
        childThreeLoc = parentObject.transform.Find("SHIBA");
        choiceThree = childThreeLoc.gameObject;
        shibaComp = choiceThree.GetComponentInChildren<Monster>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_selected == 0)
        { 
            SelectMonster();
        }
    }

    // Checks canvas for player collision with instantiated monsters
    void SelectMonster()
    {
        // Selected monster 1
        Debug.Log("WERE IN");
        if (PikamComp.monChosen == 1)
        {
            // clear canvas
            Debug.Log("WERE IN2");
            for (int i = 0; i < parentObject.transform.childCount; i++)
            {
                Transform childLoc = parentObject.transform.GetChild(i);
                GameObject child = childLoc.gameObject;
                child.DestroySafely();
            }

            // Instantiate chosen monster at center of canvas
            GameObject p = Instantiate(mon1, canvasTransform.position, canvasTransform.rotation);
            _selected = 1;
            Debug.Log("Deployed!");
        }
        
        // Selected monster 2
        if (LemonComp.monChosen == 1)
        {
            // clear canvas
            for (int i = 0; i < parentObject.transform.childCount; i++)
            {
                Transform childLoc = parentObject.transform.GetChild(i);
                GameObject child = childLoc.gameObject;
                child.DestroySafely();
            }
            
            // Instantiate chosen monster
            GameObject CMon = Instantiate(mon2, canvasTransform.position, canvasTransform.rotation);
            _selected = 1;
            Debug.Log("Deployed!");
        }
        
        // Selected monster 3
        if (shibaComp.monChosen == 1)
        {
            // clear canvas
            for (int i = 0; i < parentObject.transform.childCount; i++)
            {
                Transform childLoc = parentObject.transform.GetChild(i);
                GameObject child = childLoc.gameObject;
                child.DestroySafely();
            }
            
            // Instantiate chosen monster
            GameObject CMon = Instantiate(mon3, canvasTransform.position, canvasTransform.rotation);
            _selected = 1;
            Debug.Log("Deployed!");
        }
    }
}

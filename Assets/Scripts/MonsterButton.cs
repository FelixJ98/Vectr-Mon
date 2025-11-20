using UnityEngine;

public class MonsterButton : MonoBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private SelectionManager manager;

    public void OnSelect()
    {
        manager.AddMonster(monsterPrefab);
    }
}
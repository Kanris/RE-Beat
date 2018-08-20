using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour {
    
    [SerializeField] private DoorSwitch SwitchToObserve;

    private void Start()
    {
        InitializeEvent();
    }

    private void InitializeEvent()
    {
        if (SwitchToObserve != null)
            SwitchToObserve.OnSwitchPressed += SpawnOnChildren;
        else
            SpawnOnChildren();
    }

    private void SpawnOnChildren()
    {
        for (int index = 0; index < transform.childCount; index++)
        {
            var child = transform.GetChild(index);
            SpawnEnemy(child.name, child);
        }
    }

    private void SpawnEnemy(string name, Transform parent)
    {
        var enemyToSpawn = Resources.Load("Enemies/" + name);
        Instantiate(enemyToSpawn, parent);
    }
}

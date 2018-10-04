using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnEnemy
{
    public Transform RespawnPosition;
    public GameObject Enemy;
}

public class SpawnEnemies : MonoBehaviour {
    
    [SerializeField] private DoorSwitch SwitchToObserve;
    [SerializeField] private SpawnEnemy[] EnemiesToSpawn;

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
        foreach (var item in EnemiesToSpawn)
        {
            Instantiate(item.Enemy, item.RespawnPosition);
        }

    }
}

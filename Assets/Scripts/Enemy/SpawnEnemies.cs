using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour {

    [SerializeField] private float TimeToUpdate = 1f;
    [SerializeField] private GameObject ObjectToObserve;

    private float m_UpdateTime;
    private bool m_IsSpawned;

    private void Update()
    {
        if (!m_IsSpawned)
        {
            if (m_UpdateTime < Time.time)
            {
                m_UpdateTime = Time.time + TimeToUpdate;

                if (ObjectToObserve == null)
                {
                    m_IsSpawned = true;
                    SpawnOnChildren();
                }
            }
        }
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

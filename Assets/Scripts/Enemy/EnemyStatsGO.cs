using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyStatsGO : MonoBehaviour {

    public Enemy EnemyStats;

	// Use this for initialization
	void Start () {

        InitializeStats();

    }

    public void InitializeStats()
    {
        EnemyStats.Initialize(gameObject, GetComponent<Animator>());
    }
}

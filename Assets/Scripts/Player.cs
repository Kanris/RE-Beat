using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private float YBoundaries = -20f;
    public PlayerStats playerStats;

    private void Start()
    {
        playerStats.Initialize(gameObject);
    }

    // Update is called once per frame
    private void Update () {
		
        if (transform.position.y <= YBoundaries)
        {
            playerStats.TakeDamage(playerStats.MaxHealth);
        }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public Stats EnemyStats;

	// Use this for initialization
	void Start () {

        InitializeStats();

    }

    private void InitializeStats()
    {
        EnemyStats.Initialize(gameObject);
    }

    private IEnumerator ShowPhrase()
    {
        yield return new WaitForSeconds(2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().playerStats.TakeDamage(999);
        }
    }
}

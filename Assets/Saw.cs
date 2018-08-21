using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : MonoBehaviour {
    
    [SerializeField] private int DamageAmount = 2;

    #region collision

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);
        }
    }

    #endregion
}

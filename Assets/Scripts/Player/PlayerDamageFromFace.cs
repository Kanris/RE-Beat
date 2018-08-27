using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageFromFace : MonoBehaviour {

    public Player player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            player.IsDamageFromFace = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            player.IsDamageFromFace = false;
        }
    }
}

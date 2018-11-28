using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillingGround : MonoBehaviour {

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().playerStats.ReturnPlayerOnReturnPoint();
        }
        else if (collision.CompareTag("Item"))
        {
            yield return new WaitForSeconds(0.5f);
            if (collision != null) Destroy(collision.gameObject);
        }
    }
}

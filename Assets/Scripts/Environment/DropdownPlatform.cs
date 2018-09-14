using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(BoxCollider2D))]
public class DropdownPlatform : MonoBehaviour {

    private BoxCollider2D m_BoxCollider;
    private Platformer2DUserControl player;

    #region initialize

    // Use this for initialization
    void Start () {

        InitializePlatformCollider();

    }

    private void InitializePlatformCollider()
    {
        m_BoxCollider = GetComponent<BoxCollider2D>();
    }

    #endregion

    #region private methods

    private void Update()
    {
        if (player != null)
        {
            if (Input.GetKey(KeyCode.S) & Input.GetKey(KeyCode.Space))
            {
                player.IsCanJump = false;
                CollisionActive(false);
            }
        }
    }

    #region collision methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            player = collision.gameObject.GetComponent<Platformer2DUserControl>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject.GetComponent<Platformer2DUserControl>();
            CollisionActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & player != null)
        {
            player.IsCanJump = true;
            player = null;
            CollisionActive(true);
        }
    }

    #endregion

    private void CollisionActive(bool value)
    {
        if (value)
            gameObject.layer = 14;
        else
            gameObject.layer = 0;

        m_BoxCollider.enabled = value;
    }

    #endregion
}

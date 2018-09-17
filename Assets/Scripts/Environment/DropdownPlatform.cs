using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(BoxCollider2D))]
public class DropdownPlatform : MonoBehaviour {

    #region private fields

    private BoxCollider2D m_BoxCollider; //dropdown platform collider
    private Platformer2DUserControl player; //player user controll

    #endregion

    #region initialize

    // Use this for initialization
    void Start () {

        m_BoxCollider = GetComponent<BoxCollider2D>(); // InitializePlatformCollider

    }

    #endregion

    #region private methods

    private void Update()
    {
        if (player != null) //if player is on the platform
        {
            if (Input.GetKey(KeyCode.S) & Input.GetKey(KeyCode.Space)) //if player pressed s and space 
            {
                player.IsCanJump = false; //dont allow player to jump
                CollisionActive(false); //allow to drop down from platform
            }
        }
    }

    #region collision methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player is on the platform
        {
            player = collision.gameObject.GetComponent<Platformer2DUserControl>(); //get player control
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.CompareTag("Player")) //if player is in the platform trigger (bellow the platform)
        {
            player = collision.gameObject.GetComponent<Platformer2DUserControl>(); //get player control
            CollisionActive(false); //hide platform collision
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & player != null) //if player is above or leave platform trigger
        {
            player.IsCanJump = true; //allow player to jump
            player = null; //remove reference to the player control
            CollisionActive(true); //enable platform collision
        }
    }

    #endregion

    private void CollisionActive(bool value)
    {
        //change platform layer to play stay or jump player's animation
        if (value)
            gameObject.layer = 14;
        else
            gameObject.layer = 0;

        m_BoxCollider.enabled = value; //enable or disable platform collider;
    }

    #endregion
}

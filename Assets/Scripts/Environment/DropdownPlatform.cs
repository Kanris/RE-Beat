using UnityEngine;
using System.Collections;
using UnityStandardAssets._2D;
using UnityStandardAssets.CrossPlatformInput;

public class DropdownPlatform : MonoBehaviour {

    #region private fields

    private BoxCollider2D m_BoxCollider; //dropdown platform collider
    private Platformer2DUserControl player; //player user controll
    private bool m_IsDropDown = false;
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
            if (CrossPlatformInputManager.GetAxis("Vertical") < 0f 
                & CrossPlatformInputManager.GetButton("Jump")) //if player pressed s and space 
            {
                m_IsDropDown = true;
            }

            if (m_IsDropDown)
            {
                m_IsDropDown = false;
                StartCoroutine(DropDownPlayer());
            }
        }

    }

    #region collision methods

    private IEnumerator DropDownPlayer()
    {
        player.IsCanJump = false;

        GetComponent<PlatformEffector2D>().rotationalOffset = 180f;

        gameObject.layer = 0;

        yield return new WaitForEndOfFrame();

        player.IsCanJump = true;

        gameObject.layer = 14;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player is on the platform
        {
            player = collision.gameObject.GetComponent<Platformer2DUserControl>(); //get player control
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player is above or leave platform trigger
        {
            player = null;
            GetComponent<PlatformEffector2D>().rotationalOffset = 0f;
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

using UnityEngine;
using System.Collections;
using UnityStandardAssets._2D;

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
            if ( (InputControlManager.Instance.m_Joystick.LeftStickY < -.5f || InputControlManager.Instance.m_Joystick.DPadDown.WasPressed)
                && InputControlManager.Instance.m_Joystick.Action1.WasPressed) //if player pressed s and space 
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
        
        yield return new WaitForEndOfFrame();

        player.IsCanJump = true;

    }

    private IEnumerator OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player is on the platform
        {
            yield return new WaitForEndOfFrame();

            gameObject.layer = 14;
            player = collision.gameObject.GetComponent<Platformer2DUserControl>(); //get player control
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player is above or leave platform trigger
        {
            player = null;

            gameObject.layer = 0;

            GetComponent<PlatformEffector2D>().rotationalOffset = 0f;

        }
    }

    #endregion

    #endregion
}

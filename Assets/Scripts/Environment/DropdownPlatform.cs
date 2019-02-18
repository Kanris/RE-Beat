using UnityEngine;
using System.Collections;
using UnityStandardAssets._2D;

public class DropdownPlatform : MonoBehaviour {

    #region private fields
    
    private Platformer2DUserControl player; //player user controll
    private bool m_IsDropDown = false;
    #endregion

    #region private methods

    private void Update()
    {
        if (player != null) //if player is on the platform
        {
            if ( InputControlManager.Instance.GetVerticalValue() < -.5f && InputControlManager.Instance.IsJumpPressed() ) //if player pressed s and space 
            {
                m_IsDropDown = true;
            }

            if (m_IsDropDown)
            {
                m_IsDropDown = false;
                DropDownPlayer();
            }
        }

    }

    #region collision methods

    private void DropDownPlayer()
    {
        player.IsCanJump = false;
        //player.transform.position = player.transform.position.Add(y: -0.463f);
        gameObject.GetComponent<PlatformEffector2D>().colliderMask = gameObject.GetComponent<PlatformEffector2D>().colliderMask & ~(1 << player.gameObject.layer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            player = collision.gameObject.GetComponent<Platformer2DUserControl>();
            //gameObject.layer = 14;
        }
    }

    private IEnumerator OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (!player.IsCanJump)
            {
                yield return new WaitForSeconds(.2f);
                player.IsCanJump = true;
                gameObject.GetComponent<PlatformEffector2D>().colliderMask = gameObject.GetComponent<PlatformEffector2D>().colliderMask | (1 << player.gameObject.layer);
            }
            //gameObject.layer = 0;
            player = null;
        }
    }

    #endregion

    #endregion
}

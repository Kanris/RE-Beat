using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class JumpBox : MonoBehaviour
{
    private bool m_IsJump; //indicates is player jumping
    private Rigidbody2D m_Player; //player's rigidbody

    private void Update()
    {
        if (m_Player != null)
        {
            if (InputControlManager.Instance.IsJumpPressed())
            {
                var jumpVector = new Vector2(0f, 10f); //jump right

                if (InputControlManager.Instance.GetHorizontalValue() != 0f)
                {
                    jumpVector = InputControlManager.Instance.GetHorizontalValue() < 0f ? new Vector2(-5f, 10f) : new Vector2(5f, 10f); //jump right
                }

                m_Player.velocity = jumpVector; //move player from the stairs
            }
        }
    }

    private void FixedUpdate()
    {
        if (m_IsJump)
        {
            m_IsJump = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        JumpBoxHandler(collision, true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        JumpBoxHandler(collision, false);
    }

    private void JumpBoxHandler(Collider2D collision, bool isOnPlatform)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlatformerCharacter2D>().SetPlayerOnJumpBox(isOnPlatform);

            collision.GetComponent<Rigidbody2D>().gravityScale = isOnPlatform ? 0f : 3f;

            if (isOnPlatform)
            {
                collision.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                collision.transform.position = collision.transform.position.With(y: transform.position.y);
            }
            collision.GetComponent<Platformer2DUserControl>().enabled = !isOnPlatform;

            m_Player = isOnPlatform ? collision.GetComponent<Rigidbody2D>() : null;
        }
    }
}

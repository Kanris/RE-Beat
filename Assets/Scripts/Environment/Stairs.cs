using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

public class Stairs : MonoBehaviour {

    private Animator m_Animator;
    private bool isPlayerOnStairs;
    private Rigidbody2D m_Player;
    private bool isJumping; 

    private void Start()
    {
        isPlayerOnStairs = false;
    }

    private void FixedUpdate()
    {
        if (isPlayerOnStairs)
        {
            if (m_Player != null)
            {
                if (CrossPlatformInputManager.GetAxis("Vertical") != 0f)
                {
                    m_Animator.SetBool("IsMovingOnStairs", true);

                    var yPos = 0.03f;

                    if (CrossPlatformInputManager.GetAxis("Vertical") < 0f)
                        yPos = -yPos;

                    m_Player.position += new Vector2(0, yPos);
                }
                else if (CrossPlatformInputManager.GetAxis("Horizontal") != 0f & CrossPlatformInputManager.GetButtonDown("Jump") & !isJumping)
                {
                    isJumping = true;
                    PlayerOnStairs(false, m_Player.gameObject);
                }
                else
                {
                    if (m_Player.bodyType == RigidbodyType2D.Dynamic)
                    {
                        m_Player.bodyType = RigidbodyType2D.Kinematic;
                        isJumping = false;
                    }

                    m_Player.GetComponent<Platformer2DUserControl>().enabled = false;
                    m_Animator.SetBool("IsMovingOnStairs", false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !isPlayerOnStairs)
        {
            PlayerOnStairs(true, collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & isPlayerOnStairs)
        {
            PlayerOnStairs(false, collision.gameObject);

            m_Animator = null;
            m_Player = null;
        }
    }

    private void PlayerOnStairs(bool isOnstairs, GameObject collision)
    {
        isPlayerOnStairs = isOnstairs;
        GetComponentsOnPlayer(collision);
        m_Animator.SetBool("OnStairs", isPlayerOnStairs);

        if (isOnstairs)
        {
            m_Player.bodyType = RigidbodyType2D.Kinematic;
            m_Player.velocity = Vector3.zero;
        }
        else
        {
            if (isJumping)
            {
                m_Player.velocity += new Vector2(0, 8f);
            }

            m_Player.bodyType = RigidbodyType2D.Dynamic;
        }

        collision.GetComponent<Platformer2DUserControl>().enabled = !isOnstairs;

        isJumping = false;
    }

    private void GetComponentsOnPlayer(GameObject collision)
    {
        m_Animator = collision.GetComponent<Animator>();
        m_Player = collision.GetComponent<Rigidbody2D>();
    }
}

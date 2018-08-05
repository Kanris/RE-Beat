using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

public class Stairs : MonoBehaviour {

    private Animator m_Animator;
    private bool isPlayerOnStairs;
    private Rigidbody2D m_Player;
    [SerializeField] private bool isJumping; 

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
                    MoveOnStairs();
                }
                else if (CrossPlatformInputManager.GetAxis("Horizontal") != 0f & 
                    CrossPlatformInputManager.GetButton("Jump") & !isJumping)
                {
                    isJumping = true;
                    PlayerOnStairs(false, m_Player.gameObject);
                }
                else
                {
                    m_Animator.SetBool("IsMovingOnStairs", false);
                }
            }
        }
        else if (isJumping & m_Player != null)
        {
            if (m_Animator.GetBool("Ground"))
            {
                isJumping = false;
                m_Player.GetComponent<Platformer2DUserControl>().enabled = true;
                m_Player = null;
            }
        }
    }

    private void MoveOnStairs()
    {
        m_Animator.SetBool("IsMovingOnStairs", true);

        var yPos = 0.03f;

        if (CrossPlatformInputManager.GetAxis("Vertical") < 0f)
            yPos = -yPos;

        m_Player.position += new Vector2(0, yPos);
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
            collision.GetComponent<Platformer2DUserControl>().enabled = false;
            m_Player.gravityScale = 0f;
            m_Player.velocity = Vector3.zero;
        }
        else
        {
            m_Player.gravityScale = 3f;

            if (isJumping)
            {
                var jumpVector = new Vector2(5f, 10f);

                if (CrossPlatformInputManager.GetAxis("Horizontal") < 0f)
                    jumpVector = new Vector2(-5f, 10f);

                m_Player.velocity = jumpVector;
            }
        }
    }

    private void GetComponentsOnPlayer(GameObject collision)
    {
        m_Animator = collision.GetComponent<Animator>();
        m_Player = collision.GetComponent<Rigidbody2D>();
    }
}

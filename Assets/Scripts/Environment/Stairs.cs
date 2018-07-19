using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour {

    private Animator m_Animator;
    private bool isPlayerOnStairs;
    private Rigidbody2D m_Player;

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
                if (Input.GetKey(KeyCode.W))
                {
                    m_Animator.SetBool("IsMovingOnStairs", true);
                    m_Player.position += new Vector2(0, 0.03f);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    m_Animator.SetBool("IsMovingOnStairs", true);
                    m_Player.position += new Vector2(0, -0.03f);
                }
                else
                {
                    if (m_Player.bodyType == RigidbodyType2D.Dynamic)
                        m_Player.bodyType = RigidbodyType2D.Kinematic;
                    m_Animator.SetBool("IsMovingOnStairs", false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !isPlayerOnStairs)
        {
            isPlayerOnStairs = true;
            m_Animator = collision.GetComponent<Animator>();
            m_Player = collision.GetComponent<Rigidbody2D>();

            m_Player.bodyType = RigidbodyType2D.Kinematic;
            m_Player.velocity = new Vector2(0, 0);

            m_Animator.SetBool("OnStairs", isPlayerOnStairs);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & isPlayerOnStairs)
        {
            isPlayerOnStairs = false;

            m_Animator.SetBool("OnStairs", isPlayerOnStairs);
            m_Player.bodyType = RigidbodyType2D.Dynamic;

            m_Animator = null;
            m_Player = null;
        }
    }

}

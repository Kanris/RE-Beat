using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

public class Stairs : MonoBehaviour {

    private Transform m_StairsTop;
    private Animator m_Animator;
    private Rigidbody2D m_Player;
    private bool m_VerticalMove;
    private bool isJumping;

    private void Start()
    {
        InitializeStairsTop();
        
    }

    #region Initialize

    private void InitializeStairsTop()
    {
        if (transform.childCount > 0)
        {
            m_StairsTop = transform.GetChild(0).gameObject.transform;
        }
        else
        {
            Debug.LogError("Stairs.InitializeStairsTop: Can't find top of the stairs");
        }
    }

    #endregion

    private void Update()
    {
        if (m_Player != null)
        {
            if (CrossPlatformInputManager.GetAxis("Vertical") != 0f & !m_VerticalMove)
            {
                m_VerticalMove = true;
            }
            else if (CrossPlatformInputManager.GetAxis("Horizontal") != 0f &
                CrossPlatformInputManager.GetButton("Jump") & !isJumping)
            {
                isJumping = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (m_Player != null)
        {
            if (m_VerticalMove)
            {
                m_VerticalMove = false;
                MoveOnStairs();
            }
            else if (isJumping)
            {
                PlayerOnStairs(false, m_Player.gameObject);
            }
            else
            {
                m_Animator.SetBool("IsMovingOnStairs", false);
            }
        }
    }

    private void MoveOnStairs()
    {
        m_Animator.SetBool("IsMovingOnStairs", true);

        var yPos = 0.03f;

        if (CrossPlatformInputManager.GetAxis("Vertical") < 0f)
        {
            yPos = -yPos;
            m_Player.position += new Vector2(0, yPos);
        }
        else
        {

            var nextPlayerPosition = m_Player.transform.position.y + yPos;

            if (nextPlayerPosition < m_StairsTop.position.y)
                m_Player.position += new Vector2(0, yPos);
            else
                m_Animator.SetBool("IsMovingOnStairs", false);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerOnStairs(true, collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerOnStairs(false, collision.gameObject);

            isJumping = false;
            m_Player = null;
        }
    }

    private void PlayerOnStairs(bool isOnstairs, GameObject collision)
    {
        GetComponentsOnPlayer(collision);

        m_Animator.SetBool("OnStairs", isOnstairs);
        m_Player.GetComponent<Platformer2DUserControl>().enabled = !isOnstairs;

        if (isOnstairs)
        {
            m_Player.gravityScale = 0f;
            m_Player.position = new Vector2(m_StairsTop.position.x + 0.1f, m_Player.position.y);
            m_Player.velocity = Vector3.zero;
        }
        else
        {
            m_Player.gravityScale = 3f;

            if (isJumping)
            {
                isJumping = false;
                var jumpVector = new Vector2(5f, 10f);

                if (CrossPlatformInputManager.GetAxis("Horizontal") < 0f)
                {
                    jumpVector = new Vector2(-5f, 10f);
                }

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

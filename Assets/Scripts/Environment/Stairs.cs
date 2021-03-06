﻿using UnityEngine;
using UnityStandardAssets._2D;

public class Stairs : MonoBehaviour {

    #region private fields

    private Transform m_StairsTop; //stairs top transform

    private Animator m_Animator; //player's animator
    private Rigidbody2D m_Player; //player's rigidbody
    private bool m_VerticalMove; //is player move verticaly
    private bool isJumping; //is player press jump button

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        InitializeStairsTop();

    }

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
        if (m_Player != null) //if player is on stairs
        {
            if (InputControlManager.Instance.GetHorizontalValue() != 0f && InputControlManager.Instance.IsJumpPressed()
                    && !isJumping) //if player want to jump from stairs
            {
                isJumping = true;
            }
            else if (InputControlManager.Instance.GetVerticalValue() != 0f && !m_VerticalMove && !isJumping) //if player moves on stairs
            {
                m_VerticalMove = true;
            }

            if (m_Player.GetComponent<Rigidbody2D>().gravityScale != 0f)
            {
                m_Player.GetComponent<Rigidbody2D>().gravityScale = 0f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (m_Player != null) //if player is on stairs
        {
            if (m_VerticalMove) //if player is moving on stairs
            {
                m_VerticalMove = false;
                MoveOnStairs(); //move player
            }
            else if (isJumping) //if player want to jump from stairs
            {
                isJumping = false;

                var jumpVector = InputControlManager.Instance.GetHorizontalValue() < 0f ? new Vector2(-5f, 10f) : new Vector2(5f, 10f); //jump right

                m_Player.velocity = jumpVector; //move player from the stairs
            }
            else if (m_Animator.GetBool("IsMovingOnStairs")) //if player animation show player movement on stairs
            {
                m_Animator.SetBool("IsMovingOnStairs", false); //change animation state
            }
        }
    }

    private void MoveOnStairs()
    {
        m_Animator.SetBool("IsMovingOnStairs", true); //play player move on stairs animation

        var yPos = 0.03f; //move value

        if (InputControlManager.Instance.GetVerticalValue() < 0f) //if player move down on stairs
        {
            yPos = -yPos; //change move value
            m_Player.position += new Vector2(0, yPos); //move player down
        }
        else //if player move to the top of stairs
        {
            var nextPlayerPosition = m_Player.transform.position.y + yPos; //predict next player position

            if (nextPlayerPosition < m_StairsTop.position.y) //move player up if he is not rechead top
                m_Player.position += new Vector2(0, yPos);
            else //if next position is top or above top
                m_Animator.SetBool("IsMovingOnStairs", false); //stop player's movement
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player on stairs
        {
            GetComponentsOnPlayer(collision.gameObject); //get animator and controler from player gameobject
            PlayerOnStairs(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leave stairs
        {
            PlayerOnStairs(false);
        }
    }

    private void PlayerOnStairs(bool isOnstairs)
    {
        m_Animator.SetBool("OnStairs", isOnstairs); //play onstairs animation
        m_Player.GetComponent<PlatformerCharacter2D>().enabled = !isOnstairs; //disable or enable standart player movement script

        if (isOnstairs) //if player is on stairs
        {
            m_Player.gravityScale = 0f; //disable gravity
            m_Player.position = new Vector2(m_StairsTop.position.x + 0.1f, m_Player.position.y); //place player in the center of the stairs
            m_Player.velocity = Vector2.zero; //disable player velocity
        }
        else //if player leave stairs
        {
            m_Player.gravityScale = 3f; //return gravity back to normal

            m_Animator.SetBool("IsMovingOnStairs", false); //stop player's movement

            m_Player.GetComponent<PlatformerCharacter2D>().m_IsHaveDoubleJump = true; //reset double jump state

            m_Animator = null;
            m_Player = null;
        }
    }

    private void GetComponentsOnPlayer(GameObject collision)
    {
        m_Animator = collision.GetComponent<Animator>();
        m_Player = collision.GetComponent<Rigidbody2D>();
    }

    #endregion
}

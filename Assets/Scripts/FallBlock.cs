using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallBlock : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField] private float IdleTime = 4f; //block idle time
    [SerializeField] private float FallTime = 2f; //block fall time

    #endregion

    private Rigidbody2D m_Rigidbody; //block rigidbody
    private float m_UpdateTime; //change state time
    private bool m_IsIdle; //is block idling
    private float m_YPosition;

    #endregion

    #region private methods

    #region initialize

    private void Start()
    {
        InitializeRigidbody();
        m_YPosition = -5f;
    }

    private void InitializeRigidbody()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    #endregion

    private void FixedUpdate()
    {
        if (m_UpdateTime <= Time.time) //if need to change state
        {
            m_UpdateTime = Time.time;

            m_IsIdle = !m_IsIdle; //change state

            if (m_IsIdle) //is idle state
            {
                m_UpdateTime += IdleTime; //add idle time
            }
            else
            {
                m_UpdateTime += FallTime; //add fall time
            }

            MoveBlock(); //move or stop block (base on state)
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player in block's trigger
        {
            collision.GetComponent<Player>().playerStats.KillPlayer(); //kill player
        }
    }

    private void MoveBlock()
    {
        var moveVector = Vector2.zero; //stop block's moving

        if (!m_IsIdle) //if block is not idle
        {
            moveVector = new Vector2(0f, m_YPosition); //get move vector
            m_YPosition *= -1; //get next y value

            if (m_YPosition > 0) //idle on top
                m_IsIdle = true;
        }

        m_Rigidbody.velocity = moveVector; //move block
    }

    #endregion
}

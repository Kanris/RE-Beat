using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallBlock : MonoBehaviour {

    #region serialize fields

    [SerializeField] private float IdleTime = 4f;
    [SerializeField] private float FallTime = 2f;

    #endregion

    #region private fields

    private Rigidbody2D m_Rigidbody;
    private float m_UpdateTime;
    private bool m_IsIdle;
    private float m_FallValue;

    #endregion

    #region private methods

    #region initialize

    private void Start()
    {
        InitializeRigidbody();
        m_FallValue = -5f;
    }

    private void InitializeRigidbody()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    #endregion

    private void FixedUpdate()
    {
        if (m_UpdateTime <= Time.time)
        {
            m_UpdateTime = Time.time;

            m_IsIdle = !m_IsIdle;

            if (m_IsIdle)
            {
                m_UpdateTime += IdleTime;
            }
            else
            {
                m_UpdateTime += FallTime;
            }

            MoveBlock();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().playerStats.TakeDamage(999);
        }
    }

    private void MoveBlock()
    {
        var moveVector = Vector2.zero;

        if (!m_IsIdle)
        {
            moveVector = new Vector2(0f, m_FallValue);
            m_FallValue *= -1;

            if (m_FallValue > 0)
                m_IsIdle = true;
        }

        m_Rigidbody.velocity = moveVector;
    }

    #endregion
}

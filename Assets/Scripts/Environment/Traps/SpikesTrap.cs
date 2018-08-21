using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpikesTrap : MonoBehaviour {

    public int DamageAmount = 2;

    private PlayerStats m_Player;
    private bool m_IsTriggered;
    private bool m_IsShake;
    private bool m_IsDanger;
    private float m_ShakePosX;
    private Rigidbody2D m_Rigidbody;

    private void Start()
    {
        m_ShakePosX = 0.3f;
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    #region trigger

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_Player = collision.GetComponent<Player>().playerStats;

            if (!m_IsShake & !m_IsDanger)
                m_IsTriggered = true;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_Player = null;
        }
    }

    #endregion

    private void FixedUpdate()
    {
        if (m_Player != null)
        {
            if (m_IsTriggered & !m_IsShake)
            {
                StartCoroutine(Shake());
            }

            if (m_IsDanger)
            {
                AttackPlayer();
            }
        }
    }

    private IEnumerator Shake()
    {
        m_IsTriggered = false;
        m_IsShake = true;

        yield return Shake(5);

        yield return VerticalMovement(0.5f, 0.5f);

        m_IsShake = false;

        m_IsDanger = true;

        yield return new WaitForSeconds(0.5f);
        
        yield return VerticalMovement(-0.5f, 0.5f);

        m_IsDanger = false;
    }

    #region position movement

    private IEnumerator Shake(int iterations)
    {
        for (int index = 0; index < iterations; index++)
        {

            m_ShakePosX = -m_ShakePosX;
            m_Rigidbody.velocity = new Vector2(m_ShakePosX, 0f);

            yield return new WaitForSeconds(0.2f);

            m_Rigidbody.velocity = Vector2.zero;
        }
    }

    private IEnumerator VerticalMovement(float posY, float time)
    {
        m_Rigidbody.velocity = new Vector2(0f, posY);

        yield return new WaitForSeconds(time);

        m_Rigidbody.velocity = Vector2.zero;
    }

    #endregion

    private void AttackPlayer()
    {
        m_Player.TakeDamage(DamageAmount);
    }

}

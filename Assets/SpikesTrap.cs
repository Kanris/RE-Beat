using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesTrap : MonoBehaviour {

    public int DamageAmount = 2;

    private PlayerStats m_Player;
    private bool m_IsTriggered;
    private bool m_IsShake;
    private bool m_IsDanger;
    private float m_ShakePosX;

    private void Start()
    {
        m_ShakePosX = 0.05f;
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

        yield return ShakeHorizontal(5);

        yield return VerticalMovement(4, true);

        m_IsShake = false;

        m_IsDanger = true;

        yield return new WaitForSeconds(0.5f);
        
        yield return VerticalMovement(4, false);

        m_IsDanger = false;
    }

    #region position movement

    private IEnumerator VerticalMovement(int iterations, bool isUp)
    {
        var posY = -0.05f;

        if (isUp)
            posY *= -1;

        for (int index = 0; index < iterations; index++)
        {
            transform.position += new Vector3(0f, posY);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ShakeHorizontal(int iterations)
    {
        for (int index = 0; index < iterations; index++)
        {
            m_ShakePosX = -m_ShakePosX;
            transform.position += new Vector3(m_ShakePosX, 0f);

            yield return new WaitForSeconds(0.2f);
        }
    }

    #endregion

    private void AttackPlayer()
    {
        m_Player.TakeDamage(DamageAmount);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpikesTrap : MonoBehaviour {

    #region private fields

    [SerializeField, Range(2, 10)] private int DamageAmount = 2; //spike damage amount

    private PlayerStats m_Player; //reference to player stats
    private Rigidbody2D m_Rigidbody; //current object of rigidbody
    private bool m_IsTriggered; //if trap is playing shake animation
    private float m_ShakePosX; //x shake

    #endregion

    #region private methods

    private void Start()
    {
        m_ShakePosX = 0.3f; //shake amount
        m_Rigidbody = GetComponent<Rigidbody2D>(); //reference to the spike rigidbody
    }

    #region trigger

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is on trap
        {
            m_Player = collision.GetComponent<Player>().playerStats; //get reference to the player stats

            if (!m_IsTriggered) //if spike is not playing shake animation and is not fully open
                StartCoroutine(Shake()); //start shake
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_Player = null; //remove reference if player is not on spikes
        }
    }

    #endregion

    private IEnumerator Shake()
    {
        m_IsTriggered = true; //notify that shake animation in progress

        yield return Shake(5); //shake

        yield return VerticalMovement(1f, 0.2f); //move spikes up

        AttackPlayer(); //try to damage player

        yield return new WaitForSeconds(0.2f); //wait before hide spikes
        
        yield return VerticalMovement(-1, 0.2f); //hide spikes

        m_IsTriggered = false; //notify that shake animation is over
    }

    #region position movement

    private IEnumerator Shake(int iterations)
    {
        for (int index = 0; index < iterations; index++) //shake spikes from left to right
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
        if (m_Player != null)
            m_Player.TakeDamage(DamageAmount);
    }
    #endregion
}

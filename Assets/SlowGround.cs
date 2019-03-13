using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class SlowGround : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField, Range(1, 10)] private float m_PlayerSpeedInGround = 2f; //player's speed in ground

    private float m_StandartSpeedValue = 4f; //default player speed

    //when player enter slow down ground
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ChangePlayerStats(collision);
    }

    //when player leave slow down ground
    private void OnTriggerExit2D(Collider2D collision)
    {
        ChangePlayerStats(collision, true);
    }

    //change player's movement speed stat
    private void ChangePlayerStats(Collider2D collision, bool isLeave = false)
    {
        if (collision.CompareTag("Player"))
        {
            if (!isLeave)
                m_StandartSpeedValue = collision.GetComponent<PlatformerCharacter2D>().m_MaxSpeed;

            collision.GetComponent<PlatformerCharacter2D>().m_MaxSpeed = isLeave ? m_StandartSpeedValue : m_PlayerSpeedInGround;
        }
    }
}

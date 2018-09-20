using System.Collections;
using UnityEngine;

public class TurnAroundOnTrigger : MonoBehaviour {

    #region serialize fields

    [SerializeField] private EnemyMovement m_EnemyMovement; //to turn around
    [SerializeField] private bool m_IsNeedToTurnAround = true; //is trigger behind the enemy
    [SerializeField] private float m_WaitTimer = 3f; //pursuit time

    #endregion

    #region private fields

    private bool m_IsPlayerNear; //is player near the enemy

    #endregion

    #region private methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (m_IsNeedToTurnAround) m_EnemyMovement.TurnAround(); //turn around if trigger behind the enemy

            m_IsPlayerNear = true;
            m_EnemyMovement.isPlayerNear = m_IsPlayerNear;
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = false; 

            yield return new WaitForSeconds(m_WaitTimer); //pursuit timer

            if (!m_IsPlayerNear) //if enemy couldn't catch up player
             m_EnemyMovement.isPlayerNear = false; //player is not near
        }
    }

    #endregion
}

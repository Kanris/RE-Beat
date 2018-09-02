using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAroundOnTrigger : MonoBehaviour {

    #region serialize fields

    [SerializeField] private EnemyMovement m_EnemyMovement;
    [SerializeField] private bool m_IsNeedToTurnAround = true;
    [SerializeField] private float m_WaitTimer = 3f;

    #endregion

    #region private fields

    private bool m_IsPlayerNear;

    #endregion

    #region private methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (m_IsNeedToTurnAround) m_EnemyMovement.TurnAround();

            m_IsPlayerNear = true;
            m_EnemyMovement.isPlayerNear = m_IsPlayerNear;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = true;
            m_EnemyMovement.isPlayerNear = m_IsPlayerNear;
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = false;

            yield return new WaitForSeconds(m_WaitTimer);

            if (!m_IsPlayerNear)
             m_EnemyMovement.isPlayerNear = false;
        }
    }

    #endregion
}

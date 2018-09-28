using System.Collections;
using UnityEngine;

public class PlayerInTrigger : MonoBehaviour {

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnPlayerInTrigger;

    [SerializeField, Range(0f, 10f)] private float m_WaitBeforeStopChasing = 2f; //timer before drone stop chasing
    [SerializeField] private Transform m_Follow; //drone to follow

    private bool m_IsPlayerNear = false; //indicates is player near
    private bool m_IsGoingToStop = false;

    private void Update()
    {
        transform.position = m_Follow.position; //folow the drone body
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (OnPlayerInTrigger != null) //notify that player is near
                OnPlayerInTrigger(true);

            m_IsPlayerNear = true;

            StopAllCoroutines(); //stop - StopChasing - courutine
            m_IsGoingToStop = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = false;

            if (!m_IsGoingToStop)
                StartCoroutine(StopChasing());
        }
    }

    private IEnumerator StopChasing()
    {
        m_IsGoingToStop = true;

        if (m_WaitBeforeStopChasing > 0)
            yield return new WaitForSeconds(m_WaitBeforeStopChasing);

        if (!m_IsPlayerNear)
        {
            if (OnPlayerInTrigger != null)
            {
                OnPlayerInTrigger(false);
            }
        }

        m_IsGoingToStop = false;
    }

}

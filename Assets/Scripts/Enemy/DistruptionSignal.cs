using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class DistruptionSignal : MonoBehaviour
{

    private Camera2DFollow m_Camera;
    private bool m_IsPlayerNear;

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = Camera.main.GetComponent<Camera2DFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsPlayerNear)
        {
            if (GameMaster.Instance.IsPlayerDead)
            {
                m_IsPlayerNear = false;
                m_Camera.StopLowHealthEffect();
            }
        }
    }

    private void OnDestroy()
    {
        if (m_IsPlayerNear)
        {
            m_Camera.StopLowHealthEffect();

            UIManager.Instance.DisplayNotificationMessage
                ("Distruption signal is disappear", UIManager.Message.MessageType.Message);
        }
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            m_IsPlayerNear = true;

            yield return new WaitForSeconds(1f);

            if (m_IsPlayerNear)
            {

                UIManager.Instance.DisplayNotificationMessage
                    ("Distruption signal is nearby", UIManager.Message.MessageType.Message);

                m_Camera.PlayLowHealthEffect();
            }
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsPlayerNear)
        {
            m_IsPlayerNear = false;

            yield return new WaitForSeconds(1f);

            if (!m_IsPlayerNear)
                m_Camera.StopLowHealthEffect();
        }
    }
}

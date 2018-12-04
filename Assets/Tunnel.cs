using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tunnel : MonoBehaviour {

    [SerializeField] AnimationClip m_InTunnelAnimation;

    private Transform m_SpawnOnExit;
    private static Transform m_Companion;

    private void Start()
    {
        m_SpawnOnExit = transform.GetChild(0);
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Companion"))
        {
            m_Companion = collision.transform;

            m_Companion.GetComponent<Animator>().SetTrigger("InTunnel");

            yield return new WaitForSeconds(m_InTunnelAnimation.length);

            yield return null;

            collision.gameObject.SetActive(false);
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (m_Companion != null)
            {
                if (IsCanLeaveTunnel(collision.transform))
                {
                    m_Companion.position = m_SpawnOnExit.position;
                    m_Companion.gameObject.SetActive(true);

                    m_Companion.GetComponent<Animator>().SetTrigger("FromTunnel");
                    m_Companion.GetComponent<Companion>().enabled = false;

                    yield return new WaitForSeconds(m_InTunnelAnimation.length);

                    m_Companion.GetComponent<Companion>().enabled = true;
                    m_Companion.GetComponent<Companion>().SetTunnel(transform);

                    m_Companion = null;
                }
            }
        }
    }

    private bool IsCanLeaveTunnel(Transform player)
    {
        var difference = player.position - transform.position;
        var result = true;

        if (difference.x < 0 & transform.localScale.x > 0)
            result = false;

        else if (difference.x > 0 & transform.localScale.x < 0)
            result = false;

        return result;
    }
}

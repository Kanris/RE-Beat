using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tunnel : MonoBehaviour {

    private Transform m_SpawnOnExit;
    private static Transform m_Companion;

    private void Start()
    {
        m_SpawnOnExit = transform.GetChild(0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Companion"))
        {
            m_Companion = collision.transform;
            
            collision.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (m_Companion != null)
            {
                m_Companion.position = m_SpawnOnExit.position;
                m_Companion.gameObject.SetActive(true);

                m_Companion.GetComponent<Companion>().SetTunnel(transform);

                m_Companion = null;
            }
        }
    }
}

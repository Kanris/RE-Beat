using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tunnel : MonoBehaviour {

    [SerializeField] private AnimationClip m_InTunnelAnimation;
    [SerializeField] private GameObject m_FollowCompanion;

    private Transform m_SpawnOnExit;
    private static bool m_IsSpawning;
    private static bool m_IsCanSpawn;

    private void Start()
    {
        m_SpawnOnExit = transform.GetChild(0);
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Companion"))
        {
            m_IsSpawning = false;

            collision.GetComponent<Animator>().SetTrigger("InTunnel");

            yield return new WaitForSeconds(m_InTunnelAnimation.length);

            Destroy(collision.gameObject);
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsSpawning & m_IsCanSpawn)
        {
            if (IsCanLeaveTunnel(collision.transform))
            {
                m_IsSpawning = true;

                var m_Companion = Instantiate(m_FollowCompanion, m_SpawnOnExit.position, Quaternion.identity);

                m_Companion.GetComponent<Animator>().SetTrigger("FromTunnel");

                yield return new WaitForSeconds(m_InTunnelAnimation.length);

                m_Companion.GetComponent<Companion>().SetTarget(collision.transform);
                m_Companion.GetComponent<Companion>().SetTunnel(transform);
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

    public static void SetIscanSpawn(bool value)
    {
        m_IsCanSpawn = value;
    }
}

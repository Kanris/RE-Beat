using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tunnel : MonoBehaviour {

    [SerializeField] private GameObject m_InteractionUI;
    [SerializeField] private GameObject m_TeleportToTunnel;

    [SerializeField] private AnimationClip m_InTunnelAnimation;
    [SerializeField] private GameObject m_FollowCompanion;

    private Transform m_SpawnOnExit;

    private Transform m_CompanionToTeleport;

    private void Start()
    {
        m_SpawnOnExit = transform.GetChild(0);

        if (m_InteractionUI != null)
        {
            m_InteractionUI.SetActive(false);
            GetComponent<SpriteRenderer>().color = Color.magenta;
        }
    }

    private void Update()
    {
        if (m_CompanionToTeleport != null & InputControlManager.IsCanUseSubmitButton())
        {
            if (InputControlManager.IsUpperButtonsPressed())
            {
                MoveToNextTunnel();
            }
        }
    }

    private void MoveToNextTunnel()
    {
        m_CompanionToTeleport.transform.position = m_TeleportToTunnel.transform.position;
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Companion"))
        {
            collision.GetComponent<Animator>().SetTrigger("InTunnel");

            yield return new WaitForSeconds(m_InTunnelAnimation.length);

            Destroy(collision.transform.parent.gameObject);
        }

        if (collision.CompareTag("Player") && GameMaster.Instance.IsPlayerDead)
        {
            if (m_InteractionUI != null)
            {
                m_InteractionUI.SetActive(true);
                m_CompanionToTeleport = collision.transform;
            }
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !Companion.m_IsWithPlayer && !GameMaster.Instance.IsPlayerDead)
        {
            if (IsCanLeaveTunnel(collision.transform))
            {
                var m_Companion = Instantiate(m_FollowCompanion, m_SpawnOnExit.position, Quaternion.identity)
                                    .transform.GetChild(0);

                m_Companion.GetComponent<Animator>().SetTrigger("FromTunnel");

                yield return new WaitForSeconds(m_InTunnelAnimation.length);

                m_Companion.GetComponent<Companion>().SetTarget(collision.transform);
                m_Companion.GetComponent<Companion>().SetTunnel(transform);
            }
        }

        if (collision.CompareTag("Player") && GameMaster.Instance.IsPlayerDead)
        {
            if (m_InteractionUI != null)
            {
                m_InteractionUI.SetActive(false);
                m_CompanionToTeleport = null;
            }
        }
    }

    private bool IsCanLeaveTunnel(Transform player)
    {
        var difference = player.position - transform.position;
        var result = true;

        if (difference.x < 0 && transform.localScale.x > 0)
            result = false;

        else if (difference.x > 0 && transform.localScale.x < 0)
            result = false;

        return result;
    }
}

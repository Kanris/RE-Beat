using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class ReachablePoint : MonoBehaviour {

    [SerializeField] private Transform m_NearestTunnel;

    private SpriteRenderer m_SpriteRenderer;

    private void Start()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        GetComponent<Animator>()?.SetFloat("Speed", Random.Range(.8f, 1.2f));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player triggered reacheble point
        {
            //set reachable point transform
            GameMaster.Instance.SetReachablePoint(transform, m_NearestTunnel);

            //activate point effect
            SetActivatePoint(true);

            //display to the player that revive with companion is available
            UIManager.Instance.SetReviveAvailable(true);
            UIManager.Instance
                .DisplayNotificationMessage("Companion can now revive you", UIManager.Message.MessageType.Message);
        }
    }

    public void SetActivatePoint(bool value)
    {
        var color = Color.white;

        if (value)
        {
            ColorUtility.TryParseHtmlString("#FF56BC", out color);
        }

        m_SpriteRenderer.color = color;
    }

}

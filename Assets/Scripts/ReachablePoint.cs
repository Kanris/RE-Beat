using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachablePoint : MonoBehaviour {

    [SerializeField] private Transform m_NearestTunnel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player triggered reacheble point
        {
            //set reachable point transform
            GameMaster.Instance.SetReachablePoint(transform, m_NearestTunnel);

            //display to the player that revive with companion is available
            UIManager.Instance.SetReviveAvailable(true);
            UIManager.Instance
                .DisplayNotificationMessage("Companion can now revive you", UIManager.Message.MessageType.Message);
        }
    }

}

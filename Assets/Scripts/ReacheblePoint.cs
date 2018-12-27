using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacheblePoint : MonoBehaviour {

    [SerializeField] private Transform m_NearestTunnel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameMaster.Instance.SetReacheblePoint(transform, m_NearestTunnel);
            UIManager.Instance.SetReviveAvailable(true);
        }
    }

}

﻿using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

public class RevivePlayer : MonoBehaviour {

    [Header("Objects to spawn")]
    [SerializeField] private GameObject m_PlayerToRevive;
    [SerializeField] private GameObject m_Companion;

    [Header("Effects")]
    [SerializeField] private GameObject m_ReviveParticles;

    private bool m_IsReviving;
    private GameObject m_PlayerThatInteract;

    private int m_ScrapAmount;

    private void Start()
    {
        m_ScrapAmount = PlayerStats.Scrap;
        PlayerStats.Scrap = -PlayerStats.Scrap;

        UIManager.Instance.EnableCompanionUI();
    }

    private IEnumerator Revive()
    {
        m_IsReviving = true;

        var materialForPlayer = m_PlayerThatInteract.GetComponent<SpriteRenderer>().material;
        materialForPlayer.color = materialForPlayer.color.ChangeColor(a: 1f);

        yield return Camera.main.GetComponent<Camera2DFollow>().PlayReviveEffect();

        Destroy(m_PlayerThatInteract.transform.parent.gameObject);

        GameMaster.Instance.m_Player = Instantiate(m_PlayerToRevive, transform.position, transform.rotation);
        GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<SpriteRenderer>().material = materialForPlayer;

        Instantiate(m_Companion, transform.position, transform.rotation).transform.GetChild(0)
            .GetComponent<SpriteRenderer>().material = materialForPlayer;

        Destroy(Instantiate(m_ReviveParticles, transform.position, Quaternion.identity), 5f);

        PlayerStats.Scrap = m_ScrapAmount;

        UIManager.Instance.EnableRegularUI();

        UIManager.Instance.RemoveRevive(PlayerStats.m_ReviveCount);
        PlayerStats.m_ReviveCount -= 1;

        GameMaster.Instance.IsPlayerDead = false;

        Physics2D.IgnoreLayerCollision(8, 13, false); //ignore all enemies

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsReviving)
        {
            m_PlayerThatInteract = collision.gameObject;

            m_PlayerThatInteract.GetComponent<Platformer2DUserControl>().enabled = false;
            m_PlayerThatInteract.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            StartCoroutine(Revive());
        }
    }
}

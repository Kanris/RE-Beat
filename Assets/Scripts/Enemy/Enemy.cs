﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour {

    public Stats EnemyStats;

    private EnemyMovement m_EnemyMovement;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private string Phrase;

	// Use this for initialization
	void Start () {

        InitializeStats();

        InitializeEnemyMovement();
    }

    private void InitializeStats()
    {
        EnemyStats.Initialize(gameObject);
    }

    private void InitializeEnemyMovement()
    {
        m_EnemyMovement = GetComponent<EnemyMovement>();
    }

    private IEnumerator ShowPhrase()
    {
        m_Text.text = Phrase;
        m_EnemyMovement.isWaiting = true;

        yield return new WaitForSeconds(2f);

        m_Text.text = string.Empty;
        m_EnemyMovement.isWaiting = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().playerStats.TakeDamage(999);
            StartCoroutine(ShowPhrase());
        }
    }
}
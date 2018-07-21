using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public Stats EnemyStats;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private string Phrase;

	// Use this for initialization
	void Start () {

        InitializeStats();
    }

    private void InitializeStats()
    {
        EnemyStats.Initialize(gameObject);
    }

    private IEnumerator ShowPhrase()
    {
        m_Text.text = Phrase;

        yield return new WaitForSeconds(2f);

        m_Text.text = string.Empty;
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

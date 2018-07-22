using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour {

    public Stats EnemyStats;

    private EnemyMovement m_EnemyMovement;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private GameObject m_AlarmImage;
    [SerializeField] private string Phrase;
    [SerializeField] private float WaitTimeAfterSpot = 2f;
    private float m_UpdateTime = 0f;

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

    private string ChangePhrase()
    {
        return Phrase.Replace("<br>", "\n");
    }

    private void Update()
    {
        if (m_EnemyMovement.isWaiting)
        {
            if (m_UpdateTime < Time.time)
            {
                m_UpdateTime = Time.time + 0.5f;

                if (m_AlarmImage.activeSelf)
                    m_EnemyMovement.isWaiting = false;
            }
        }
    }

    private IEnumerator ShowPhrase()
    {
        m_Text.text = ChangePhrase();
        m_EnemyMovement.isWaiting = true;

        yield return new WaitForSeconds(WaitTimeAfterSpot);

        m_Text.text = string.Empty;
        m_EnemyMovement.isWaiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            PlayerSpot(false);
            AttackPlayer(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerSpot(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerSpot(false);
        }
    }

    private void AttackPlayer(Collision2D collision)
    {
        collision.transform.GetComponent<Player>().playerStats.TakeDamage(999);
        StartCoroutine(ShowPhrase());
    }

    private void PlayerSpot(bool isSpot)
    {
        m_AlarmImage.SetActive(isSpot);

        if (isSpot)
            m_EnemyMovement.Speed = 2f;
        else
            m_EnemyMovement.Speed = 1f;
    }
}

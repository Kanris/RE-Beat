using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyMovement), typeof(EnemyStatsGO))]
public class PatrolEnemy : MonoBehaviour {

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnPlayerSpot;

    private Enemy m_EnemyStats;
    private EnemyMovement m_EnemyMovement;
    private TextMeshProUGUI m_Text;
    private float m_UpdateTime = 0f;

    [SerializeField] private SpriteRenderer m_AlarmImage;
    [SerializeField] private float WaitTimeAfterSpot = 2f;

	// Use this for initialization
	void Start () {

        InitializeStats();

        InitializeEnemyMovement();

        m_AlarmImage.gameObject.SetActive(false);
    }

    private void InitializeStats()
    {
        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats;
    }

    private void Update()
    {
        if (m_EnemyStats.IsPlayerNear)
        {
            if (GameMaster.Instance.isPlayerDead)
            {
                m_EnemyStats.ChangeIsPlayerNear(false);
                StartCoroutine(PlayerSpot(false));
            }
        }
    }

    private void InitializeEnemyMovement()
    {
        m_EnemyMovement = GetComponent<EnemyMovement>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            AttackPlayer(collision);

            if (!m_EnemyStats.IsPlayerNear)
            {
                m_EnemyMovement.TurnAround();
            }
        }
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_EnemyStats.ChangeIsPlayerNear(true);

            yield return PlayerSpot(m_EnemyStats.IsPlayerNear);
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_EnemyStats.ChangeIsPlayerNear(false);

            yield return new WaitForSeconds(1f);

            if (!m_EnemyStats.IsPlayerNear)
                yield return PlayerSpot(false);
        }
    }

    private void AttackPlayer(Collision2D collision)
    {
        collision.transform.GetComponent<Player>().playerStats.TakeDamage(m_EnemyStats.DamageAmount);       
    }

    private IEnumerator PlayerSpot(bool isSpot)
    {
        yield return null;

        m_AlarmImage.gameObject.SetActive(isSpot);

        if (OnPlayerSpot != null)
            OnPlayerSpot(false);

        if (isSpot)
            m_EnemyStats.ChangeSpeed(2f);
        else
            m_EnemyStats.ChangeSpeed(1f);
    }
}

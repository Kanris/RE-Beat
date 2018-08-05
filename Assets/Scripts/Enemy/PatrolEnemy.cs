using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyMovement))]
public class PatrolEnemy : MonoBehaviour {

    public Enemy EnemyStats;

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

    private void Update()
    {
        if (EnemyStats.IsPlayerNear)
        {
            if (GameMaster.Instance.isPlayerDead)
            {
                EnemyStats.IsPlayerNear = false;
                StartCoroutine(PlayerSpot(false));
            }
        }
    }

    private void InitializeStats()
    {
        EnemyStats.Initialize(gameObject, GetComponent<Animator>());
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

            if (!EnemyStats.IsPlayerNear)
            {
                m_EnemyMovement.TurnAround();
            }
        }
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EnemyStats.IsPlayerNear = true;

            yield return PlayerSpot(EnemyStats.IsPlayerNear);
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EnemyStats.IsPlayerNear = false;

            yield return new WaitForSeconds(1f);

            if (!EnemyStats.IsPlayerNear)
                yield return PlayerSpot(false);
        }
    }

    private void AttackPlayer(Collision2D collision)
    {
        collision.transform.GetComponent<Player>().playerStats.TakeDamage(EnemyStats.DamageAmount);       
    }

    private IEnumerator PlayerSpot(bool isSpot)
    {
        yield return null;

        m_AlarmImage.gameObject.SetActive(isSpot);

        m_EnemyMovement.isWaiting = false;

        if (isSpot)
            m_EnemyMovement.Speed = 2f;
        else
            m_EnemyMovement.Speed = 1f;
    }
}

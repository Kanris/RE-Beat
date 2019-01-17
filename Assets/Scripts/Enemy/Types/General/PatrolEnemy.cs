using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement), typeof(EnemyStatsGO))]
public class PatrolEnemy : MonoBehaviour {

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnPlayerSpot;

    private Enemy m_EnemyStats;

    [SerializeField] private SpriteRenderer m_AlarmImage;

    [Header("Move Information")]
    [SerializeField, Range(0.1f, 20f)] private float m_IdleSpeed = 1f;
    [SerializeField, Range(0.1f, 20f)] private float m_SpeedUpSpeed = 2f;
    [SerializeField, Range(0f, 20f)] private float WaitTimeAfterSpot = 2f;

    [Header("Death conditions")]
    [SerializeField, Range(-100f, 100f)] private float m_YBoundaries = -30f;
    

	// Use this for initialization
	void Start () {

        InitializeStats();
        
        m_AlarmImage.gameObject.SetActive(false);
    }

    private void InitializeStats()
    {
        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats;
    }

    private void Update()
    {
        if (GameMaster.Instance.IsPlayerDead && GameMaster.Instance.m_Player == null)
        {
            if (m_EnemyStats.IsPlayerNear)
            {
                m_EnemyStats.ChangeIsPlayerNear(false);
                PlayerSpot(false);
            }
        }

        if (transform.position.y < m_YBoundaries)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();

            m_EnemyStats.ChangeIsPlayerNear(true);

            PlayerSpot(m_EnemyStats.IsPlayerNear);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_EnemyStats.IsPlayerNear)
        {
            StartCoroutine(ResetState());
        }
    }

    private IEnumerator ResetState()
    {
        yield return new WaitForSeconds(WaitTimeAfterSpot);

        m_EnemyStats.ChangeIsPlayerNear(false);

        PlayerSpot(false);
    }

    private void PlayerSpot(bool isSpot)
    {
        m_AlarmImage.gameObject.SetActive(isSpot);

        if (OnPlayerSpot != null)
            OnPlayerSpot(false); //continue moving 

        if (isSpot)
            m_EnemyStats.ChangeSpeed(m_SpeedUpSpeed);
        else
            m_EnemyStats.ChangeSpeed(m_IdleSpeed);
    }
}

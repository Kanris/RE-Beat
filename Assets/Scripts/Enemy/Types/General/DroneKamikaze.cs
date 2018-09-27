using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyStatsGO))]
public class DroneKamikaze : MonoBehaviour {
    
    private Rigidbody2D m_Rigidbody;
    private Animator m_Animator;
    private Enemy m_EnemyStats;
    private float m_PrevXPosition;

    private float m_VelocityMin = -5f;
    private float m_VelocityMax = 5f;

    // Use this for initialization
    void Start () {
        
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats;

        int randX = Random.Range(0, 2);
        int randY = Random.Range(0, 2);

        m_Rigidbody.velocity = new Vector2(randX == 0 ? -2f : 2f, randY == 0 ? -2f : 2f);
        m_PrevXPosition = transform.position.x;
    }

    private void FixedUpdate()
    {
        if (m_PrevXPosition < transform.position.x)
        {
            m_Animator.SetFloat("Direction", 1f);
        }
        else
        {
            m_Animator.SetFloat("Direction", -1f);
        }

        m_PrevXPosition = transform.position.x;

        m_Rigidbody.velocity = new Vector2( Mathf.Clamp(m_Rigidbody.velocity.x, m_VelocityMin, m_VelocityMax), Mathf.Clamp(m_Rigidbody.velocity.y, m_VelocityMin, m_VelocityMax));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            m_EnemyStats.HitPlayer(collision.transform.GetComponent<Player>().playerStats);
            m_EnemyStats.TakeDamage(999);
        }
    }

}

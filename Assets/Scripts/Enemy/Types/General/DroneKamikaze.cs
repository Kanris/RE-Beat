using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class DroneKamikaze : MonoBehaviour {

    [SerializeField] private GameObject DeathParticles;
    [SerializeField, Range(0, 10)] private int DamageAmount = 1;

    private Rigidbody2D m_Rigidbody;
    private Animator m_Animator;
    private float m_PrevXPosition;

    private float m_VelocityMin = -5f;
    private float m_VelocityMax = 5f;

    private bool m_IsDestroying = false;
    private PlayerStats m_PlayerStats;

    // Use this for initialization
    void Start () {
        
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();

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
        if (collision.transform.CompareTag("Player") & !m_IsDestroying)
        {
            collision.transform.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);
            StartCoroutine( DestroyDrone() );
        }

        if (collision.gameObject.layer == 14 & m_IsDestroying) //object layer - ground
        {
            StartCoroutine( DestroyDrone(2f) );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttackRange") & !m_IsDestroying)
        {
            m_Rigidbody.gravityScale = 2f;
            m_Rigidbody.sharedMaterial = null;
            m_IsDestroying = true;
        }

        if (collision.CompareTag("Player") & m_IsDestroying)
        {
            m_PlayerStats = collision.GetComponent<Player>().playerStats;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsDestroying)
        {
            m_PlayerStats = null;
        }
    }

    private IEnumerator DestroyDrone(float waitTimeBeforeDestroy = 0f)
    {
        yield return new WaitForSeconds(waitTimeBeforeDestroy);

        var destroyParticles = Instantiate(DeathParticles, transform.position, Quaternion.identity);
        Destroy(destroyParticles, 1f);

        if (m_PlayerStats != null)
            m_PlayerStats.TakeDamage(DamageAmount);

        Destroy(gameObject);
    }
}

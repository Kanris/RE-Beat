using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RangeEnemy : MonoBehaviour {

    public MageEnemyStats EnemyStats;

    private EnemyMovement m_EnemyMovement;
    private bool m_IsPlayerDead = false;
    private bool m_CanCreateNewFireball = true;
    private Animator m_Animator;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private GameObject m_AlarmImage;



    // Use this for initialization
    void Start()
    {
        InitializeStats();

        InitializeEnemyMovement();

        InitializeAnimator();
    }

    private void InitializeStats()
    {
        EnemyStats.Initialize(gameObject);
    }

    private void InitializeEnemyMovement()
    {
        m_EnemyMovement = GetComponent<EnemyMovement>();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
            Debug.LogError("RangeEnemy.InitializeAnimator: Can't find animator on GameObject");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KillPlayer(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (m_IsPlayerDead)
            m_IsPlayerDead = false;
    }

    private void KillPlayer(Collision2D collision)
    {
        m_IsPlayerDead = true;
        m_EnemyMovement.isWaiting = false;
        collision.transform.GetComponent<Player>().playerStats.TakeDamage(999);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerDead)
        {
            StartCoroutine(CreateFireball());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerDead & m_CanCreateNewFireball)
        {
            StartCoroutine(CreateFireball());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerDead)
        {
            m_EnemyMovement.isWaiting = false;
        }
    }

    private IEnumerator CreateFireball()
    {
        if (m_CanCreateNewFireball)
        {
            m_EnemyMovement.isWaiting = true;
            Animate(true);

            m_CanCreateNewFireball = false;

            yield return new WaitForSeconds(0.6f);

            Animate(false);

            var newFireball = Resources.Load("Fireball");

            var instantiateFireball = Instantiate(newFireball, transform.position, transform.rotation);

            yield return new WaitForSeconds(EnemyStats.AttackSpeed);

            m_CanCreateNewFireball = true;
        }
    }

    private void Animate(bool isAttacking)
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool("isAttacking", isAttacking);    
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {

    [SerializeField, Range(-3, -200)] private float YBoundaries = -20f;
    [SerializeField, Range(-3, -20)] private float YFallDeath = -3f;
    [SerializeField] private float ThrowX = 0.08f;
    [SerializeField] private float ThrowY = 0.1f;
    [SerializeField] private GameObject AttackRange;

    private float m_YPositionBeforeJump;
    private Vector2 m_ThrowBackVector;
    private Animator m_Animator;
    private bool isAttacking = false;

    public PlayerStats playerStats;
    [HideInInspector] public bool isPlayerThrowingBack;
    [HideInInspector] public bool isTriggered;

    private void Start()
    {
        playerStats.Initialize(gameObject);

        InitializeAnimator();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Player.InitializeAnimator: Can't find Animator component on Gameobject");
        }
    }

    // Update is called once per frame
    private void Update () {
		
        if (transform.position.y <= YBoundaries)
        {
            playerStats.TakeDamage(playerStats.MaxHealth);
        }

        JumpHeightControl();

        if (!AttackRange.activeSelf)
        {
            if (!isAttacking)
            {
                if (CrossPlatformInputManager.GetButtonDown("Fire1"))
                {
                    StartCoroutine(Attack());
                }
            }
        }
    }

    private IEnumerator Attack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            AttackRange.SetActive(true);

            yield return new WaitForSeconds(PlayerStats.AttackSpeed);
            isAttacking = false;
            AttackRange.SetActive(isAttacking);
        }
    }

    private void FixedUpdate()
    {
        if (m_Animator.GetBool("Damage"))
        {
            if (!isPlayerThrowingBack)
            {
                if (transform.localScale.x == 1)
                {
                    m_ThrowBackVector = new Vector2(-ThrowX, ThrowY);
                }
                else
                {
                    m_ThrowBackVector = new Vector2(ThrowX, ThrowY);
                }

                if (!isTriggered)
                {
                    m_ThrowBackVector = new Vector2(-m_ThrowBackVector.x, m_ThrowBackVector.y);
                }

                isPlayerThrowingBack = true;
            }
            else
            {
                GetComponent<Rigidbody2D>().position += m_ThrowBackVector;
            }
        }
    }

    private void JumpHeightControl()
    {
        if (!m_Animator.GetBool("Ground"))
        {
            if (m_YPositionBeforeJump + YFallDeath >= transform.position.y & !GameMaster.Instance.isPlayerDead)
            {
                playerStats.TakeDamage(999);
            }
        }
        else
        {
            m_YPositionBeforeJump = transform.position.y;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") & isAttacking)
        {
            Debug.LogError("Attack");
            var enemyStats = GetStats(collision);

            if (enemyStats != null)
            {
                if (collision.GetComponent<EnemyMovement>().Speed == 1f)
                {
                    collision.GetComponent<EnemyMovement>().TurnAround();
                }

                enemyStats.TakeDamage(PlayerStats.DamageAmount);
            }
        }
    }

    private Stats GetStats(Collider2D collision)
    {
        Stats enemyStats = null;

        if (collision.GetComponent<PatrolEnemy>() != null)
        {
            enemyStats = collision.GetComponent<PatrolEnemy>().EnemyStats;
        }
        else if (collision.GetComponent<RangeEnemy>() != null)
        {
            enemyStats = collision.GetComponent<RangeEnemy>().EnemyStats;
        }

        return enemyStats;
    }
}

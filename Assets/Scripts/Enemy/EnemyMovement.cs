using System.Collections;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyStatsGO))]
public class EnemyMovement : MonoBehaviour {

    #region private fields

    private Rigidbody2D m_Rigidbody2D;
    private Animator m_Animator;
    private Enemy m_EnemyStats;
    private Vector2 m_PreviousPosition = Vector2.zero;

    private float m_ThrowUpdateTime;
    private float m_Speed = 1f;
    private bool m_IsWaiting = false;
    private bool m_IsJumping = false;
    private bool m_IsThrowBack;
    private bool m_CantMoveFurther;

    public float m_MoveUpdateTime;
    private float m_MinMoveTime = 0.5f;
    private float m_MaxMoveTime = 4f;

    #endregion

    #region inspector fields

    [SerializeField] private float IdleTime = 2f;

    #endregion

    #region public fields

    public bool isPlayerNear; //for jump

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        InitializeRigidBody();

        InitializeAnimator();

        SubscribeOnEvents();

        SpeedChange(GetDefaultSpeed());

        InitializeEnemyStats();
    }

    #region Initialize

    private void InitializeEnemyStats()
    {
        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats;
    }

    private void InitializeRigidBody()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (m_Rigidbody2D == null)
        {
            Debug.LogError("EnemyMovement.InitializeRigidBody: Can't find rigidbody on gameobject");
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("EnemyMovement.InitializeAnimator: Can't find animator on gameobject");
        }
    }

    private void SubscribeOnEvents()
    {
        if (GetComponent<EnemyStatsGO>() != null)
        {
            var enemy = GetComponent<EnemyStatsGO>();

            enemy.EnemyStats.OnSpeedChange += SpeedChange;
            enemy.EnemyStats.OnEnemyTakeDamage += isPlayerNear =>
            {
                if (!isPlayerNear)
                {
                    TurnAround();
                }
            };

            enemy.EnemyStats.OnPlayerHit += isPlayerNear =>
            {
                if (!isPlayerNear)
                    TurnAround();
            };

            enemy.EnemyStats.OnEnemyTakeDamage += ThrowBack;
        }

        if (GetComponent<PatrolEnemy>() != null)
            GetComponent<PatrolEnemy>().OnPlayerSpot += ChangeWaitingState;
        else
            GetComponent<RangeEnemy>().OnPlayerSpot += ChangeWaitingState;
    }

        private float GetDefaultSpeed()
    {
        var defaultSpeed = 0f;

        if (GetComponent<EnemyStatsGO>() != null)
        {
            defaultSpeed = GetComponent<EnemyStatsGO>().EnemyStats.Speed;
        }

        return defaultSpeed;
    }

    #endregion

    private void FixedUpdate()
    {
        if (m_CantMoveFurther)
        {
            m_CantMoveFurther = false;
            m_IsWaiting = false;
            TurnAround();
        }

        if (!m_IsWaiting & !m_IsThrowBack)
        {
            if (m_MoveUpdateTime > Time.time | isPlayerNear | m_EnemyStats.IsPlayerNear)
            {
                if (!m_Animator.GetBool("isWalking"))
                    SetAnimation(true);

                m_Rigidbody2D.position += new Vector2(-transform.localScale.x, 0) * Time.fixedDeltaTime * m_Speed;

                if (m_Rigidbody2D.position == m_PreviousPosition & !m_IsWaiting)
                {
                    if (isPlayerNear) StartCoroutine(Jump());
                    else m_CantMoveFurther = true;
                }
                else
                    m_PreviousPosition = m_Rigidbody2D.position;

                if (isPlayerNear | m_EnemyStats.IsPlayerNear)
                    m_MoveUpdateTime = Time.time + 2.2f;
            }
            else
                StartCoroutine(Idle(false));
        }
        else if (m_Animator.GetBool("isWalking"))
            SetAnimation(false);

        if (m_IsJumping)
        {
            m_Rigidbody2D.velocity = new Vector2(-0.001f, 10f);
        }

        if (m_IsThrowBack)
        {
            if (m_ThrowUpdateTime <= Time.time)
            {
                m_Rigidbody2D.velocity = Vector2.zero;
                m_IsThrowBack = false;
            }
            else
            {
                var multiplier = GetMultiplier();

                m_Rigidbody2D.velocity = new Vector2(m_EnemyStats.m_ThrowX * multiplier, 0f);
            }
        }
    }

    private int GetMultiplier()
    {
        var multiplier = Convert.ToInt32(transform.localScale.x);

        if (!m_EnemyStats.IsPlayerNear)
            multiplier *= -1;

        return multiplier;
    }

    private void ThrowBack(bool value)
    {
        if (!m_EnemyStats.m_IsBigMonster)
        {
            m_IsThrowBack = true;
            m_ThrowUpdateTime = Time.time + 0.2f;
        }
    }

    private IEnumerator Jump()
    {
        m_IsJumping = true;
        Physics2D.IgnoreLayerCollision(8, 13);

        yield return new WaitForSeconds(0.2f);

        Physics2D.IgnoreLayerCollision(8, 13, false);
        m_IsJumping = false;
        m_Rigidbody2D.velocity = Vector2.zero;
    }

    private void SpeedChange(float speed)
    {
        m_Speed = speed;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") & !isPlayerNear)
        {
            m_CantMoveFurther = true;
        }
    }

    private IEnumerator Idle(bool haveToTurnAround)
    {
        if (!m_IsWaiting)
        {
            ChangeWaitingState(true);
            SetAnimation(false);

            yield return new WaitForSeconds(IdleTime);

            m_MoveUpdateTime = Time.time + Random.Range(m_MinMoveTime, m_MaxMoveTime);

            if (!haveToTurnAround)
            {
                var isWantToTurnAround = Random.Range(0, 2);

                if (isWantToTurnAround == 1)
                    TurnAround();
            }

            ChangeWaitingState(false);
        }
    }

    private void SetAnimation(bool value)
    {
        m_Animator.SetBool("isWalking", value);
    }

    #endregion

    #region public methods

    public void TurnAround()
    {
        transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
    }

    public void ChangeWaitingState(bool value)
    {
        m_IsWaiting = value;

    }

    #endregion
}

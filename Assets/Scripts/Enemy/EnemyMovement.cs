using System.Collections;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyStatsGO))]
public class EnemyMovement : MonoBehaviour {

    #region private fields

    private Rigidbody2D m_Rigidbody2D;
    private Animator m_Animator;
    private Vector2 m_PreviousPosition = Vector2.zero;
    private bool m_IsWaiting = false;
    private bool m_IsJumping = false;
    private float m_Speed = 1f;
    private Enemy m_EnemyStats;
    private bool m_IsThrowBack;
    private float m_UpdateTime;

    #endregion

    #region inspector fields

    [SerializeField] private float IdleTime = 2f;
    [HideInInspector] public float m_PosX = -1f;

    #endregion

    #region public fields

    #region events

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnWaitingStateChange;

    #endregion

    public bool isPlayerNear;

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        InitializeRigidBody();

        InitializeAnimator();

        SubscribeOnEvents();

        SpeedChange(GetDefaultSpeed());

        GetComponent<EnemyStatsGO>().EnemyStats.OnEnemyTakeDamage += ThrowBack;

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
                    TurnAround();
            };
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
        if (!m_IsWaiting)
        {
            if (!m_Animator.GetBool("isWalking"))
                SetAnimation();

            m_Rigidbody2D.position += new Vector2(m_PosX, 0) * Time.fixedDeltaTime * m_Speed;
            SetAnimation();

            if (m_Rigidbody2D.position == m_PreviousPosition & !m_IsWaiting)
            {
                if (isPlayerNear) StartCoroutine(Jump());
                else StartCoroutine(Idle());
            }
            else
                m_PreviousPosition = m_Rigidbody2D.position;
        }
        else if (m_Animator.GetBool("isWalking"))
        {
            SetAnimation();
        }

        if (m_IsJumping)
        {
            m_Rigidbody2D.velocity = new Vector2(-0.001f, 10f);
        }

        if (m_IsThrowBack)
        {
            if (m_UpdateTime <= Time.time)
            {
                m_Rigidbody2D.velocity = Vector2.zero;
                m_IsThrowBack = false;
            }
            else
            {
                var multiplier = GetMultiplier();

                m_Rigidbody2D.velocity = new Vector2(m_EnemyStats.m_ThrowX * multiplier, m_EnemyStats.m_ThrowY);
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
            m_UpdateTime = Time.time + 0.2f;

            if (GetComponent<PatrolEnemy>() != null)
                GetComponent<EnemyMovement>().TurnAround();
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
            StartCoroutine(Idle());
        }
    }

    private IEnumerator Idle()
    {
        if (!m_IsWaiting)
        {
            ChangeWaitingState(true);

            TurnAround();
            SetAnimation();

            yield return new WaitForSeconds(IdleTime);

            ChangeWaitingState(false);
        }
    }

    private void SetAnimation()
    {
        m_Animator.SetBool("isWalking", !m_IsWaiting);
    }

    #endregion

    #region public methods

    public void TurnAround()
    {
        transform.localScale = new Vector3(m_PosX, 1, 1);
        m_PosX = -m_PosX;
    }

    public void ChangeWaitingState(bool value)
    {
        m_IsWaiting = value;

        if (OnWaitingStateChange != null)
            OnWaitingStateChange(value);
    }

    #endregion
}

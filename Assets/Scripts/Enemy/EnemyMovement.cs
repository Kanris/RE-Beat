using System.Collections;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyStatsGO))]
public class EnemyMovement : MonoBehaviour {

    #region private fields

    public delegate void VoidDelegateFloat(float value);
    public VoidDelegateFloat OnEnemyTurnAround;

    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character

    [Header("Walking type")]
    [SerializeField] private bool m_IsSimpleMovement; //from collider to collider

    const float k_GroundedRadius = .05f; // Radius of the overlap circle to determine if grounded

    private Rigidbody2D m_Rigidbody2D;
    private Animator m_Animator;
    private Enemy m_EnemyStats;
    private Vector2 m_PreviousPosition = Vector2.zero;

    private float m_ThrowUpdateTime;
    private float m_Speed = 10f;
    private bool m_IsWaiting = false;
    private bool m_IsThrowBack;
    private bool m_Grounded;
    private bool m_CantMoveFurther;

    private float m_MoveUpdateTime;
    private float m_MinMoveTime = 0.5f;
    private float m_MaxMoveTime = 4f;

    [HideInInspector] public int m_Direction = -1;
    #endregion

    #region inspector fields

    [SerializeField, Range(0f, 20f)] private float IdleTime = 2f;

    #endregion

    #region private methods

    #region Initialize

    // Use this for initialization
    private void Start() {

        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        m_Animator = GetComponent<Animator>();

        SubscribeOnEvents();

        SpeedChange(GetComponent<EnemyStatsGO>().EnemyStats.Speed);

        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats;

        OnEnemyTurnAround = GetComponent<EnemyStatsGO>().ChangeUIScale;
    }

    private void SubscribeOnEvents()
    {
        if (GetComponent<EnemyStatsGO>() != null)
        {
            var enemy = GetComponent<EnemyStatsGO>();

            enemy.EnemyStats.OnSpeedChange += SpeedChange;

            enemy.EnemyStats.OnEnemyTakeDamage += StartThrowBack;
        }

        if (GetComponent<PatrolEnemy>() != null)
            GetComponent<PatrolEnemy>().OnPlayerSpot += ChangeWaitingState;
        else if (GetComponent<RangeEnemy>() != null)
            GetComponent<RangeEnemy>().OnPlayerSpot += ChangeWaitingState;
    }

    #endregion

    private void Update()
    {
        m_Grounded = false;

        m_Grounded = CheckGround(m_GroundCheck.position);

        if (m_Grounded)
        {
            if(CheckGround(m_GroundCheck.position.Add(y: 1f))) //if enemy stack
            {
                m_CantMoveFurther = true;
            }
        }
        else
        {
            m_CantMoveFurther = true;
        }

        if (m_CantMoveFurther)
        {
            m_CantMoveFurther = false;
            TurnAround();
        }
    }

    private bool CheckGround(Vector3 position)
    {
        var grounded = false;

        var colliders = Physics2D.OverlapCircleAll(position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                grounded = true;
                break;
            }
        }

        return grounded;
    }

    private void FixedUpdate()
    {
        //Debug.LogError(m_IsWaiting + " " + m_IsThrowBack);

        if (!m_IsWaiting & !m_IsThrowBack) //if enemey is not waiting or is not throwing back
        {
            if ((m_MoveUpdateTime > Time.time | m_EnemyStats.IsPlayerNear) | m_IsSimpleMovement)
            {
                m_Rigidbody2D.velocity = new Vector2(-transform.localScale.x * m_Speed, 0); //move enemy

                if (!m_Animator.GetBool("isWalking")) //if enemy still not playing move animation
                    SetAnimation(true);

                if (m_EnemyStats.IsPlayerNear) //Continue pursuit time if enemy will loose sight of player
                    m_MoveUpdateTime = Time.time + 2.2f;
            }
            else
                StartCoroutine(Idle());
        }
        else if (m_Animator.GetBool("isWalking"))
            SetAnimation(false);

        if (m_IsThrowBack)
        {
            if (m_ThrowUpdateTime > Time.time)
            {
                if (m_ThrowUpdateTime > Time.time)
                {
                    if (m_Direction == -1)
                        m_Direction = GetDirection();

                    m_Rigidbody2D.velocity = new Vector2(m_EnemyStats.m_ThrowX * m_Direction, 0f);

                    SetAnimation(false);
                }
            }
            else
            {
                m_Direction = -1;

                m_Rigidbody2D.velocity = Vector2.zero;
                m_IsThrowBack = false;
            }
        }
    }

    private void StartThrowBack(bool value, int divider)
    {
        if (!m_EnemyStats.m_IsBigMonster)
        {
            if (divider > 0)
            {
                m_IsThrowBack = true;
                m_ThrowUpdateTime = Time.time + 0.2f;
            }
        }
    }

    private int GetDirection()
    {
        if (GameMaster.Instance.m_Player != null)
        {
            var distance = GameMaster.Instance.m_Player.transform.GetChild(0).position - transform.position;
            return distance.x > 0 ? -1 : 1;
        }

        return 0;
    }

    private IEnumerator Idle()
    {
        if (!m_IsWaiting)
        {
            ChangeWaitingState(true);
            SetAnimation(false);

            m_Rigidbody2D.velocity = Vector2.zero;

            yield return new WaitForSeconds(IdleTime);

            if (!m_EnemyStats.IsPlayerNear)
            {
                m_MoveUpdateTime = Time.time + Random.Range(m_MinMoveTime, m_MaxMoveTime);

                if (Random.Range(0, 2) == 1)
                    TurnAround();

                ChangeWaitingState(false);
            }
        }
    }

    private void SpeedChange(float speed)
    {
        m_Speed = speed;
    }

    private void SetAnimation(bool value)
    {
        m_Animator.SetBool("isWalking", value);
    }

    #endregion

    #region public methods

    public void TurnAround()
    {
        OnEnemyTurnAround(-transform.localScale.x);
        transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
    }

    public void ChangeWaitingState(bool value)
    {
        m_IsWaiting = value;

    }

    #endregion
}

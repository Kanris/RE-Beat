﻿using System.Collections;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyStatsGO))]
public class EnemyMovement : MonoBehaviour {

    #region private fields

    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character

    const float k_GroundedRadius = .01f; // Radius of the overlap circle to determine if grounded

    private Rigidbody2D m_Rigidbody2D;
    private Animator m_Animator;
    private Enemy m_EnemyStats;
    private Vector2 m_PreviousPosition = Vector2.zero;

    private float m_ThrowUpdateTime;
    private float m_Speed = 1f;
    private bool m_IsWaiting = false;
    private bool m_IsThrowBack;
    private bool m_Grounded;
    private bool m_CantMoveFurther;

    private float m_MoveUpdateTime;
    private float m_MinMoveTime = 0.5f;
    private float m_MaxMoveTime = 4f;
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
        else
            GetComponent<RangeEnemy>().OnPlayerSpot += ChangeWaitingState;
    }

    #endregion

    private void Update()
    {
        m_Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;
        }

        if (!m_Grounded)
        {
            TurnAround();
        }
        else if (m_CantMoveFurther)
        {
            m_CantMoveFurther = false;
            TurnAround();
        }
    }

    private void FixedUpdate()
    {
        if (!m_IsWaiting & !m_IsThrowBack) //if enemey is not waiting or is not throwing back
        {
            if ((m_MoveUpdateTime > Time.time | m_EnemyStats.IsPlayerNear))
            {
                m_Rigidbody2D.position += new Vector2(-transform.localScale.x, 0) * Time.fixedDeltaTime * m_Speed; //move enemy

                if (!m_Animator.GetBool("isWalking")) //if enemy still not playing move animation
                    SetAnimation(true);

                if (m_EnemyStats.IsPlayerNear) //Continue pursuit time if enemy will loose sight of player
                    m_MoveUpdateTime = Time.time + 2.2f;

                CheckStackPosition();
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
                var multiplier = GetMultiplier();
                m_Rigidbody2D.velocity = new Vector2(m_EnemyStats.m_ThrowX * multiplier, 0f);
            }
            else
                m_IsThrowBack = false;
        }
    }

    private void StartThrowBack(bool value)
    {
        if (!m_EnemyStats.m_IsBigMonster)
        {
            m_IsThrowBack = true;
            m_ThrowUpdateTime = Time.time + 0.07f;
        }
    }

    private void CheckStackPosition()
    {
        if (m_Rigidbody2D.position == m_PreviousPosition & !m_IsWaiting) //if enemy can't move further
        {
            m_CantMoveFurther = true; //turn around
        }
        else
            m_PreviousPosition = m_Rigidbody2D.position; //remember this position maybe next will be same
    }

    private int GetMultiplier()
    {
        var multiplier = Convert.ToInt32(transform.localScale.x);

        if (!m_EnemyStats.IsPlayerNear)
            multiplier *= -1;

        return multiplier;
    }

    private void SpeedChange(float speed)
    {
        m_Speed = speed;
    }

    private IEnumerator Idle()
    {
        if (!m_IsWaiting)
        {
            ChangeWaitingState(true);
            SetAnimation(false);

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

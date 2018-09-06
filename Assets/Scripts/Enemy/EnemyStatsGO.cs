using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class EnemyStatsGO : MonoBehaviour {

    public Enemy EnemyStats;

    [SerializeField] private bool DontResurect;

    private Rigidbody2D m_Rigidbody;
    private bool m_IsThrowBack;
    private float m_UpdateTime;

    #region initialize

    private void Awake()
    {
        InitializeRigidbody();
    }

    private void InitializeRigidbody()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    #endregion

    // Use this for initialization
    void Start () {

        InitializeStats();

        if (DontResurect)
            EnemyStats.OnObjectDeath += SaveState;

        EnemyStats.OnEnemyTakeDamage += ThrowBack;
    }

    private void Update()
    {
        if (m_IsThrowBack)
        {
            if (m_UpdateTime <= Time.time)
            {
                m_Rigidbody.velocity = Vector2.zero;
                m_IsThrowBack = false;
            }
            else
            {
                var multiplier = GetMultiplier();

                m_Rigidbody.velocity = new Vector2(EnemyStats.m_ThrowX * multiplier, EnemyStats.m_ThrowY);
            }
        }
    }

    public void InitializeStats()
    {
        EnemyStats.Initialize(gameObject, GetComponent<Animator>());
    }

    public void SaveState()
    {
        GameMaster.Instance.SaveState(transform.name, 0, GameMaster.RecreateType.Object);
    }

    private int GetMultiplier()
    {
        var multiplier = Convert.ToInt32(transform.localScale.x);

        if (!EnemyStats.IsPlayerNear)
            multiplier *= -1;

        return multiplier;
    }

    private void ThrowBack(bool value)
    {
        if (!EnemyStats.m_IsBigMonster)
        {
            m_IsThrowBack = true;
            m_UpdateTime = Time.time + 0.2f;

            if (GetComponent<PatrolEnemy>() != null)
                GetComponent<EnemyMovement>().TurnAround();
        }
    }
}

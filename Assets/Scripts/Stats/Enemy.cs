﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy : Stats
{
    #region delegates

    public delegate void VoidFloatDelegate(float value);
    public event VoidFloatDelegate OnSpeedChange;

    public delegate void VoidBoolDelegate(bool value);
    public event VoidBoolDelegate OnEnemyTakeDamage;
    public event VoidBoolDelegate OnPlayerHit;

    #endregion

    #region public fields

    public int DamageAmount = 1;
    public float Speed = 1f;
    public float AttackSpeed = 2f;
    public bool m_IsBigMonster;
    [SerializeField, Range(1, 100)] private int DropCoins = 1;

    #endregion

    #region private fields

    private bool m_IsPlayerNear;

    #endregion

    #region properties

    public bool IsPlayerNear
    {
        get
        {
            return m_IsPlayerNear;
        }
    }

    #endregion

    #region override

    public override void Initialize(GameObject gameObject, Animator animator = null)
    {
        OnObjectDeath += GiveCoinsToPlayer;

        base.Initialize(gameObject, animator);
    }

    public override void TakeDamage(int amount, int divider = 1)
    {
        if (OnEnemyTakeDamage != null)
        {
            OnEnemyTakeDamage(m_IsPlayerNear);
        }

        base.TakeDamage(amount, divider);
    }

    public void HitPlayer(PlayerStats player)
    {
        if (OnPlayerHit != null)
            OnPlayerHit(m_IsPlayerNear);

        player.TakeDamage(DamageAmount);
    }

    #endregion

    #region public methods

    public void ChangeIsPlayerNear(bool value)
    {
        m_IsPlayerNear = value;
    }

    public void ChangeSpeed(float value)
    {
        Speed = value;

        if (OnSpeedChange != null)
            OnSpeedChange(value);
    }

    #endregion

    #region private methods

    private void GiveCoinsToPlayer()
    {
        PlayerStats.Coins = DropCoins;
    }

    #endregion
}

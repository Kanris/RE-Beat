using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stats {

    public int MaxHealth = 200;

    private int m_CurrentHealth;
    private int CurrentHealth
    {
        get
        {
            return m_CurrentHealth;
        }
        set
        {
            m_CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
        }
    }

    private GameObject m_GameObject;

    public virtual void Initialize(GameObject gameObject)
    {
        InitializeGameObject(gameObject);
        InitializeHealth();
    }

    private void InitializeGameObject(GameObject gameObject)
    {
        if (gameObject == null)
            Debug.LogError("Stats: GameObject is null");
        else
            m_GameObject = gameObject;
    }

    private void InitializeHealth()
    {
        if (MaxHealth <= 0)
        {
            Debug.LogError("Stats: Max health is less or equals to 0. Destroying Game object");
            GameMaster.Destroy(m_GameObject);
        }
        else
            m_CurrentHealth = MaxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth == 0)
        {
            KillObject();
        }
    }

    protected virtual void KillObject()
    {
        GameMaster.Destroy(m_GameObject);
    }
	
}

[System.Serializable]
public class PlayerStats : Stats
{
    public static int GemsAmount;

    protected override void KillObject()
    {
        GameMaster.Instance.RespawnPlayer();
        base.KillObject();
    }
}

[System.Serializable]
public class EnemyStats : Stats
{

}

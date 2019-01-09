using UnityEngine;
using System.Collections;

[System.Serializable]
public class Enemy : Stats
{
    #region delegates

    public delegate void VoidFloatDelegate(float value);
    public event VoidFloatDelegate OnSpeedChange;
    public event VoidFloatDelegate OnEnemyTakeDamageValue;


    public delegate void VoidBoolIntDelegate(bool value, int divider);
    public event VoidBoolIntDelegate OnEnemyTakeDamage;

    public delegate void VoidBoolDelegate(bool value);
    public event VoidBoolDelegate OnPlayerHit;

    #endregion

    #region public fields

    [Header("Enemy main stats")]
    [Range(1, 10)]public int DamageAmount = 1;
    [Range(0.1f, 600f)] public float Speed = 1f;
    [Range(0.1f, 10f)] public float AttackSpeed = 2f;
    [Range(1, 100)] public int DropScrap = 1;

    [Header("Special stats")]
    public bool m_IsBigMonster;
    [SerializeField] private bool DontResurect;
    [SerializeField] private ShieldInfo m_ShieldInfo;
    #endregion

    #region private fields

    private bool m_IsPlayerNear;
    private bool m_IsShieldCreated;
    private bool m_IsInvincible;

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
        if (DontResurect)
            OnObjectDeath += SaveState;

        base.Initialize(gameObject, animator);
    }

    public override void TakeDamage(int amount, int divider = 1)
    {
        if (!m_IsInvincible)
        {
            if (OnEnemyTakeDamage != null)
            {
                OnEnemyTakeDamage(m_IsPlayerNear, divider);
            }

            if (OnEnemyTakeDamageValue != null)
            {
                OnEnemyTakeDamageValue(amount);
            }

            base.TakeDamage(amount, divider);

            if (m_ShieldInfo.IsHasShield)
            {
                if (CurrentHealth <= MaxHealth * 0.5f & !m_IsShieldCreated)
                {
                    m_IsShieldCreated = true;
                    m_IsInvincible = true;
                    CreateShield();
                }
            }
        }
    }

    public void HitPlayer(PlayerStats player, int damageAmount = -1)
    {
        if (OnPlayerHit != null)
            OnPlayerHit(m_IsPlayerNear);

        damageAmount = damageAmount > -1 ? damageAmount : DamageAmount;

        player.TakeDamage(damageAmount);
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

    public void SaveState()
    {
        GameMaster.Instance.SaveState(m_GameObject.transform.name, 0, GameMaster.RecreateType.Object);
    }

    #endregion

    #region private methods
    
    public void CreateShield()
    {
        if (m_ShieldInfo.ShieldType != DebuffPanel.DebuffTypes.None)
        {
            var shieldGO = Resources.Load("Effects/Shields/" + m_ShieldInfo.ShieldType) as GameObject;
            var shieldInstantiate = GameMaster.Instantiate(shieldGO, m_GameObject.transform);

            shieldInstantiate.GetComponent<EnemyShield>().OnShieldDestroy += SetInvincible;
        }
    }

    private void SetInvincible(bool value)
    {
        m_IsInvincible = value;
    }

    #endregion
}

[System.Serializable]
public class ShieldInfo
{
    public bool IsHasShield = false;
    public DebuffPanel.DebuffTypes ShieldType;
}

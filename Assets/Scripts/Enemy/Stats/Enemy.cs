using UnityEngine;
using System.Collections;

[System.Serializable]
public class Enemy : Stats
{
    #region delegates

    public delegate void VoidFloatDelegate(float value);
    public event VoidFloatDelegate OnSpeedChange;
    public event VoidFloatDelegate OnEnemyTakeDamageValue;

    public delegate void VoidBoolDelegate(bool value);
    public event VoidBoolDelegate OnPlayerHit;
    public event VoidBoolDelegate OnEnemyTakeDamage;

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

    public EnemyShield CreatedShield { set; get; }

    #endregion

    #region override

    public override void Initialize(GameObject gameObject, Animator animator = null)
    {
        if (DontResurect)
            OnObjectDeath += SaveState;

        base.Initialize(gameObject, animator);
    }

    public override void TakeDamage(int amount, float throwX, float throwY)
    {
        if (!m_IsInvincible)
        {
            OnEnemyTakeDamage?.Invoke(m_IsPlayerNear);

            OnEnemyTakeDamageValue?.Invoke(amount);

            base.TakeDamage(amount, throwX, throwY);

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

            CreatedShield = shieldInstantiate.GetComponent<EnemyShield>();
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

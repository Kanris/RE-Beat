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

    [Header("Enemy main stats")]
    [Range(1, 10)]public int DamageAmount = 1;
    [Range(0.1f, 10f)] public float Speed = 1f;
    [Range(0.1f, 10f)] public float AttackSpeed = 2f;
    [SerializeField, Range(1, 100)] private int DropCoins = 1;

    [Header("Special stats")]
    public bool m_IsBigMonster;
    [SerializeField] private bool DontResurect;

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
        OnObjectDeath += GiveCoinsToPlayer; //give player conins on death

        if (DontResurect)
            OnObjectDeath += SaveState;

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

    public void SaveState()
    {
        
        GameMaster.Instance.SaveState(m_GameObject.transform.name, 0, GameMaster.RecreateType.Object);
    }

    #endregion

    #region private methods

    private void GiveCoinsToPlayer()
    {
        PlayerStats.Coins = DropCoins;
    }

    #endregion
}

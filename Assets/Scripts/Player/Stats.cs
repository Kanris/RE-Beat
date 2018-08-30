using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[System.Serializable]
public class Stats {

    #region public fields

    public int MaxHealth = 200;

    #endregion

    #region protected fields

    protected GameObject m_GameObject;
    protected Animator m_Animator;

    #endregion

    #region private fields

    private int m_CurrentHealth;

    #region private serialize fields

    [SerializeField] private GameObject DeathParticle;
    [SerializeField] private string HitSound;
    [SerializeField] private string DeathSound;

    #endregion

    #endregion

    #region delegates

    public delegate void DeathDelegate();
    public event DeathDelegate OnObjectDeath;

    #endregion

    #region properties

    public int CurrentHealth
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

    #endregion

    #region Initialize

    public virtual void Initialize(GameObject gameObject, Animator animator = null)
    {
        InitializeGameObject(gameObject);
        InitializeHealth();

        if (animator == null)
            InitializeAnimator();
        else
            m_Animator = animator;
    }

    private void InitializeAnimator()
    {
        m_Animator = m_GameObject.GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("PlayerStats.InitializeAnimator: Can't initialize animator.");
        }
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

    #endregion

    #region virtual methods

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth == 0)
        {
            if (OnObjectDeath != null)
                OnObjectDeath();

            PlayDeathSound();
            KillObject();
        }

        if (CurrentHealth > 0)
        {
            GameMaster.Instance.StartCoroutine(PlayTakeDamageAnimation());
            PlayHitSound();
        }
    }

    protected virtual IEnumerator PlayTakeDamageAnimation()
    {
        PlayHitAnimation(true);

        yield return new WaitForSeconds(0.1f);

        PlayHitAnimation(false);
    }

    protected virtual void KillObject()
    {
        SpawnParticle();
        GameMaster.Destroy(m_GameObject);
    }

    protected virtual void SpawnParticle()
    {
        if (DeathParticle != null)
        {
            if (m_GameObject != null)
            {
                var gameObjectToDestroy =
                    GameMaster.Instantiate(DeathParticle, m_GameObject.transform.position, m_GameObject.transform.rotation);
                GameMaster.Destroy(gameObjectToDestroy, 1f);
            }
        }
    }

    #endregion

    #region protected methods

    protected void PlayHitAnimation(bool isHit)
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool("Damage", isHit);

            if (isHit)
                m_Animator.SetTrigger("DamageTrigger");
        }
    }

    #endregion

    #region private methods

    private void PlayHitSound()
    {
        if (!string.IsNullOrEmpty(HitSound))
        {
            AudioManager.Instance.Play(HitSound);
        }
    }

    private void PlayDeathSound()
    {
        if (!string.IsNullOrEmpty(DeathSound))
        {
            AudioManager.Instance.Play(DeathSound);
        }
    }

    #endregion

}

[System.Serializable]
public class PlayerStats : Stats
{
    #region delegates

    public delegate IEnumerator IEnumeratorDelegate(int value);
    public static event IEnumeratorDelegate OnCoinsAmountChange;

    #endregion

    #region public fields

    public static int DamageAmount = 50;
    public static float AttackSpeed = 0.6f;
    public static float Invincible = 2f;
    public static Inventory PlayerInventory;
    public static int CurrentPlayerHealth;
    private static int m_Coins = 0;

    #endregion

    #region properties

    public static int Coins
    {
        set
        {
            m_Coins += value;

            if (OnCoinsAmountChange != null)
                GameMaster.Instance.StartCoroutine( OnCoinsAmountChange(value) );
        }
        get
        {
            return m_Coins;
        }
    }

    #endregion

    #region private fields

    private bool isInvincible;

    #endregion

    #region override methods

    public override void Initialize(GameObject gameObject, Animator animator = null)
    {
        if (PlayerInventory == null)
            PlayerInventory = new Inventory(10);

        base.Initialize(gameObject);

        UIManager.Instance.Clear();

        if (CurrentPlayerHealth > 0)
            UIManager.Instance.AddHealth(CurrentPlayerHealth);
        else
        {
            UIManager.Instance.AddHealth(CurrentHealth);
            CurrentPlayerHealth = CurrentHealth;
        }

        UIManager.Instance.ChangeCoinsAmount(m_Coins);
    }

    public override void TakeDamage(int amount)
    {
        if (!isInvincible)
        {
            base.TakeDamage(amount);
            CurrentPlayerHealth -= amount;

            UIManager.Instance.RemoveHealth(amount);
        }
    }

    protected override IEnumerator PlayTakeDamageAnimation()
    {
        m_GameObject.GetComponent<Platformer2DUserControl>().enabled = false;
        PlayHitAnimation(true);

        isInvincible = true;

        yield return new WaitForSeconds(0.4f);

        m_GameObject.GetComponent<Platformer2DUserControl>().enabled = true;

        PlayHitAnimation(false);
        
        m_GameObject.GetComponent<Player>().isPlayerThrowingBack = false;
        
        yield return InvincibleAnimation();
        
        isInvincible = false;
    }

    protected override void KillObject()
    {
        GameMaster.Instance.InitializePlayerRespawn(true);
        base.KillObject();
    }

    #endregion

    #region private methods

    private IEnumerator InvincibleAnimation()
    {
        m_Animator.SetTrigger("InvincibleTrigger");
        m_Animator.SetBool("Invincible", true);

        //Physics2D.IgnoreLayerCollision(16, 8);

        yield return new WaitForSeconds(Invincible);

        //Physics2D.IgnoreLayerCollision(16, 8, false);

        m_Animator.SetBool("Invincible", false);
    }

    #endregion
}

[System.Serializable]
public class Enemy : Stats
{
    #region delegates

    public delegate void VoidFloatDelegate(float value);
    public event VoidFloatDelegate OnSpeedChange;

    public delegate void VoidBoolDelegate(bool value);
    public event VoidBoolDelegate OnEnemyTakeDamage;

    #endregion

    #region public fields

    public int DamageAmount = 1;
    public float Speed = 1f;
    public float AttackSpeed = 2f;
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

    public override void TakeDamage(int amount)
    {
        if (OnEnemyTakeDamage != null)
        {
            OnEnemyTakeDamage(m_IsPlayerNear);
        }

        base.TakeDamage(amount);
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

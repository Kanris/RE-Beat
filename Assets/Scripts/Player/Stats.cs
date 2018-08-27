using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[System.Serializable]
public class Stats {

    public int MaxHealth = 200;

    protected GameObject m_GameObject;
    protected Animator m_Animator;

    private int m_CurrentHealth;

    [SerializeField] private GameObject DeathParticle;
    [SerializeField] private string HitSound;
    [SerializeField] private string DeathSound;

    public delegate void DeathDelegate();
    public event DeathDelegate OnObjectDeath;

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

    protected void PlayHitAnimation(bool isHit)
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool("Damage", isHit);

            if (isHit)
                m_Animator.SetTrigger("DamageTrigger");
        }
    }

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
	
}

[System.Serializable]
public class PlayerStats : Stats
{
    public static int GemsAmount;
    public static int DamageAmount = 50;
    public static float AttackSpeed = 0.6f;
    public static float Invincible = 2f;
    public static Inventory PlayerInventory;

    private bool isInvincible;

    public override void Initialize(GameObject gameObject, Animator animator = null)
    {
        if (PlayerInventory == null)
            PlayerInventory = new Inventory(10);

        base.Initialize(gameObject);

        UIManager.Instance.Clear();
        UIManager.Instance.AddHealth(CurrentHealth);
    }

    public override void TakeDamage(int amount)
    {
        if (!isInvincible)
        {
            base.TakeDamage(amount);

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

    private IEnumerator InvincibleAnimation()
    {
        m_Animator.SetTrigger("InvincibleTrigger");
        m_Animator.SetBool("Invincible", true);

        Physics2D.IgnoreLayerCollision(16, 8);

        yield return new WaitForSeconds(Invincible);

        Physics2D.IgnoreLayerCollision(16, 8, false);

        m_Animator.SetBool("Invincible", false);
    }

    protected override void KillObject()
    {
        GameMaster.Instance.InitializePlayerRespawn(true);
        base.KillObject();
    }
}

[System.Serializable]
public class Enemy : Stats
{
    public delegate void VoidFloatDelegate(float value);
    public event VoidFloatDelegate OnSpeedChange;

    public delegate void VoidBoolDelegate(bool value);
    public event VoidBoolDelegate OnEnemyTakeDamage;

    public int DamageAmount = 1;
    public float Speed = 1f;

    public float AttackSpeed = 2f;

    private bool m_IsPlayerNear;

    public bool IsPlayerNear
    {
        get    
        {
            return m_IsPlayerNear;
        }
    }

    public override void TakeDamage(int amount)
    {
        if (OnEnemyTakeDamage != null)
        {
            OnEnemyTakeDamage(m_IsPlayerNear);
        }

        base.TakeDamage(amount);
    }

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
}

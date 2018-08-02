using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[System.Serializable]
public class Stats {

    public int MaxHealth = 200;

    protected GameObject m_GameObject;

    private int m_CurrentHealth;
    [SerializeField] private GameObject DeathParticle;
    protected Animator m_Animator;

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

    public virtual void Initialize(GameObject gameObject)
    {
        InitializeGameObject(gameObject);
        InitializeHealth();
        InitializeAnimator();
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

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth == 0)
        {
            KillObject();
        }

        if (CurrentHealth > 0)
            GameMaster.Instance.StartCoroutine(PlayTakeDamageAnimation());
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
    public static string PlayerName = "Penny";
    public static int GemsAmount;
    public static int DamageAmount = 50;
    public static float AttackSpeed = 0.1f;
    public static Inventory PlayerInventory;

    private bool isInvincible;

    public override void Initialize(GameObject gameObject)
    {
        if (PlayerInventory == null)
            PlayerInventory = new Inventory(10);

        base.Initialize(gameObject);

        UIManager.Instance.AddHealth(CurrentHealth);
    }

    public override void TakeDamage(int amount)
    {
        if (!isInvincible)
        {
            AudioManager.Instance.Play("Hit");

            base.TakeDamage(amount);

            UIManager.Instance.RemoveHealth(amount);
        }
    }

    protected override IEnumerator PlayTakeDamageAnimation()
    {
        m_GameObject.GetComponent<Platformer2DUserControl>().enabled = false;
        PlayHitAnimation(true);

        isInvincible = true;

        yield return new WaitForSeconds(0.3f);

        m_GameObject.GetComponent<Platformer2DUserControl>().enabled = true;

        PlayHitAnimation(false);
        isInvincible = false;
        
        m_GameObject.GetComponent<Player>().isPlayerThrowingBack = false;

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
    public int DamageAmount = 1;
}

[System.Serializable]
public class MageEnemyStats : Enemy
{
    public float AttackSpeed = 2f;
}

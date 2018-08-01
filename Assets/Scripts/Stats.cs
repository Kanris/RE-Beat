using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stats {

    public int MaxHealth = 200;

    protected GameObject m_GameObject;

    private int m_CurrentHealth;
    [SerializeField] private GameObject DeathParticle;

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
    public static Inventory PlayerInventory;

    private Animator m_Animator;
    private bool isInvincible;

    public override void Initialize(GameObject gameObject)
    {
        if (PlayerInventory == null)
            PlayerInventory = new Inventory(10);

        base.Initialize(gameObject);

        InitializeAnimator();

        UIManager.Instance.AddHealth(CurrentHealth);
    }

    private void InitializeAnimator()
    {
        m_Animator = m_GameObject.GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("PlayerStats.InitializeAnimator: Can't initialize animator.");
        }
    }

    public override void TakeDamage(int amount)
    {
        if (!isInvincible)
        {
            base.TakeDamage(amount);

            UIManager.Instance.RemoveHealth(amount);

            if (CurrentHealth > 0)
                GameMaster.Instance.StartCoroutine(PlayTakeDamageAnimation());
        }
    }

    private IEnumerator PlayTakeDamageAnimation()
    {
        m_Animator.SetBool("Damage", true);
        m_Animator.SetTrigger("DamageTrigger");
        isInvincible = true;

        yield return new WaitForSeconds(0.3f);

        m_Animator.SetBool("Damage", false);
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
public class MageEnemyStats : Stats
{
    public float AttackSpeed = 2f;
}

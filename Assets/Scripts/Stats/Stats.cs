using System.Collections;
using UnityEngine;

[System.Serializable]
public class Stats {

    #region public fields

    public int MaxHealth = 200;
    [Range(0f, 10f)] public float m_ThrowX = 2f;
    [Range(0f, 10f)] public float m_ThrowY = 0.5f;

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

    public virtual void TakeDamage(int amount, int divider = 1)
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
            GameMaster.Instance.StartCoroutine(PlayTakeDamageAnimation(divider));
            PlayHitSound();
        }
    }

    protected virtual IEnumerator PlayTakeDamageAnimation(int divider)
    {
        PlayHitAnimation(true);

        var m_prevThrowX = m_ThrowX;
        var m_prevThrowY = m_ThrowY;

        m_ThrowX /= divider;
        m_ThrowY /= divider;

        yield return new WaitForSeconds(0.1f);

        m_ThrowX = m_prevThrowX;
        m_ThrowY = m_prevThrowY;

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
            m_Animator.SetBool("Hit", isHit);
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

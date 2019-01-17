using System.Collections;
using UnityEngine;

[System.Serializable]
public class Stats {

    #region public fields

    #region private serialize fields

    [Header("Effects")]
    [SerializeField] public GameObject DeathParticle; //particles that will be spawn after object death
    [SerializeField] public Audio HitSound; //sound that will be played when object gets hit
    [SerializeField] public Audio DeathSound; //sound that will be played when object died

    #endregion

    [Header("General stats")]
    [Range(1, 4000)] public int MaxHealth = 200;

    [HideInInspector] public float m_ThrowBackX;
    [HideInInspector] public float m_ThrowBackY;

    #endregion

    #region protected fields

    protected GameObject m_GameObject; //gameobject that contains stat class
    protected Animator m_Animator; //animator of the gameobject

    #endregion

    #region private fields

    private int m_CurrentHealth; //current object health

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
            PlayHitSound();
            m_CurrentHealth = Mathf.Clamp(value, 0, MaxHealth); //current health value can't be higher than max health and less than zero
        }
    }

    #endregion

    #region Initialize

    public virtual void Initialize(GameObject gameObject, Animator animator = null)
    {
        m_GameObject = gameObject;

        m_CurrentHealth = MaxHealth;

        if (animator == null)
            m_Animator = m_GameObject.GetComponent<Animator>();
        else
            m_Animator = animator;
    }

    #endregion

    #region virtual methods

    public virtual void TakeDamage(int amount, float throwX, float throwY)
    {
        CurrentHealth -= amount; //change current health

        if (CurrentHealth == 0) //if object is dead
        {
            OnObjectDeath?.Invoke();

            PlayDeathSound();
            KillObject(); //destroy gameobject
        }
        else if (CurrentHealth > 0) //object still alive
        {
            //set throwback values
            m_ThrowBackX = throwX;
            m_ThrowBackY = throwY;

            Debug.LogError(m_GameObject.name + " x:" + m_ThrowBackX + " y:" + m_ThrowBackY);

            GameMaster.Instance.StartCoroutine(ObjectTakeDamage()); //play hit animation and throw back object
        }
    }

    protected virtual IEnumerator ObjectTakeDamage()
    {
        PlayHitAnimation(true); //play hit animation

        yield return new WaitForSeconds(0.1f); //thorw time

        PlayHitAnimation(false); //stop hit animation
    }

    protected virtual void KillObject()
    {
        PlayDeathParticles(); //show death particles
        GameMaster.Destroy(m_GameObject); //destroy gameobject
    }

    protected virtual void PlayDeathParticles()
    {
        if (DeathParticle != null) //if there is death particles
        {
            if (m_GameObject != null) //if object still exists
            {
                var gameObjectToDestroy =
                    GameMaster.Instantiate(DeathParticle, m_GameObject.transform.position, m_GameObject.transform.rotation);

                GameMaster.Destroy(gameObjectToDestroy, 3f);
            }
        }
    }
    #endregion

    #region protected methods

    protected void PlayHitAnimation(bool value)
    {
        if (m_Animator != null)
        {
            m_Animator.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0); //stop player movement (prepare to throw back)
            m_Animator.SetBool("Hit", value); //play hit animation
        }
    }

    #endregion

    #region private methods

    private void PlayHitSound()
    {
        if (HitSound != null)
        {
            AudioManager.Instance.Play(HitSound);
        }
    }

    private void PlayDeathSound()
    {
        if (DeathSound != null)
        {
            AudioManager.Instance.Play(DeathSound);
        }
    }

    #endregion

}

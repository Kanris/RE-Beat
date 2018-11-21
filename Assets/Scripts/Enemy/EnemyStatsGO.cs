using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class EnemyStatsGO : MonoBehaviour {

    public enum EnemyType { Regular, Drone }
    public EnemyType m_EnemyType;

    public GameObject m_GameObjectToDestroy;
    public Enemy EnemyStats;

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnDroneDestroy;

    [Header("Drone stats")]
    [SerializeField, Range(0f, 10f)] private float DeathDetonationTimer = 2f; //time before destroying drone
    [SerializeField] private bool m_DestroyOnCollision = false;
    [SerializeField] private LayerMask m_LayerMask;

    [Header("Effects")]
    [SerializeField] private GameObject GroundHitParticles;

    private Rigidbody2D m_Rigidbody;
    private Animator m_Animator;
    private float m_DestroyTimer;
    [HideInInspector] public bool m_IsDestroying = false; //is drone going to blow up

    // Use this for initialization
    void Start () {

        InitializeStats();

        InitializeComponents();
    }

    public void InitializeStats()
    {
        EnemyStats.Initialize(m_GameObjectToDestroy, GetComponent<Animator>());
    }

    private void Update()
    {
        if (m_IsDestroying)
        {
            if (m_DestroyTimer < Time.time)
            {
                m_IsDestroying = false;
                StartCoroutine(DestroyDrone());
            }
        }
    }

    public void TakeDamage(PlayerStats playerStats, int zone, int damageAmount = 0)
    {
        if (m_EnemyType == EnemyType.Regular)
        {
            if (playerStats != null)
                playerStats.HitEnemy(EnemyStats, zone);
            else
                EnemyStats.TakeDamage(damageAmount, zone);
        }
        else
        {
            if (!m_IsDestroying)
            {
                EnemyStats.CurrentHealth--;

                if (EnemyStats.CurrentHealth < 1)
                {
                    DetroySequence();
                }
                else
                    m_Animator.SetTrigger("Hit");
            }
        }

    }

    private void InitializeComponents()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_EnemyType == EnemyType.Drone)
            OnDroneCollision(collision);
        else
            OnRegularCollision(collision);
    }

    private void OnRegularCollision(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            EnemyStats.HitPlayer(collision.transform.GetComponent<Player>().playerStats);
        }
    }

    private void OnDroneCollision(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsDestroying)
        {
            EnemyStats.HitPlayer(collision.transform.GetComponent<Player>().playerStats);

            if (m_DestroyOnCollision)
            {
                m_IsDestroying = true;
                StartCoroutine(DestroyDrone());
            }
        }

        if (collision.gameObject.layer == 14 & m_IsDestroying) //object layer - ground
        {
            StartCoroutine(DestroyDrone(DeathDetonationTimer));
        }
        else if (collision.gameObject.layer == 14)
        {
            Destroy(
                Instantiate(GroundHitParticles, collision.contacts[0].point, Quaternion.identity),
                1f);
        }
    }

    #region drone's methods
    private void DetroySequence()
    {
        m_Animator.SetTrigger("Destroy");

        m_IsDestroying = true;
        m_Rigidbody.sharedMaterial = null;
        m_Rigidbody.gravityScale = 3f;

        Destroy(GetComponent<TrailRenderer>());

        m_DestroyTimer = 3f + Time.time;

        OnDroneDestroy(true);
    }

    private IEnumerator DestroyDrone(float waitTimeBeforeDestroy = 0f)
    {
        yield return new WaitForSeconds(waitTimeBeforeDestroy);

        var destroyParticles = Instantiate(EnemyStats.DeathParticle, transform.position, Quaternion.identity);
        Destroy(destroyParticles, 1f);

        var hit2D = Physics2D.OverlapCircle(transform.position, 2, m_LayerMask); // player in range

        if (hit2D != null)
        {
            var playerStats = hit2D.GetComponent<Player>().playerStats;

            playerStats.TakeDamage(EnemyStats.DamageAmount);
            playerStats.DebuffPlayer(DebuffPanel.DebuffTypes.Defense, 5f);
        }

        PlayerStats.Scrap = EnemyStats.DropScrap;

        Destroy(
            Instantiate(EnemyStats.DeathParticle, transform.position, Quaternion.identity), 2f);

        AudioManager.Instance.Play(EnemyStats.DeathSound);

        Destroy(m_GameObjectToDestroy);
    }
    #endregion

    #region test methods

    [ContextMenu("CreateShieldOnEnemy")]
    public void CreateShield()
    {
        EnemyStats.CreateShield();
    }

    #endregion
}

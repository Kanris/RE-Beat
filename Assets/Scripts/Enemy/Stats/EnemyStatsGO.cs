using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
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
    [SerializeField] private GameObject m_HitParticles;
    [SerializeField] private GameObject m_Scraps;

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

    private void InitializeComponents()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
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

            if (EnemyStats.CurrentHealth > 0)
                CreateHitParticles();
            else
                CreateScraps();
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
                {
                    CreateHitParticles();
                    m_Animator.SetTrigger("Hit");
                }
            }
        }
    }

    private void CreateScraps()
    {
        if (m_Scraps != null)
        {
            var target = GameObject.FindGameObjectWithTag("Player").transform;

            GameMaster.Instantiate(m_Scraps, transform.position, Quaternion.identity)
                .GetComponent<ScrapObject>().SetTarget(target, EnemyStats.DropScrap);
        }
    }

    private void CreateHitParticles()
    {
        if (m_HitParticles != null)
        {
            var hitParticles = Instantiate(m_HitParticles);
            hitParticles.transform.position = transform.position;

            Destroy(hitParticles, 2f);
        }
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
                m_DestroyTimer = 3f + Time.time;
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
        m_Rigidbody.mass = 1f;
        m_Rigidbody.gravityScale = 3f;

        Destroy(GetComponent<TrailRenderer>());

        m_DestroyTimer = 3f + Time.time;

        OnDroneDestroy(true);
    }

    private IEnumerator DestroyDrone(float waitTimeBeforeDestroy = 0f)
    {
        yield return new WaitForSeconds(waitTimeBeforeDestroy);

        var hit2D = Physics2D.OverlapCircle(transform.position, 2, m_LayerMask); // player in range

        if (hit2D != null)
        {
            //set hit direction
            var fromWhereHit = hit2D.transform.position - transform.position;
            fromWhereHit.Normalize();
            hit2D.GetComponent<Player>().m_EnemyHitDirection = fromWhereHit.x > 0f ? 1 : -1;

            //player takes damage
            var playerStats = hit2D.GetComponent<Player>().playerStats;

            playerStats.TakeDamage(EnemyStats.DamageAmount);
            playerStats.DebuffPlayer(DebuffPanel.DebuffTypes.Defense, 5f);
        }

        if (EnemyStats.DeathParticle != null)
        {
            Destroy(
                Instantiate(EnemyStats.DeathParticle, transform.position, Quaternion.identity), 2f);
            EnemyStats.DeathParticle = null;
        }

        CreateScraps();

        EnemyStats.TakeDamage(1);
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

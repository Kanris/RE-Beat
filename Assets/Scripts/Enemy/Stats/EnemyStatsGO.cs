using UnityEngine;
using System.Collections;
using UnityStandardAssets._2D;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class EnemyStatsGO : MonoBehaviour {

    public enum EnemyType { Regular, Drone }
    public EnemyType m_EnemyType;

    public GameObject m_GameObjectToDestroy;
    public Enemy EnemyStats;

    public delegate void VoidDelegateBool(bool value);
    public event VoidDelegateBool OnDroneDestroy;

    [Header("Drone stats")]
    [SerializeField, Range(0f, 10f)] private float DeathDetonationTimer = 2f; //time before destroying drone
    [SerializeField] private bool m_DestroyOnCollision = false;
    [SerializeField] private LayerMask m_LayerMask;

    [Header("UI")]
    [SerializeField] private GameObject m_HealthUI;
    [SerializeField] private Image m_CurrentHealthImage;

    [Header("Effects")]
    [SerializeField] private GameObject GroundHitParticles;
    [SerializeField] private GameObject m_HitParticles;
    [SerializeField] private GameObject m_Scraps;


    private Rigidbody2D m_Rigidbody;
    private Animator m_Animator;
    private float m_DestroyTimer;
    [HideInInspector] public bool m_IsDestroying = false; //is drone going to blow up

    // Use this for initialization
    void Start() {

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

    public void ChangeUIScale(float value)
    {
        m_HealthUI.transform.localScale = new Vector3(value, 1, 1);
    }

    private void DisplayHealthChange(float damageAmount)
    {
        if (PlayerStats.m_IsCanSeeEnemyHP && !m_HealthUI.activeSelf)
            m_HealthUI.SetActive(true);

        m_CurrentHealthImage.fillAmount = (float)EnemyStats.CurrentHealth / (float)EnemyStats.MaxHealth;
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
        if(GetComponent<EnemyStatsGO>().enabled)
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
                    if (EnemyStats.CurrentHealth == 1)
                    {
                        DetroySequence();
                    }
                    else
                    {
                        CreateHitParticles();
                        EnemyStats.TakeDamage(1, zone);
                    }
                }
            }

            if (m_CurrentHealthImage != null) DisplayHealthChange(damageAmount);
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
            if (PlayerStats.m_IsInvincibleWhileDashing && collision.gameObject.GetComponent<Animator>().GetBool("Dash"))
            {
                Camera.main.GetComponent<Camera2DFollow>().PlayHitEffect();
            }
            else
            {
                EnemyStats.HitPlayer(collision.transform.GetComponent<Player>().playerStats, EnemyStats.DamageAmount);
            }


            if (PlayerStats.m_IsDamageEnemyWhileDashing && collision.gameObject.GetComponent<Animator>().GetBool("Dash"))
            {
                Debug.LogError("Enemy take damage");
                EnemyStats.TakeDamage(PlayerStats.DamageAmount / 3);
            }
        }
    }

    private void OnDroneCollision(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsDestroying)
        {
            var damageAmount = 1;

            if (PlayerStats.m_IsInvincibleWhileDashing && collision.gameObject.GetComponent<Animator>().GetBool("Dash"))
            {
                damageAmount = 0;
            }

            EnemyStats.HitPlayer(collision.transform.GetComponent<Player>().playerStats, damageAmount);

            if (PlayerStats.m_IsDamageEnemyWhileDashing && collision.gameObject.GetComponent<Animator>().GetBool("Dash"))
            {
                EnemyStats.TakeDamage(damageAmount);
            }

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
        m_Rigidbody.gravityScale = 3f;
        m_Rigidbody.constraints = RigidbodyConstraints2D.None;

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

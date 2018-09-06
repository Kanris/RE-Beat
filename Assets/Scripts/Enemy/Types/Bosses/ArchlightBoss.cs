using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(EnemyStatsGO))]
public class ArchlightBoss : MonoBehaviour
{

    #region SerializeField
    [SerializeField] private GameObject LeftTeleport;
    [SerializeField] private GameObject CenterTeleport;
    [SerializeField] private GameObject RightTeleport;
    
    [SerializeField] private GameObject FireballGO;

    [SerializeField] private GameObject FlyingPlatform;
    [SerializeField] private GameObject Key;

    [SerializeField] private float TeleportSpeed = 5f;
    [SerializeField] private int Stage2Health = 500;
    [SerializeField] private int Stage3Health = 200;

    #endregion

    #region private

    private Dictionary<Vector3, int> teleportDestinations;
    private Animator m_Animator;
    private int m_NextTeleportIndex = 10;
    private Enemy m_Stats;
    private bool m_Stage2;
    private bool m_Stage3;
    private bool m_IsReset;
    private float m_TeleportTimer;
    private Vector3 m_NextDestination;
    #endregion

    // Use this for initialization
    void Start()
    {
        InitializeTeleportsTransform();

        InitializeAnimator();

        InitializeStats();

        SubscribeEvents();

        m_NextDestination = GetDestination();

        m_TeleportTimer = 0f;
    }

    #region Initialize

    private void InitializeTeleportsTransform()
    {
        teleportDestinations = new Dictionary<Vector3, int>();

        InitializeTransform(LeftTeleport, 1);
        InitializeTransform(RightTeleport, -1);
        InitializeTransform(CenterTeleport, 0);
    }

    private void InitializeTransform(GameObject teleport, int platform)
    {
        if (teleport != null)
        {
            for (int index = 0; index < teleport.transform.childCount; index++)
            {
                teleportDestinations.Add(teleport.transform.GetChild(index).position, platform);
            }
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void InitializeStats()
    {
        m_Stats = GetComponent<EnemyStatsGO>().EnemyStats;
    }

    private void SubscribeEvents()
    {
        m_Stats.OnObjectDeath += ArchlightDead;
        m_Stats.OnEnemyTakeDamage += OnPlayerHitTeleport;
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        ChangeState();

        if (m_TeleportTimer <= Time.time)
        {
            m_TeleportTimer = Time.time + TeleportSpeed;

            CrossAttack();

            StartCoroutine(TeleportSequence(m_NextDestination));

            m_NextDestination = GetDestination();
        }

        if (GameMaster.Instance.isPlayerDead)
        {
            if (!m_IsReset)
                StartCoroutine(ResetState());
        }
        else if (m_IsReset)
            m_IsReset = false;
    }

    #region State

    private void ChangeState()
    {
        if (m_Stats.CurrentHealth <= Stage2Health & !m_Stage2)
            m_Stage2 = true;

        if (m_Stats.CurrentHealth <= Stage3Health & !m_Stage3)
        {
            m_Stage3 = true;

            ChangeLightState(false);
            TeleportSpeed = 2f;
        }
    }

    private IEnumerator ResetState()
    {
        m_IsReset = true;

        GetComponent<EnemyStatsGO>().InitializeStats();

        m_Stage2 = false;
        m_Stage3 = false;

        TeleportSpeed = 5f;

        yield return new WaitForSeconds(0.5f);

        ResetArchlightPosition();

        ChangeLightState(true);
        gameObject.SetActive(false);
    }

    private void ResetArchlightPosition()
    {
        if (CenterTeleport != null)
        {
            if (CenterTeleport.transform.childCount > 0)
            {
                transform.position = CenterTeleport.transform.GetChild(0).position;
            }
        }
    }

    private void ChangeLightState(bool value)
    {
        LeftTeleport.SetActive(value);
        RightTeleport.SetActive(value);
        CenterTeleport.SetActive(value);
    }

    #endregion

    private void ArchlightDead()
    {
        ChangeLightState(true);
        FlyingPlatform.SetActive(true);
        Key.SetActive(true);

        ShowParticles("Effects/Archlight/Archlight Death Particle", 10f);

        GameMaster.Instance.SaveState("BossTrigger", 0, GameMaster.RecreateType.Object);
        GameMaster.Instance.SaveState("Junk", 0, GameMaster.RecreateType.Object, "E1M1");
    }

    private void ShowParticles(string path, float time = 1.5f)
    {
        var particle = Resources.Load(path) as GameObject;

        if (particle != null)
        {
            var DeathParticleInstantiate =
                Instantiate(particle, transform.position, transform.rotation);
            Destroy(DeathParticleInstantiate, time);
        }
    }

    #region Teleport

    private void OnPlayerHitTeleport(bool value)
    {
        m_TeleportTimer = Time.time;
        ShowParticles("Effects/Archlight/Archlight Hit Particle", 5f);
    }

    private IEnumerator TeleportSequence(Vector3 destination)
    {
        TeleportAnimation(true);

        TeleportSound();

        yield return new WaitForSeconds(0.5f);

        transform.position = destination;
        ChangeLookPosition();

        TeleportAnimation(false);

        if (m_Stage2) WeirdAttack();

    }

    private void TeleportSound()
    {
        AudioManager.Instance.Play("Teleport");
    }

    private Vector3 GetDestination()
    {
        var randomDestination = GetRandomIndex();

        var index = 0;
        Vector3 teleportDestination = Vector3.zero;

        foreach (var item in teleportDestinations)
        {
            if (index == randomDestination)
            {
                teleportDestination = new Vector3(item.Key.x, item.Key.y - 1f, item.Key.z);
                break;
            }

            index++;
        }

        return teleportDestination;
    }

    private int GetRandomIndex()
    {
        int randIndex;

        do
        {
            randIndex = Random.Range(0, teleportDestinations.Count);
        }
        while (randIndex == m_NextTeleportIndex);

        m_NextTeleportIndex = randIndex;

        return randIndex;
    }

    private void TeleportAnimation(bool isTeleport)
    {
        m_Animator.SetBool("Teleport", isTeleport);
    }

    private void ChangeLookPosition()
    {
        float lookPosition = teleportDestinations[new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z)];
        m_Animator.SetFloat("LookPosition", lookPosition);
    }

    #endregion

    #region Attack

    private void CrossAttack()
    {
        PlayAttackSound();

        CreateFireball(Vector3.up);

        CreateFireball(Vector3.down);

        CreateFireball(Vector3.right);

        CreateFireball(Vector3.left);
    }

    private void WeirdAttack()
    {
        PlayAttackSound();

        CreateFireball(new Vector3(1, 1, 0));

        CreateFireball(new Vector3(-1, 1, 0));

        CreateFireball(new Vector3(-1, -1, 0));

        CreateFireball(new Vector3(1, -1, 0));
    }

    private void CreateFireball(Vector2 direction)
    {
        var fireball = Instantiate(FireballGO, transform.position, transform.rotation);
        fireball.GetComponent<Fireball>().Direction = direction;
    }

    private void PlayAttackSound()
    {
        AudioManager.Instance.Play("Archlight-attack");
    }

    #endregion

    #region collision

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(m_Stats.DamageAmount);
        }
    }

    #endregion
}

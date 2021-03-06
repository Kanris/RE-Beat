﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(EnemyStatsGO))]
public class DroneHive : MonoBehaviour
{

    #region SerializeField
    [SerializeField] private Image m_Health;

    [Header("Teleport")]
    [SerializeField] private GameObject m_ThrowBackAbility;

    [Header("Teleport Points")]
    [SerializeField] private GameObject LeftTeleport;
    [SerializeField] private GameObject CenterTeleport;
    [SerializeField] private GameObject RightTeleport;
    
    [Header("Stats")]
    [SerializeField] private GameObject FireballGO;
    [SerializeField] private float TeleportSpeed = 5f;

    [Header("Items activate after drone death")]
    [SerializeField] private GameObject Key;

    [Header("Boss stage hp")]
    [SerializeField] private int Stage2Health = 500;
    [SerializeField] private int Stage3Health = 200;

    [Header("Effects")]
    [SerializeField] private Audio TeleportAudio;
    [SerializeField] private Audio AttackAudio;
    [SerializeField] private GameObject DeathParticle;
    [SerializeField] private GameObject HitParticle;

    [Header("Trigger")]
    [SerializeField] private ObjectAppearOnTrigger m_BossTrigger;
    [SerializeField] private GameObject m_BlockDoor;

    [Header("Audio")]
    [SerializeField] private Audio m_BossBattleAudio;
    [SerializeField] private Audio m_SceneBackgroundMusic;

    [Header("Spawn")]
    [SerializeField] private Transform m_SpawnLocation;
    [SerializeField] private GameObject m_KamikazeDrone;
    [SerializeField] private GameObject m_ChaserDrone;

    #endregion

    #region private

    private Dictionary<Vector3, int> teleportDestinations;
    private Animator m_Animator;
    private int m_NextTeleportIndex = 10;
    private Enemy m_Stats;
    private bool m_Stage2;
    private bool m_Stage3;
    private bool m_IsTeleporting;
    private float m_TeleportTimer;
    private Vector3 m_NextDestination;

    private GameObject m_EnemyToSpawn;
    private int m_SpawnDroneCount;
    private bool m_IsCanSpawn;

    private Vector2 m_StartPosition;
    #endregion

    // Use this for initialization
    void Start()
    {
        m_StartPosition = transform.position;

        InitializeTeleportsTransform();

        InitializeAnimator();

        InitializeStats();

        SubscribeEvents();

        m_NextDestination = GetDestination();

        m_TeleportTimer = 0f;

        m_EnemyToSpawn = m_KamikazeDrone;

        m_SpawnDroneCount = 3;
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
        m_Stats.OnEnemyTakeDamageValue += ChangeUIHealth;
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        ChangeState();

        if (GameMaster.Instance.IsPlayerDead)
        {
            GameMaster.Instance.StartCoroutine(ResetState());
        }

        if (m_TeleportTimer <= Time.time)
        {
            m_TeleportTimer = Time.time + TeleportSpeed;

            StartCoroutine(TeleportSequence(m_NextDestination));

            m_NextDestination = GetDestination();
        }

        if (Key.activeSelf)
        {
            Key.SetActive(false);
            AudioManager.Instance.SetBackgroundMusic(m_BossBattleAudio);
        }
    }

    #region State

    private void ChangeState()
    {
        if (m_Stats.CurrentHealth <= Stage2Health & !m_Stage2)
        {
            m_Stage2 = true;

            for (var index = 0; index < m_SpawnLocation.childCount; index++)
            {
                m_SpawnLocation.GetChild(index).GetComponent<EnemyStatsGO>().TakeDamage(null, 0, 1);
            }

            m_EnemyToSpawn = m_ChaserDrone;
            m_SpawnDroneCount = 1;
        }

        if (m_Stats.CurrentHealth <= Stage3Health & !m_Stage3)
        {
            m_Stage3 = true;

            ChangeLightState(false);
            TeleportSpeed = 2f;
        }
    }

    private IEnumerator ResetState()
    {
        GetComponent<EnemyStatsGO>().InitializeStats();
        
        m_Stage2 = false;
        m_Stage3 = false;

        TeleportSpeed = 5f;

        yield return new WaitForSeconds(1f);

        ResetArchlightPosition();
        m_BlockDoor.SetActive(false);
        m_Health.fillAmount = 1f;
        
        ChangeLightState(true);
        Key.SetActive(true);

        AudioManager.Instance.SetBackgroundMusic(m_SceneBackgroundMusic);

        transform.position = m_StartPosition;

        for (var index = 0; index < m_SpawnLocation.childCount; index++)
        {
            Destroy(m_SpawnLocation.GetChild(index).gameObject);
        }

        m_EnemyToSpawn = m_KamikazeDrone;

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

        ShowParticles(DeathParticle, 10f);

        GameMaster.Instance.SaveState("BossTrigger", 0, GameMaster.RecreateType.Object);
        GameMaster.Instance.SaveState("Junk", 0, GameMaster.RecreateType.Object, "Forest");
        GameMaster.Instance.SaveState(m_BlockDoor.name, 0, GameMaster.RecreateType.Object);

        Destroy(m_BlockDoor);
        Destroy(m_BossTrigger.gameObject);
    }

    private void ShowParticles(GameObject particle, float time = 1.5f)
    {
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
        if (!m_IsTeleporting)
        {
            GetComponent<EnemyStatsGO>().enabled = false;

            WeirdAttack();

            m_TeleportTimer = Time.time;

            m_IsCanSpawn = false;

            ShowParticles(HitParticle, 5f);
        }
    }

    private void ChangeUIHealth(float value)
    {
        var fillValue = 100 * value / m_Stats.MaxHealth / 100;

        m_Health.fillAmount -= fillValue;
    }

    private IEnumerator TeleportSequence(Vector3 destination)
    {

        CreateThrowback();

        yield return new WaitForSeconds(.1f);

        TeleportAnimation(true);

        TeleportSound();

        WeirdAttack();

        var spawnPosition = transform.position;

        GetComponent<EnemyStatsGO>().enabled = false;

        yield return new WaitForSeconds(0.5f);

        GetComponent<EnemyStatsGO>().enabled = true;

        transform.position = destination;
        ChangeLookPosition();

        TeleportAnimation(false);

        if (!m_Stage3 & m_SpawnLocation.childCount < m_SpawnDroneCount & m_IsCanSpawn)
        {
            var drone = Instantiate(m_EnemyToSpawn, m_SpawnLocation);
            drone.transform.position = spawnPosition;
        }

        m_IsCanSpawn = true;
    }

    private void CreateThrowback()
    {
        //throw back player
        Physics2D.OverlapCircle(transform.position, 4f, 1 << LayerMask.NameToLayer("Player"))?.gameObject.GetComponent<Player>().playerStats.HitPlayer(0);
        //destroy throwback effect
        Destroy(Instantiate(m_ThrowBackAbility, transform), .6f);
    }

    private void TeleportSound()
    {
        AudioManager.Instance.Play(TeleportAudio);
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
        m_IsTeleporting = isTeleport;
        m_Animator.SetBool("Teleport", isTeleport);
    }

    private void ChangeLookPosition()
    {
        float lookPosition = teleportDestinations[new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z)];
        m_Animator.SetFloat("LookPosition", lookPosition);
    }

    #endregion

    #region Attack

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
        AudioManager.Instance.Play(AttackAudio);
    }

    #endregion

    #region collision

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().playerStats.HitPlayer(m_Stats.DamageAmount);
        }
    }

    #endregion
}

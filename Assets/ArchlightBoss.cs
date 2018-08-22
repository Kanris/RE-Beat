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

    [SerializeField] private GameObject FlyingPlatform;
    [SerializeField] private GameObject Key;

    [SerializeField] private float TeleportSpeed = 5f;

    #endregion

    #region private

    private Dictionary<Vector3, int> teleportDestinations;
    private Animator m_Animator;
    private bool m_IsTeleport;
    private int m_CurrentTeleportIndex = 10;
    private Enemy m_Stats;
    private bool m_Stage2;
    private bool m_Stage3;
    private bool m_IsReset;

    #endregion

    // Use this for initialization
    void Start()
    {

        InitializeTeleportsTransform();

        InitializeAnimator();

        InitializeStats();

        SubscribeEvents();

        StartCoroutine(TeleportSequence());
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
        m_Stats.OnEnemyTakeDamage += BossTeleport;
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        if (m_Stats.CurrentHealth < 500 & !m_Stage2)
            m_Stage2 = true;

        if (m_Stats.CurrentHealth < 200 & !m_Stage3)
        {
            ChangeLightState(false);
            m_Stage3 = true;
            TeleportSpeed = 2f;
        }

        if (GameMaster.Instance.isPlayerDead)
        {
            if (!m_IsReset)
                StartCoroutine(ResetArchlight());
        }
        else if(m_IsReset)
        {
            m_IsReset = false;
            StartCoroutine(TeleportSequence());
        }
    }

    public IEnumerator ResetArchlight()
    {
        GetComponent<EnemyStatsGO>().InitializeStats();
        m_Stage2 = false;
        m_Stage3 = false;
        m_IsTeleport = false;
        m_IsReset = true;
        TeleportSpeed = 5f;
        yield return new WaitForSeconds(0.5f);

        ChangeLightState(true);
        gameObject.SetActive(false);
    }

    private void ChangeLightState(bool value)
    {
        LeftTeleport.SetActive(value);
        RightTeleport.SetActive(value);
        CenterTeleport.SetActive(value);
    }

    private void ArchlightDead()
    {
        ChangeLightState(true);
        FlyingPlatform.SetActive(true);
        Key.SetActive(true);

        GameMaster.Instance.SaveState("BossTrigger", 0, GameMaster.RecreateType.Object);
    }

    #region Teleport

    private void BossTeleport(bool value)
    {

        TeleportSound();

        CrossAttack();

        if (m_Stage2) WeirdAttack();

        TeleportAnimation(true);

        transform.position = GetDestination();

        TeleportAnimation(false);
    }

    private IEnumerator TeleportSequence()
    {
        m_IsTeleport = true;
        TeleportAnimation(m_IsTeleport);

        TeleportSound();

        yield return new WaitForSeconds(0.5f);

        transform.position = GetDestination();

        m_IsTeleport = false;
        TeleportAnimation(m_IsTeleport);

        if (m_Stage2) WeirdAttack();

        yield return new WaitForSeconds(TeleportSpeed);

        CrossAttack();

        StartCoroutine(TeleportSequence());
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
                ChangeLookPosition(item.Value);
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
        while (m_CurrentTeleportIndex == randIndex);

        m_CurrentTeleportIndex = randIndex;

        return randIndex;
    }

    private void TeleportAnimation(bool isTeleport)
    {
        m_Animator.SetBool("Teleport", isTeleport);
    }

    private void ChangeLookPosition(float lookPosition)
    {
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
        var fireballGO = Resources.Load("GreenFireball");
        var fireball = Instantiate(fireballGO, transform.position, transform.rotation) as GameObject;
        fireball.GetComponent<Fireball>().Direction = direction;
    }

    private void PlayAttackSound()
    {
        AudioManager.Instance.Play("Archlight-attack");
    }

    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(m_Stats.DamageAmount);
        }
    }
}

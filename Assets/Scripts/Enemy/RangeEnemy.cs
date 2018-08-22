using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyStatsGO))]
public class RangeEnemy : MonoBehaviour {

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnPlayerSpot;

    private Enemy m_EnemyStats;
    private EnemyMovement m_EnemyMovement; private Animator m_Animator;
    private TextMeshProUGUI m_Text;
    private bool m_CanCreateNewFireball = true;

    [SerializeField] private SpriteRenderer m_AlarmImage;

    // Use this for initialization
    void Start()
    {
        InitializeStats();

        InitializeEnemyMovement();

        InitializeAnimator();

        m_AlarmImage.gameObject.SetActive(false);
    }

    private void InitializeStats()
    {
        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats;
    }

    private void InitializeEnemyMovement()
    {
        m_EnemyMovement = GetComponent<EnemyMovement>();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
            Debug.LogError("RangeEnemy.InitializeAnimator: Can't find animator on GameObject");
    }

    //simplify
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!m_EnemyStats.IsPlayerNear)
            {
                m_EnemyMovement.TurnAround();
            }

            collision.transform.GetComponent<Player>().playerStats.TakeDamage(m_EnemyStats.DamageAmount);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_EnemyStats.IsPlayerNear)
        {
            m_EnemyStats.ChangeIsPlayerNear(true);
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_EnemyStats.IsPlayerNear)
        {
            m_EnemyStats.ChangeIsPlayerNear(false);

            yield return ResetState();
        }
    }

    private void Update()
    {
        if (GameMaster.Instance.isPlayerDead)
        {
            m_EnemyStats.ChangeIsPlayerNear(false);
            ChangeAlertStatus(false);
        }

        if (m_EnemyStats.IsPlayerNear)
        {
            ChangeAlertStatus(true);

            if (m_CanCreateNewFireball)
            {
                StartCoroutine(StartCast());
            }
        }
    }

    private void ChangeAlertStatus(bool value)
    {
        OnPlayerSpot(value);
        EnableWarningSign(value);
    }

    private IEnumerator StartCast()
    {
        if(m_EnemyStats.IsPlayerNear)
        {
            if (m_CanCreateNewFireball)
            {
                AudioManager.Instance.Play("Cast");

                Animate(true);

                m_CanCreateNewFireball = false;

                yield return new WaitForSeconds(0.6f);

                Animate(false);

                CreateFireball();

                yield return CastCooldown();
            }
        }
    }

    private void CreateFireball()
    {
        var newFireball = Resources.Load("Fireball");

        var instantiateFireball = Instantiate(newFireball, transform.position, transform.rotation) as GameObject;

        if (m_EnemyMovement.m_PosX < 0)
        {
            instantiateFireball.GetComponent<Fireball>().Direction = Vector3.left;
        }
        else
            instantiateFireball.GetComponent<Fireball>().Direction = Vector3.right;
    }

    private IEnumerator CastCooldown()
    {
        yield return new WaitForSeconds(m_EnemyStats.AttackSpeed);

        m_CanCreateNewFireball = true;
    }


    private IEnumerator ResetState()
    {
        yield return new WaitForSeconds(1f);

        if (!m_EnemyStats.IsPlayerNear)
        {
            ChangeAlertStatus(false);
        }
    }

    private void Animate(bool isAttacking)
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool("isAttacking", isAttacking);
        }
    }

    private void EnableWarningSign(bool isAttacking)
    {
        if (m_AlarmImage != null)
        {
            m_AlarmImage.gameObject.SetActive(isAttacking);
        }
    }

}

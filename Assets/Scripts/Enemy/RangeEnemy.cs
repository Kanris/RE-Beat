using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RangeEnemy : MonoBehaviour {
    
    public MageEnemyStats EnemyStats;

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
        EnemyStats.Initialize(gameObject, GetComponent<Animator>());
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
            if (!EnemyStats.IsPlayerNear)
            {
                m_EnemyMovement.TurnAround();
            }

            collision.transform.GetComponent<Player>().playerStats.TakeDamage(EnemyStats.DamageAmount);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !EnemyStats.IsPlayerNear)
        {
            EnemyStats.IsPlayerNear = true;
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & EnemyStats.IsPlayerNear)
        {
            EnemyStats.IsPlayerNear = false;

            yield return ResetState();
        }
    }

    private void Update()
    {
        if (GameMaster.Instance.isPlayerDead)
        {
            EnemyStats.IsPlayerNear = false;
            m_EnemyMovement.isWaiting = false;
            EnableWarningSign(false);
        }

        if (EnemyStats.IsPlayerNear)
        {
            m_EnemyMovement.isWaiting = true;
            EnableWarningSign(m_EnemyMovement.isWaiting);

            if (m_CanCreateNewFireball)
            {
                StartCoroutine(StartCast());
            }
        }
    }

    private IEnumerator StartCast()
    {
        if(EnemyStats.IsPlayerNear)
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
            instantiateFireball.GetComponent<Fireball>().Direction = -Vector3.right;
        }
    }

    private IEnumerator CastCooldown()
    {
        yield return new WaitForSeconds(EnemyStats.AttackSpeed);

        m_CanCreateNewFireball = true;
    }


    private IEnumerator ResetState()
    {
        yield return new WaitForSeconds(1f);

        if (!EnemyStats.IsPlayerNear)
        {
            m_EnemyMovement.isWaiting = false;
            EnableWarningSign(false);
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

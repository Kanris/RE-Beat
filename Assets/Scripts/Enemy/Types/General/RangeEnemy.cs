using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(EnemyStatsGO))]
public class RangeEnemy : MonoBehaviour {

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnPlayerSpot;

    private Enemy m_EnemyStats;
    private EnemyMovement m_EnemyMovement; private Animator m_Animator;
    private TextMeshProUGUI m_Text;
    private bool m_CanCreateNewFireball = true;

    [SerializeField] private SpriteRenderer m_AlarmImage;
    [SerializeField] private GameObject[] ThrowObjects;


    #region Initialize
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
    #endregion

    #region Collision

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_EnemyStats.HitPlayer(collision.transform.GetComponent<Player>().playerStats);
        }
    }

    #endregion

    #region Trigger

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

    #endregion

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
        if (OnPlayerSpot != null)
            OnPlayerSpot(value); //stop moving 

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
        var throwObject = ThrowObjects[GetRandomIndex()];

        var instantiateFireball = Instantiate(throwObject, transform.position, transform.rotation) as GameObject;

        if (-transform.localScale.x < 0)
        {
            instantiateFireball.GetComponent<Fireball>().Direction = Vector3.left;
        }
        else
            instantiateFireball.GetComponent<Fireball>().Direction = Vector3.right;
    }

    private int GetRandomIndex()
    {
        return Random.Range(0, ThrowObjects.Length);
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

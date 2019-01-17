using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(EnemyStatsGO), typeof(Animator))]
public class RangeEnemy : MonoBehaviour {

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnPlayerSpot;

    private Enemy m_EnemyStats;
    private Animator m_Animator;
    private TextMeshProUGUI m_Text;
    private float m_NextFireballTime;

    [SerializeField] private SpriteRenderer m_AlarmImage;
    [SerializeField] private Transform m_FirePoint;

    [Header("Spells that mage knows")]
    [SerializeField] private GameObject[] ThrowObjects;


    #region Initialize
    // Use this for initialization
    void Start()
    {
        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats;

        m_Animator = GetComponent<Animator>();

        m_AlarmImage.gameObject.SetActive(false);
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();

            m_EnemyStats.ChangeIsPlayerNear(true);

            ChangeAlertStatus(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_EnemyStats.IsPlayerNear)
        {
            StartCoroutine( ResetState() );
        }
    }

    private void Update()
    {
        if (GameMaster.Instance.IsPlayerDead && GameMaster.Instance.m_Player == null)
        {
            if (m_EnemyStats.IsPlayerNear)
            {
                m_EnemyStats.ChangeIsPlayerNear(false);
                ChangeAlertStatus(false);
            }
        }

        if (m_EnemyStats.IsPlayerNear)
        {
            if (m_NextFireballTime < Time.time)
            {
                m_NextFireballTime = Time.time + m_EnemyStats.AttackSpeed;
                CastFireball();
            }

        }
    }

    private void ChangeAlertStatus(bool value)
    {
        OnPlayerSpot?.Invoke(value);

        m_AlarmImage.gameObject.SetActive(value);
    }

    private void CastFireball()
    {
        AudioManager.Instance.Play("Cast");

        Animate(true);
    }

    private void CreateFireball()
    {
        Animate(false);

        var throwObject = ThrowObjects[Random.Range(0, ThrowObjects.Length)];

        var instantiateFireball = Instantiate(throwObject, m_FirePoint.position, Quaternion.identity) as GameObject;

        if (-transform.localScale.x < 0)
        {
            instantiateFireball.GetComponent<Fireball>().Direction = Vector3.left;
        }
        else
            instantiateFireball.GetComponent<Fireball>().Direction = Vector3.right;
    }


    private IEnumerator ResetState()
    {
        yield return new WaitForSeconds(3f);

        m_EnemyStats.ChangeIsPlayerNear(false);

        ChangeAlertStatus(false);
    }

    private void Animate(bool isAttacking)
    {
        if (m_Animator != null)
        {
            m_Animator.SetBool("isAttacking", isAttacking);
        }
    }

}

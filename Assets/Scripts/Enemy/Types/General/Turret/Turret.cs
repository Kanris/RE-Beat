using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Turret : MonoBehaviour {

    [Header("Triggers")]
    [SerializeField] private PlayerInTrigger m_PlayerIsNear;
    [SerializeField] private PlayerInTrigger m_PlayerInAttackRange;

    [Header("Bullet")]
    [SerializeField] private GameObject m_ExplosionPrefab;
    [SerializeField] private Transform m_FirePoint;
    [SerializeField] private Audio m_ShootAudio;

    private Animator m_Animator;
    private Enemy m_EnemyStats;

    private Transform m_PlayerTransform;
    private float m_NextAttackTime;

    private bool m_IsPlayerNearTurret = false;
    
	// Use this for initialization
	void Start () {

        m_Animator = GetComponent<Animator>();
        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats;

        m_PlayerIsNear.OnPlayerInTrigger += SetPlayerIsNear;
        m_PlayerInAttackRange.OnPlayerInTrigger += SetPlayerInAttackRange;

    }

	// Update is called once per frame
	void Update () {
		
        if (m_PlayerTransform != null)
        {
            if (!m_Animator.GetBool("IsPlayerNear"))
            {
                var difference = m_PlayerTransform.position - transform.position;
                SetLookAnimation(difference.x);

                if (m_NextAttackTime < Time.time)
                {
                    m_NextAttackTime = Time.time + m_EnemyStats.AttackSpeed;
                    DrawBulletTrailEffect(m_PlayerTransform.position);
                }
            }
        }
        else if (GameMaster.Instance.IsPlayerDead)
        {
            SetPlayerInAttackRange(false, null);
            SetPlayerIsNear(false, null);
        }
	}

    #region shoot

    private void DrawBulletTrailEffect(Vector3 whereToShoot)
    {
        if (m_PlayerTransform != null)
        {
            var explosion = Instantiate(m_ExplosionPrefab, m_FirePoint.position,
                        Quaternion.identity);

            explosion.GetComponent<ExplosionObject>().SetTarget(m_PlayerTransform);

            AudioManager.Instance.Play(m_ShootAudio);
        }
    }

    private void SetLookAnimation(float x)
    {
        var isLookingRight = x > 0 ? true : false;

        m_Animator.SetBool("IsPlayerOnTheRight", isLookingRight);
    }

    #endregion

    private void SetPlayerIsNear(bool value, Transform player)
    {
        m_IsPlayerNearTurret = value;

        if (!value)
        {
            StopAllCoroutines();
            StartCoroutine(WaitBeforeAppear());
        }
        else
        {
            m_Animator.SetBool("IsPlayerNear", true);

            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void SetPlayerInAttackRange(bool value, Transform player)
    {
        m_Animator.SetBool("IsAttacking", value);

        if (value)
        {
            m_PlayerTransform = player;

            if (m_NextAttackTime < Time.time)
                m_NextAttackTime = Time.time + .5f;
        }
        else
            m_PlayerTransform = null;
    }
}

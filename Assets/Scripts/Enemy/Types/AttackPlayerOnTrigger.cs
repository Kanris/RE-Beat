using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayerOnTrigger : MonoBehaviour {

    #region private fields

    private bool m_IsAttack;
    private PlayerStats m_PlayerStats;
    private Animator m_Animator;
    private Enemy m_EnemyStats;

    #endregion

    private void Start()
    {
        InitializeAnimator();
        InitializeStats();
    }

    private void InitializeAnimator()
    {
        m_Animator = transform.parent.GetComponent<Animator>();
    }

    private void InitializeStats()
    {
        m_EnemyStats = transform.parent.GetComponent<EnemyStatsGO>().EnemyStats;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsAttack)
        {
            m_IsAttack = true;
            m_PlayerStats = collision.GetComponent<Player>().playerStats;

            StartCoroutine(Attack());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsAttack)
        {
            m_PlayerStats = null;
            m_EnemyStats.ChangeIsPlayerNear(false);
        }
    }

    private IEnumerator Attack()
    {
        m_Animator.SetBool("Attack", true);

        yield return new WaitForSeconds(0.2f);

        if (m_PlayerStats != null)
            m_PlayerStats.TakeDamage(m_EnemyStats.DamageAmount);

        m_Animator.SetBool("Attack", false);

        yield return new WaitForSeconds(m_EnemyStats.AttackSpeed);

        m_IsAttack = false;
    }

}

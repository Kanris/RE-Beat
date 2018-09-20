using System.Collections;
using UnityEngine;

public class AttackPlayerOnTrigger : MonoBehaviour {

    #region private fields

    private bool m_IsAttack; //indicates that enemy is attacking
    private PlayerStats m_PlayerStats; //player stats to attack
    private Enemy m_EnemyStats; //to get damage amount
    private Animator m_Animator; //to play hit animation

    #endregion

    private void Start()
    {
        m_Animator = transform.parent.GetComponent<Animator>();
        m_EnemyStats = transform.parent.GetComponent<EnemyStatsGO>().EnemyStats;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsAttack) //if enemy is not attacking
        {
            m_IsAttack = true;
            m_PlayerStats = collision.GetComponent<Player>().playerStats; //get player stats

            StartCoroutine(Attack()); //start attac
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

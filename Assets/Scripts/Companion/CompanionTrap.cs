using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionTrap : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField, Range(0f, 50f)] private float m_DamageDealt = 1f; //damage dealt by this trap
    [SerializeField, Range(1f, 10f)] private float m_HoldTime = 3f; //how long this trap is hold enemy
    [SerializeField, Range(1f, 10f)] private float m_LifeTime = 6f; //how long this trap will exist before destroy from scene

    private float m_LifeCountTime; //trap's life time counter
    private float m_HoldCountTime; //trap's hold enemy time counter

    private Animator m_Animator; //trap's animator

    private EnemyMovement m_EnemyInTrap; //enemie's, that in this trap, movement script 

    // Start is called before the first frame update
    void Start()
    {
        m_LifeCountTime = m_LifeTime + Time.time; //time when trap is has to be destroyed (if there is not enemies in trap)
        m_Animator = GetComponent<Animator>(); //get trap's animator
    }

    // Update is called once per frame
    void Update()
    {
        if (m_LifeCountTime < Time.time && m_EnemyInTrap == null) //if trap's life time is over and there is no enemy in trap
        {
            DestroyTrap(); //remove trap from scene
        }

        if (m_EnemyInTrap != null) //if enemy in trap
        {
            if (m_HoldCountTime < Time.time) //if hold time is expire
            {
                DestroyTrap(); //remove trap from scene
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && m_EnemyInTrap == null) //if enemies near trigger and there is no enemy in this trap already
        {
            TriggerTrap(collision); //try to place enemy in trap
        }
    }

    //try to place enemy in trap
    private void TriggerTrap(Collider2D collision)
    {
        m_EnemyInTrap = collision.GetComponent<EnemyMovement>();

        if (m_EnemyInTrap != null) //if enemy with enemymovement script is in trap
        {
            HoldInTrap(true); //place enemy in trap
            m_Animator.SetTrigger("Triggered"); //play triggered animation
            m_HoldCountTime = m_HoldTime + Time.time; //set hold time
        }
    }

    //hold or release enemy (base on value)
    private void HoldInTrap(bool value)
    {
        //if there is enemy in trap
        if (m_EnemyInTrap != null)
        {
            m_EnemyInTrap.enabled = !value; //disable/enable enemies movement script
            m_EnemyInTrap.GetComponent<Animator>().SetBool("isWalking", !value); //player/stop enemies walking animation
        }
    }

    //remove trap from scene
    private void DestroyTrap()
    {
        HoldInTrap(false); //release enemy

        Destroy(gameObject); //remove trap from scene
    }
}

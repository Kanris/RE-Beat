using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class DroneKamikaze : MonoBehaviour
{
    [SerializeField] private LayerMask WhatIsEnemy;

    [Header("Stats")]
    [SerializeField, Range(1, 5)] private int Health = 1; //drone health
    [SerializeField, Range(0, 10)] private int DamageAmount = 1; //damage to the player
    [SerializeField, Range(1f, 5f)] private float AttackSpeed = 0.5f;
    [SerializeField, Range(0f, 10f)] private float DeathDetonationTimer = 2f; //time before destroying drone

    [Header("Effects")]
    [SerializeField] private GameObject DeathParticles; //particles that shows after drone destroy

    private Rigidbody2D m_Rigidbody;
    private Animator m_Animator;

    private bool m_IsDestroying = false; //is drone going to blow up

    #region initialize

    // Use this for initialization
    private void Start()
    {

        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();

        InitializeKamikaze();

    }

    private void InitializeKamikaze()
    {
        //initialize random direction
        var randX = Random.Range(0, 2);
        var randY = Random.Range(0, 2);

        m_Rigidbody.velocity = new Vector2(randX == 0 ? -2f : 2f, randY == 0 ? -2f : 2f);
    }

    #endregion

    private void FixedUpdate()
    {
        m_Rigidbody.velocity =
                        new Vector2(Mathf.Clamp(m_Rigidbody.velocity.x, -5f, 5f), Mathf.Clamp(m_Rigidbody.velocity.y, -5f, 5f));
    }

    #region collision/trigger detections

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsDestroying)
        {
            collision.transform.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);

            StartCoroutine(DestroyDrone());
        }

        if (collision.gameObject.layer == 14 & m_IsDestroying) //object layer - ground
        {
            StartCoroutine(DestroyDrone(DeathDetonationTimer));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttackRange") & !m_IsDestroying)
        {
            Health--;

            if (Health <= 0)
            {
                PlayTriggerAnimation("Destroy");

                m_IsDestroying = true;
                m_Rigidbody.sharedMaterial = null;
                m_Rigidbody.gravityScale = 3f;
            }
            else
                PlayTriggerAnimation("Hit");
        }
    }

    #endregion

    private IEnumerator DestroyDrone(float waitTimeBeforeDestroy = 0f)
    {
        yield return new WaitForSeconds(waitTimeBeforeDestroy);

        var destroyParticles = Instantiate(DeathParticles, transform.position, Quaternion.identity);
        Destroy(destroyParticles, 1f);

        var hit2D = Physics2D.OverlapCircle(transform.position, 2, WhatIsEnemy);

        if (hit2D != null)
        {
            var playerStats = hit2D.GetComponent<Player>().playerStats;

            playerStats.TakeDamage(DamageAmount);
            playerStats.DebuffPlayer(DebuffPanel.DebuffTypes.Defense, 5f);
        }

        Destroy(gameObject);
    }

    private void PlayTriggerAnimation(string name)
    {
        m_Animator.SetTrigger(name);
    }
}

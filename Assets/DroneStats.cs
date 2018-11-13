using UnityEngine;
using System.Collections;

public class DroneStats : MonoBehaviour
{

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnDroneDestroy;

    [Header("Drone stats")]
    [SerializeField, Range(0f, 10f)] private float DeathDetonationTimer = 2f; //time before destroying drone
    [SerializeField, Range(1, 5)] private int Health = 1; //drone health
    [SerializeField, Range(0, 100)] private int ScrapAmount = 50;

    [Header("Attack")]
    [SerializeField] private LayerMask WhatIsEnemy;
    [Range(0, 10)] public int DamageAmount = 1; //damage to the player
    [Range(1f, 5f)] public float AttackSpeed = 0.5f;
    [SerializeField] private bool m_DestroyOnCollision = false;

    [Header("Effects")]
    [SerializeField] private GameObject DeathParticles; //particles that shows after drone destroy

    private Rigidbody2D m_Rigidbody;
    private Animator m_Animator;
    [HideInInspector] public bool m_IsDestroying = false; //is drone going to blow up

    // Use this for initialization
    void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsDestroying)
        {
            collision.transform.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);

            if (m_DestroyOnCollision)
            {
                m_IsDestroying = true;
                StartCoroutine(DestroyDrone());
            }
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
                DetroySequence();
            }
            else
                PlayTriggerAnimation("Hit");
        }
    }

    private void DetroySequence()
    {
        PlayTriggerAnimation("Destroy");

        m_IsDestroying = true;
        m_Rigidbody.sharedMaterial = null;
        m_Rigidbody.gravityScale = 3f;

        Destroy(GetComponent<TrailRenderer>());

        OnDroneDestroy(true);
    }

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

        PlayerStats.Scrap = ScrapAmount;

        Destroy(gameObject.transform.parent.gameObject);
    }

    private void PlayTriggerAnimation(string animName)
    {
        m_Animator.SetTrigger(animName);
    }
}

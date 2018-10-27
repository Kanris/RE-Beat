﻿using UnityEngine;

public class Fireball : MonoBehaviour {

    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float DestroyTime;

    [SerializeField] private int DamageAmount = 2;
    [SerializeField] private float Speed = 2.5f;
    [SerializeField] private bool isNeedRotation = true;
    [SerializeField] private float DestroyDelay = 0.2f;
    [SerializeField] private LayerMask m_LayerMask;

    private Animator m_Animator;
    private bool isDestroying = false;

	// Use this for initialization
	void Start () {

        InitializeDirection();

        InitializeAnimator();

        DestroyTime = Time.time + 5f;
    }

    private void InitializeDirection()
    {
        if (isNeedRotation)
        {
            if (Direction == Vector3.left)
                transform.Rotate(0, 0, 180);

            else if (Direction == Vector3.up)
                transform.Rotate(0, 0, 90);

            else if (Direction == Vector3.down)
                transform.Rotate(0, 0, 270);

            else if (Direction == new Vector3(1, 1, 0))
                transform.Rotate(0, 0, 50);

            else if (Direction == new Vector3(-1, 1, 0))
                transform.Rotate(0, 0, 140);

            else if (Direction == new Vector3(-1, -1, 0))
                transform.Rotate(0, 0, 230);

            else if (Direction == new Vector3(1, -1, 0))
                transform.Rotate(0, 0, 310);
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Fireball.InitializeAnimator: Can't find component - Animator on Gameobject");
        }
    }

    private void Update()
    {
        if (Time.time >= DestroyTime & !isDestroying)
            DestroyFireball();
    }

    // Update is called once per frame
    void FixedUpdate() {

        if (!isDestroying)
            transform.position += Direction * Time.fixedDeltaTime * Speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((m_LayerMask & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer) & !isDestroying)
        {
            Stats statsToTakeDamage = null;
            var divider = 1;

            if (collision.transform.CompareTag("Player"))
                statsToTakeDamage = collision.gameObject.GetComponent<Player>().playerStats;
            else
                if (collision.transform.CompareTag("Enemy"))
            {
                if (collision.gameObject.GetComponent<EnemyStatsGO>() != null)
                {
                    statsToTakeDamage = collision.gameObject.GetComponent<EnemyStatsGO>().EnemyStats;
                    divider = 0;
                }
                else
                    DestroyFireball();
            }

            Damage(statsToTakeDamage, divider);
        }
        else
            DestroyFireball();
    }

    private void Damage(Stats statsToTakeDamage, int divider)
    {
        if (statsToTakeDamage != null)
            statsToTakeDamage.TakeDamage(DamageAmount, divider);

        DestroyFireball();
    }

    private void DestroyFireball()
    {
        isDestroying = true;

        m_Animator.SetBool("isCollide", isDestroying);

        Destroy(gameObject, DestroyDelay);
    }
}

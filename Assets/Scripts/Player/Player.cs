using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;
using System;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {

    #region public fields

    public delegate void VoidDelegate();

    public PlayerStats playerStats; //player stats

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField, Range(-3, -20)] private float YFallDeath = -3f; //max y fall 

    [SerializeField] private GameObject m_AttackRangeAnimation; //player attack range
    [SerializeField] private AnimationClip m_AttackAnimation; //attack animation
    [SerializeField] private Transform m_AttackPosition; //attack position
    [SerializeField, Range(0.1f, 5f)] private float m_AttackRangeX; //attack range x
    [SerializeField, Range(0.1f, 5f)] private float m_AttackRangeY; //attack range y
    [SerializeField] private Transform m_FirePosition;
    [SerializeField] private LayerMask m_WhatIsEnemy; //defines what is enemy
    [SerializeField] private Audio m_AttackSound; //player attack sound
    [SerializeField] private GameObject m_ShootEffect;

    #endregion

    private Animator m_Animator; //player animator
    private Rigidbody2D m_Rigidbody2D;
    private float m_YPositionBeforeJump; //player y position before jump
    private bool isPlayerBusy = false; //is player busy right now (read map, read tasks etc.)
    private bool m_IsAttacking; //is player attacking
    private float m_MeleeAttackCooldown; //next attack time
    private float m_RangeAttackCooldown;
    private int m_EnemyHitDirection = 1;

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        Camera.main.GetComponent<CinemachineFollow>().SetCameraTarget(transform); //set new camera target

        m_Animator = GetComponent<Animator>(); //reference to the player animator
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        playerStats.Initialize(gameObject); //initialize player stats

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnGamePause += TriggerPlayerBussy;
        DialogueManager.Instance.OnDialogueInProgressChange += TriggerPlayerBussy;
        InfoManager.Instance.OnJournalOpen += TriggerPlayerBussy;

        //GetComponent<PlatformerCharacter2D>().OnLandEvent += () => AudioManager.Instance.Play("Land"); 
    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        JumpHeightControl(); //check player jump height

        Attack(m_MeleeAttackCooldown, "Fire1", () =>
        {
            m_MeleeAttackCooldown = Time.time + PlayerStats.MeleeAttackSpeed; //next attack time
            StartCoroutine(MeleeAttack());
        });

        Attack(m_RangeAttackCooldown, "Fire2", () =>
        {
            m_RangeAttackCooldown = Time.time + PlayerStats.RangeAttackSpeed; //next attack time
            UIManager.Instance.BulletCooldown(PlayerStats.RangeAttackSpeed);
            DrawBullet();
        });
    }

    private void DrawBullet()
    {
        float whereToShoot = 0f;

        if (transform.localScale.x == -1)
            whereToShoot = 180f;

        Instantiate(m_ShootEffect, m_FirePosition.position,
                    Quaternion.Euler(0f, 0f, whereToShoot));
    }

    private void Attack(float timeToCheck, string buttonToCheck, VoidDelegate action)
    {
        if (timeToCheck < Time.time) //can player attack
        {
            if (!isPlayerBusy) //is not player bussy
            {
                if (CrossPlatformInputManager.GetButtonDown(buttonToCheck))
                {
                    action();
                }
            }
        }
    }

    private IEnumerator MeleeAttack()
    {
        m_AttackRangeAnimation.SetActive(true); //play attack animation

        AudioManager.Instance.Play(m_AttackSound); //attack sound

        var enemiesToDamage = Physics2D.OverlapBoxAll(m_AttackPosition.position, 
            new Vector2(m_AttackRangeX, m_AttackRangeY), 0, m_WhatIsEnemy); //check is there enemy in attack range

        //hit every enemy in attack range
        foreach (var enemy in enemiesToDamage)
        {
            float distance = enemy.Distance(GetComponent<CapsuleCollider2D>()).distance;

            if (enemy.GetComponent<EnemyStatsGO>() != null)
                enemy.GetComponent<EnemyStatsGO>().TakeDamage(playerStats, GetHitZone(distance));
        }

        yield return new WaitForSeconds(m_AttackAnimation.length); //wait until animation play
        m_AttackRangeAnimation.SetActive(false); //stop attack animation
    }

    private int GetHitZone(float distance)
    {
        int zone = 3;

        if (m_AttackRangeX / 3 >= distance)
            zone = 1;
        else if (m_AttackRangeX / 3 * 2 >= distance)
            zone = 2;

        return zone;
        //0-33 34-66 67-100
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_AttackPosition.position, new Vector3(m_AttackRangeX, m_AttackRangeY, 1));
    }

    private void FixedUpdate()
    {
        if (m_Animator.GetBool("Hit"))
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(m_EnemyHitDirection * playerStats.m_ThrowX, playerStats.m_ThrowY);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            var dir = (new Vector2(transform.position.x, transform.position.y) - collision.contacts[0].point).normalized;

            m_EnemyHitDirection = dir.x > 0 ? 1 : -1;
        }
    }

    private void JumpHeightControl()
    {
        //if player is not on the ground
        if (!m_Animator.GetBool("Ground")) 
        {
            //check is player move from y restrictions
            if (m_YPositionBeforeJump + YFallDeath > transform.position.y & !GameMaster.Instance.IsPlayerDead)
            {
                playerStats.KillPlayer();
            }
        }
        else //save current position before jump
        {
            m_YPositionBeforeJump = transform.position.y;
        }
    }

    #endregion

    #region public methods

    public void TriggerPlayerBussy(bool value)
    {
        isPlayerBusy = value;
    }

    #endregion
}

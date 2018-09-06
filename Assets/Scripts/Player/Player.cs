using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {

    #region serialize fields

    [SerializeField, Range(-3, -200)] private float YBoundaries = -20f;
    [SerializeField, Range(-3, -20)] private float YFallDeath = -3f;

    [SerializeField] private GameObject AttackRangeAnimation;
    [SerializeField] private Transform m_AttackPosition;
    [SerializeField, Range(0.1f, 5f)] private float m_AttackRangeX;
    [SerializeField, Range(0.1f, 5f)] private float m_AttackRangeY;
    [SerializeField] private LayerMask WhatIsEnemy;
    [SerializeField] private string AttackSound = "Player Attack";

    #endregion

    #region private fields

    private Animator m_Animator;

    private float m_YPositionBeforeJump;
    private bool isPlayerBusy = false;
    private bool m_IsAttacking;
    private float m_AttackUpdateTime;

    #endregion

    #region public fields

    public PlayerStats playerStats;
    
    [HideInInspector] public bool IsDamageFromFace;

    #endregion

    #region Initialize

    private void Start()
    {
        playerStats.Initialize(gameObject);

        InitializeAnimator();

        PauseMenuManager.Instance.OnGamePause += TriggerPlayerBussy;
        DialogueManager.Instance.OnDialogueInProgressChange += TriggerPlayerBussy;

        GetComponent<PlatformerCharacter2D>().OnLandEvent += PlayLandSound;
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Player.InitializeAnimator: Can't find Animator component on Gameobject");
        }
    }

    #endregion

    private void PlayLandSound()
    {
        AudioManager.Instance.Play("Land");
    }

    // Update is called once per frame
    private void Update () {
		
        JumpHeightControl();

        if (m_AttackUpdateTime < Time.time)
        {
            if (!isPlayerBusy)
            {
                if (CrossPlatformInputManager.GetButtonDown("Fire1"))
                {
                    m_AttackUpdateTime = Time.time + PlayerStats.AttackSpeed;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (m_Animator.GetBool("Hit"))
        {
            GetComponent<Rigidbody2D>().velocity = GetThrowBackVector();
        }
    }

    private IEnumerator Attack()
    {
        AttackRangeAnimation.SetActive(true); //attack animation

        AudioManager.Instance.Play(AttackSound);

        var enemiesToDamage = Physics2D.OverlapBoxAll(m_AttackPosition.position, 
            new Vector2(m_AttackRangeX, m_AttackRangeY), 0, WhatIsEnemy);

        foreach (var enemy in enemiesToDamage)
        {
            enemy.GetComponent<EnemyStatsGO>().EnemyStats.TakeDamage(PlayerStats.DamageAmount);
        }

        yield return new WaitForSeconds(0.2f);

        AttackRangeAnimation.SetActive(false); //attack animation
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_AttackPosition.position, new Vector3(m_AttackRangeX, m_AttackRangeY, 1));
    }

    private Vector2 GetThrowBackVector()
    {
        var xThrowValue = -playerStats.m_ThrowX;

        if (transform.localScale.x.CompareTo(1f) != 0)
        {
            xThrowValue *= -1;
        }

        if (!IsDamageFromFace)
            xThrowValue *= -1;

        return new Vector2(xThrowValue, playerStats.m_ThrowY);
    }

    private void JumpHeightControl()
    {
        if (transform.position.y <= YBoundaries)
        {
            playerStats.TakeDamage(playerStats.MaxHealth);
        }

        if (!m_Animator.GetBool("Ground"))
        {
            if (m_YPositionBeforeJump + YFallDeath >= transform.position.y & !GameMaster.Instance.isPlayerDead)
            {
                playerStats.TakeDamage(999);
            }
        }
        else
        {
            m_YPositionBeforeJump = transform.position.y;
        }
    }

    public void TriggerPlayerBussy(bool value)
    {
        isPlayerBusy = value;
    }
}

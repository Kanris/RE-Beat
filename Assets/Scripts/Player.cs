using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {

    [SerializeField, Range(-3, -200)] private float YBoundaries = -20f;
    [SerializeField, Range(-3, -20)] private float YFallDeath = -3f;
    [SerializeField] private float ThrowX = 0.08f;
    [SerializeField] private float ThrowY = 0.1f;
    [SerializeField] private GameObject AttackRange;
    [SerializeField] private string AttackSound = "Player Attack";

    private float m_YPositionBeforeJump;
    private Animator m_Animator;
    private Vector2 m_ThrowBackVector;
    private bool m_IsAttacking = false;
    private bool m_IsInCooldown = false;
    private bool isPlayerBusy = false;

    public PlayerStats playerStats;

    [HideInInspector] public bool isPlayerThrowingBack;
    [HideInInspector] public bool IsDamageFromFace;

    #region Initialize

    private void Start()
    {
        playerStats.Initialize(gameObject);

        InitializeAnimator();

        PauseMenuManager.Instance.OnGamePause += TriggerPlayerBussy;
        DialogueManager.Instance.OnDialogueInProgressChange += TriggerPlayerBussy;
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

    // Update is called once per frame
    private void Update () {
		
        JumpHeightControl();

        if (IsPlayerCanAttack())
        {
            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            {
                StartCoroutine(Attack());
            }
        }
    }

    private IEnumerator Attack()
    {
        if (!m_IsAttacking)
        {

            m_IsInCooldown = true;

            m_IsAttacking = true;
            AttackRange.SetActive(true);

            AudioManager.Instance.Play(AttackSound);

            yield return new WaitForSeconds(0.1f);

            m_IsAttacking = false;

            yield return new WaitForSeconds(PlayerStats.AttackSpeed);

            m_IsInCooldown = false;
            AttackRange.SetActive(m_IsAttacking);
        }
    }

    private bool IsPlayerCanAttack()
    {
        var isPlayerCanAttack = false;

        if (!isPlayerBusy)
            if (!m_IsAttacking & !m_IsInCooldown)
                isPlayerCanAttack = true;

        return isPlayerCanAttack;
    }

    private void FixedUpdate()
    {
        if (m_Animator.GetBool("Damage"))
        {
            if (!isPlayerThrowingBack)
            {
                m_ThrowBackVector = GetThrowBackVector();
                isPlayerThrowingBack = true;
            }
            else
            {
                GetComponent<Rigidbody2D>().position += m_ThrowBackVector;
            }
        }
    }

    private Vector2 GetThrowBackVector()
    {
        var throwBackVector = Vector2.zero;

        if (transform.localScale.x.CompareTo(1f) == 0)
        {
            throwBackVector = new Vector2(-ThrowX, ThrowY);
        }
        else
        {
            throwBackVector = new Vector2(ThrowX, ThrowY);
        }

        if (!IsDamageFromFace)
        {
            throwBackVector = new Vector2(-throwBackVector.x, throwBackVector.y);
        }

        return throwBackVector;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") & m_IsAttacking)
        {
            var enemyStats = GetStats(collision);

            if (enemyStats != null)
            {
                enemyStats.TakeDamage(PlayerStats.DamageAmount);
            }
        }
    }

    private Stats GetStats(Collider2D collision)
    {
        Stats enemyStats = null;

        if (collision.GetComponent<PatrolEnemy>() != null)
        {
            enemyStats = collision.GetComponent<PatrolEnemy>().EnemyStats;
        }
        else if (collision.GetComponent<RangeEnemy>() != null)
        {
            enemyStats = collision.GetComponent<RangeEnemy>().EnemyStats;
        }

        return enemyStats;
    }

    public void TriggerPlayerBussy(bool value)
    {
        isPlayerBusy = value;
    }
}

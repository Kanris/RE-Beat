using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {

    [SerializeField, Range(-3, -200)] private float YBoundaries = -20f;
    [SerializeField, Range(-3, -20)] private float YFallDeath = -3f;
    [SerializeField] private float ThrowX = 0.08f;
    [SerializeField] private float ThrowY = 0.1f;
    public PlayerStats playerStats;

    private float m_YPositionBeforeJump;
    private Animator m_Animator;

    public bool isPlayerTakeDamage;

    private void Start()
    {
        playerStats.Initialize(gameObject);

        InitializeAnimator();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Player.InitializeAnimator: Can't find Animator component on Gameobject");
        }
    }

    // Update is called once per frame
    private void Update () {
		
        if (transform.position.y <= YBoundaries)
        {
            playerStats.TakeDamage(playerStats.MaxHealth);
        }

        JumpHeightControl();

    }

    private void FixedUpdate()
    {
        if (isPlayerTakeDamage)
        {
            if (transform.localScale.x == 1)
            {
                GetComponent<Rigidbody2D>().position += new Vector2(-ThrowX, ThrowY);
            }
            else
            {
                GetComponent<Rigidbody2D>().position += new Vector2(ThrowX, ThrowY);
            }
        }
    }

    private void JumpHeightControl()
    {
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
}

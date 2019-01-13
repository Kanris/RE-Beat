using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {

    #region public fields

    public delegate void VoidDelegate();

    public PlayerStats playerStats; //player stats

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField, Range(-3, -20)] private float YFallDeath = -3f; //max y fall 


    [SerializeField] private Transform m_AttackPosition; //attack position
    [SerializeField, Range(0.1f, 5f)] private float m_AttackRangeX; //attack range x
    [SerializeField, Range(0.1f, 5f)] private float m_AttackRangeY; //attack range y
    [SerializeField] private Transform m_FirePosition;
    [SerializeField] private LayerMask m_WhatIsEnemy; //defines what is enemy

    [Header("Effects")]
    [SerializeField] private Audio m_ShootSound; //player attack sound
    [SerializeField] private Audio m_AttackSound; //player attack sound
    [SerializeField] private GameObject m_AttackRangeAnimation; //player attack range
    [SerializeField] private AnimationClip m_AttackAnimation; //attack animation
    [SerializeField] private GameObject m_ShootEffect;
    [SerializeField] private GameObject m_LowHealthEffect;

    [Header("Camera")]
    [SerializeField] private Transform m_CameraUp;
    [SerializeField] private Transform m_CameraDown;
    [SerializeField] private Transform m_CameraRight;
    [SerializeField] private Transform m_CameraLeft;

    [Header("Vertical Dash")]
    [SerializeField] private Transform m_Center;
    [SerializeField] private GameObject m_LandEffect;

    #endregion

    private Animator m_Animator; //player animator
    private Rigidbody2D m_Rigidbody2D;
    private float m_YPositionBeforeJump; //player y position before jump
    private bool isPlayerBusy = false; //is player busy right now (read map, read tasks etc.)
    private bool m_IsAttacking; //is player attacking
    private float m_MeleeAttackCooldown; //next attack time
    private float m_RangeAttackCooldown;
    [HideInInspector] public int m_EnemyHitDirection = 1;

    private bool m_IsCreateCriticalHealthEffect;
    private bool m_IsRightStickPressed;

    private bool m_IsFallAttack = false;
    
    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        StartCoroutine( 
            Camera.main.GetComponent<Camera2DFollow>().SetTarget(transform) ); //set new camera target

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
    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        JumpHeightControl(); //check player jump height

        #region vertical dash

        if (GameMaster.Instance.m_Joystick.LeftBumper & !m_Animator.GetBool("Ground"))
        {
            if (!m_IsFallAttack)
            {
                m_IsFallAttack = true;

                m_Rigidbody2D.AddForce(new Vector2(0f, -30f), ForceMode2D.Impulse);

                m_Animator.SetBool("FallAttack", true);

                //GetComponent<PlatformerCharacter2D>().enabled = false;

                Physics2D.IgnoreLayerCollision(8, 13, true);
            }
        }

        if (m_IsFallAttack)
        {
            if (m_Animator.GetBool("Ground"))
            {
                m_IsFallAttack = false;

                m_Animator.SetBool("FallAttack", false);

                var enemiesToDamage = Physics2D.OverlapBoxAll(m_Center.position, new Vector2(2f, .5f), 0, m_WhatIsEnemy);

                foreach (var enemy in enemiesToDamage)
                {
                    if (enemy.GetComponent<EnemyStatsGO>() != null)
                    {
                        enemy.GetComponent<EnemyStatsGO>().TakeDamage(playerStats, 3, 10);
                    }
                }

                var landEffect = Instantiate(m_LandEffect);
                landEffect.transform.position = m_Center.position;

                Destroy(landEffect, 2f);

                Camera.main.GetComponent<Camera2DFollow>().Shake(.2f, .2f);

                Physics2D.IgnoreLayerCollision(8, 13, false);
                //GetComponent<PlatformerCharacter2D>().enabled = true;
            }
            else if (m_Animator.GetBool("Hit"))
            {
                m_IsFallAttack = false;

                m_Animator.SetBool("FallAttack", false);

                //GetComponent<PlatformerCharacter2D>().enabled = true;
            }
        }

        #endregion

        #region attack handler

        Attack(m_MeleeAttackCooldown, GameMaster.Instance.m_Joystick.Action3, () =>
        {
            m_MeleeAttackCooldown = Time.time + PlayerStats.MeleeAttackSpeed; //next attack time
            GameMaster.Instance.StartJoystickVibrate(1, 0.05f);
            StartCoroutine(MeleeAttack());
        });

        Attack(m_RangeAttackCooldown, GameMaster.Instance.m_Joystick.RightBumper, () =>
        {
            m_RangeAttackCooldown = Time.time + PlayerStats.RangeAttackSpeed; //next attack time
            UIManager.Instance.BulletCooldown(PlayerStats.RangeAttackSpeed);
            GameMaster.Instance.StartJoystickVibrate(1, 0.05f);

            DrawBullet();
        });

        #endregion

        #region critical health effect

        if (playerStats.CurrentHealth < 3 & !m_IsCreateCriticalHealthEffect)
        {
            m_IsCreateCriticalHealthEffect = true;
            DebuffPanel.Instance.ShowCriticalDamageSign();

            if (gameObject.activeSelf)
                StartCoroutine(CreateLowHealthEffect());
        }

        #endregion

        #region camera control right stick

        if (GameMaster.Instance.m_Joystick.RightStickY.IsPressed)
        {
            if (Mathf.Abs(GameMaster.Instance.m_Joystick.RightStickY.Value) > 0.4f) //if right stick Y pressed
            {
                var cameraTarget = GameMaster.Instance.m_Joystick.RightStickY.Value > 0 ? m_CameraUp : m_CameraDown;

                Camera.main.GetComponent<Camera2DFollow>().ChangeTarget(cameraTarget);

                m_IsRightStickPressed = true;
            }
            else if (Mathf.Abs(GameMaster.Instance.m_Joystick.RightStickX.Value) > 0.4f) //if right stick X pressed
            {
                var cameraTarget = GameMaster.Instance.m_Joystick.RightStickX.Value * transform.localScale.x > 0 ? m_CameraRight : m_CameraLeft;

                Camera.main.GetComponent<Camera2DFollow>().ChangeTarget(cameraTarget);

                m_IsRightStickPressed = true;
            }
        }
        else if (m_IsRightStickPressed)
        {
            m_IsRightStickPressed = false;
            Camera.main.GetComponent<Camera2DFollow>().ChangeTarget(transform);
        }

        #endregion
    }

    private IEnumerator CreateLowHealthEffect()
    {
        if (playerStats.CurrentHealth < 3)
        {
            var instLowHealthEffect = Instantiate(m_LowHealthEffect);
            instLowHealthEffect.transform.position = transform.position;

            Destroy(instLowHealthEffect, 3f);

            yield return new WaitForSeconds(Random.Range(.5f, 2f));

            StartCoroutine(CreateLowHealthEffect());
        }
        else
        {
            m_IsCreateCriticalHealthEffect = false;
        }
    }

    //show low health effect
    private void OnEnable()
    {
        if (playerStats.CurrentHealth < 3 & GameMaster.Instance.m_IsPlayerReturning)
        {
            m_IsCreateCriticalHealthEffect = true;
            DebuffPanel.Instance.ShowCriticalDamageSign();
            StartCoroutine(CreateLowHealthEffect());
        }
    }

    private void DrawBullet()
    {
        if (m_FirePosition != null)
        {
            float whereToShoot = 0f;

            if (transform.localScale.x == -1)
                whereToShoot = 180f;

            Instantiate(m_ShootEffect, m_FirePosition.position,
                        Quaternion.Euler(0f, 0f, whereToShoot));

            AudioManager.Instance.Play(m_ShootSound);
        }
    }

    private void Attack(float timeToCheck, InControl.InputControl inputControl, VoidDelegate action)
    {
        if (timeToCheck < Time.time) //can player attack
        {
            if (!isPlayerBusy) //is not player bussy
            {
                if (inputControl.WasPressed)
                {
                    action();
                }
            }
        }
    }

    private IEnumerator MeleeAttack()
    {
        if (m_AttackRangeAnimation != null)
        {
            m_AttackRangeAnimation.SetActive(true); //play attack animation

            m_AttackRangeAnimation.GetComponent<Animator>().speed = PlayerStats.MeleeAttackSpeed;

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
        if (m_AttackPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(m_AttackPosition.position, new Vector3(m_AttackRangeX, m_AttackRangeY, 1));
        }

        if (m_Center != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(m_Center.position, new Vector3(2f, .5f, 1f));
        }
    }

    private void FixedUpdate()
    {
        if (m_Animator.GetBool("Hit"))
        {
            GetComponent<Rigidbody2D>().velocity = 
                new Vector2(m_EnemyHitDirection * playerStats.m_ThrowX, playerStats.m_ThrowY);
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
            if (m_YPositionBeforeJump + YFallDeath > transform.position.y 
                & !GameMaster.Instance.IsPlayerDead & !GameMaster.Instance.m_IsPlayerReturning)
            {
                playerStats.ReturnPlayerOnReturnPoint();
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

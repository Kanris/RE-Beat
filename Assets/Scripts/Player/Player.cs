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
    [SerializeField] private Transform m_FirePosition; //position to fire
    [SerializeField] private LayerMask m_WhatIsEnemy; //defines what is enemy

    [Header("Effects")]
    [SerializeField] private Audio m_ShootSound; //player attack sound
    [SerializeField] private Audio m_AttackSound; //player attack sound
    [SerializeField] private GameObject m_AttackRangeAnimation; //player attack range
    [SerializeField] private AnimationClip m_AttackAnimation; //attack animation
    [SerializeField] private GameObject m_ShootEffect;
    [SerializeField] private GameObject m_LowHealthEffect;

    [Header("Camera")]
    [SerializeField] private Transform m_CameraUp; //camera up transform for right stick cam control
    [SerializeField] private Transform m_CameraDown; //camera down transform for right stick cam control
    [SerializeField] private Transform m_CameraRight; //camera right transform for right stick cam control
    [SerializeField] private Transform m_CameraLeft; //camera left transform for right stick cam control

    [Header("Vertical Dash")]
    [SerializeField] private Transform m_Center; //center to draw box for fall attack
    [SerializeField] private GameObject m_LandEffect; //particles that will be player when player land
    [SerializeField] private Audio m_FallAttackAudio; //fall attack sound

    #endregion

    private Animator m_Animator; //player animator
    private Rigidbody2D m_Rigidbody2D; //player's rigidbody
    private float m_YPositionBeforeJump; //player y position before jump
    private bool m_IsPlayerBusy = false; //is player busy right now (read map, read tasks etc.)

    private float m_MeleeAttackCooldown; //next attack time
    private float m_RangeAttackCooldown; //range attack cooldown
    private float m_FallAttackCooldown; //fall attack cooldown

    private float m_InvisibleAbilityCooldown;

    [HideInInspector] public int m_EnemyHitDirection = 1; //from where player receive damage

    private bool m_IsCreateCriticalHealthEffect; //indicates that player should display critical health particles
    private bool m_IsRightStickPressed; //indicates that player pressed right stick (to move camera)

    private bool m_IsFallAttack = false; //indicates that player is performing fall attack
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

    #region attacks

    //Generic method to execute attack logic
    private void Attack(float timeToCheck, InControl.InputControl inputControl, VoidDelegate action)
    {
        if (timeToCheck < Time.time) //can player attack
        {
            if (inputControl.WasPressed)
            {
                action();
            }
        }
    }

    //generic method to damage enemies in area
    private void DamageEnemiesInArea(Vector3 centerPosition, float width, float height, int damageAmount = 0)
    {
        //get all enemies in box area
        var enemiesToDamage = Physics2D.OverlapBoxAll(centerPosition,
            new Vector2(width, height), 0, m_WhatIsEnemy);

        //deal damage to all enemies in area
        foreach (var enemy in enemiesToDamage)
        {
            if (enemy.GetComponent<EnemyStatsGO>() != null) //if enemy hitted
            {
                var distance = enemy.Distance(GetComponent<CapsuleCollider2D>()).distance;

                enemy.GetComponent<EnemyStatsGO>().TakeDamage
                    (damageAmount == 0 ? playerStats : null, GetHitZone(distance), damageAmount);
            }
            else if (enemy.GetComponent<WorldObjectStats>() != null) //if world object hitted
            {
                enemy.GetComponent<WorldObjectStats>().TakeDamage();
            }
        }
    }

    #region fall attack

    //begin of fall attack
    private IEnumerator StartFallAttack()
    {
        if (!m_IsFallAttack) //is fall attack is not performing and player is not bussy
        {
            m_IsFallAttack = true; //fall attack is performing

            Physics2D.IgnoreLayerCollision(8, 13, true); //ignore all enemies

            m_Animator.SetBool("FallAttack", true); //play fall attack animation

            //hold player in air for .1f seconds
            m_Rigidbody2D.velocity = Vector2.zero;
            m_Rigidbody2D.gravityScale = 0f;

            yield return new WaitForSeconds(.2f);

            //push player to the ground
            m_Rigidbody2D.AddForce(new Vector2(0f, -30f), ForceMode2D.Impulse);
        }
    }

    private void StopFallAttack()
    {
        DamageEnemiesInArea(m_Center.position, 
            m_LandEffect.GetComponent<ParticleSystem>().shape.scale.x, 
            m_LandEffect.GetComponent<ParticleSystem>().shape.scale.y, 
            damageAmount: 10);

        //play land effect
        CreateLandParticles();

        //return player's default values and start fall attack cooldown
        ReturnPlayersDefaultValues();

        //shake screen when player land
        Camera.main.GetComponent<Camera2DFollow>().Shake(.2f, .2f);

        //play land effect sound
        AudioManager.Instance.Play(m_FallAttackAudio);
    }

    private void CreateLandParticles()
    {
        var landEffect = Instantiate(m_LandEffect); //create land particles
        landEffect.transform.position = m_Center.position; //place land particles in player feet

        Destroy(landEffect, 2f); //destroy land particles after 2 seconds
    }

    private void ReturnPlayersDefaultValues()
    {
        m_IsFallAttack = false; //fall attack is not performing

        m_Animator.SetBool("FallAttack", false); //stop play fall attack animation

        //return default values
        m_Rigidbody2D.gravityScale = 3f;
        m_Rigidbody2D.velocity = Vector2.zero;

        Physics2D.IgnoreLayerCollision(8, 13, false);

        //show cooldown in player's ui
        UIManager.Instance.FallAttackCooldown(PlayerStats.FallAttackSpeed); 
        
        //start fall attack cooldown
        m_FallAttackCooldown = PlayerStats.FallAttackSpeed + Time.time;
    }

    #endregion

    #region attack

    private void PerformMeleeAtack()
    {
        m_MeleeAttackCooldown = Time.time + PlayerStats.MeleeAttackSpeed; //next attack time
        InputControlManager.Instance.StartJoystickVibrate(1, 0.05f); //vibrate gamepad

        StartCoroutine(MeleeAttack()); //perform meleeattack
    }

    //TODO: rewrite Melee attack
    private IEnumerator MeleeAttack()
    {
        m_AttackRangeAnimation.SetActive(true); //play attack animation

        m_AttackRangeAnimation.GetComponent<Animator>().speed = PlayerStats.MeleeAttackSpeed;

        AudioManager.Instance.Play(m_AttackSound); //attack sound

        DamageEnemiesInArea(m_AttackPosition.position, m_AttackRangeX, m_AttackRangeY);

        yield return new WaitForSeconds(m_AttackAnimation.length); //wait until animation play
        m_AttackRangeAnimation.SetActive(false); //stop attack animation
    }

    //Player's range attack
    private void PerformRangeAttack()
    {
        m_RangeAttackCooldown = Time.time + PlayerStats.RangeAttackSpeed; //next attack time
        InputControlManager.Instance.StartJoystickVibrate(1, 0.05f);
        UIManager.Instance.BulletCooldown(PlayerStats.RangeAttackSpeed); //show bullet cooldown in player's ui

        var whereToShoot = transform.localScale.x == -1 ? 180f : 0f; // direction of shooting
        AudioManager.Instance.Play(m_ShootSound);

        Instantiate(m_ShootEffect, m_FirePosition.position,
                    Quaternion.Euler(0f, 0f, whereToShoot)); //create bullet prefab on scene
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

    #endregion

    #endregion

    #region abilities

    private IEnumerator StealthAbility(float timeStealth)
    {
        IsInvisible(true);

        yield return new WaitForSeconds(timeStealth);

        IsInvisible(false);
    }

    public void IsInvisible(bool value)
    {
        var alphaValue = value ? .4f : 1f;
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color.ChangeColor(a: alphaValue);

        Physics2D.IgnoreLayerCollision(8, 13, value);
    }

    #endregion

    #region camera control

    private void CameraRightStickControl()
    {
        if (InputControlManager.Instance.m_Joystick.RightStick.State) //if right stick pressed
        {
            if (Mathf.Abs(InputControlManager.Instance.m_Joystick.RightStickY.Value) > 0.4f) //if right stick Y pressed
            {
                var cameraTarget = InputControlManager.Instance.m_Joystick.RightStickY.Value > 0 ? m_CameraUp : m_CameraDown; //determine where to look

                Camera.main.GetComponent<Camera2DFollow>().ChangeTarget(cameraTarget); //move camera to that positon

                m_IsRightStickPressed = true; //indicates that camera is not looking at player
            }
            else if (Mathf.Abs(InputControlManager.Instance.m_Joystick.RightStickX.Value) > 0.4f) //if right stick X pressed
            {
                var cameraTarget = InputControlManager.Instance.m_Joystick.RightStickX.Value * transform.localScale.x > 0 ? m_CameraRight : m_CameraLeft; //determine where to look

                Camera.main.GetComponent<Camera2DFollow>().ChangeTarget(cameraTarget); //move camera to that positon

                m_IsRightStickPressed = true; //indicates that camera is not looking at player
            }
        }
        else if (m_IsRightStickPressed) //if camera is not looking at the player and right stick is not pressed
        {
            m_IsRightStickPressed = false; //right stick is not pressed
            Camera.main.GetComponent<Camera2DFollow>().ChangeTarget(transform); //return camera to the player
        }
    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        JumpHeightControl(); //check player jump height

        if (!GameMaster.Instance.IsPlayerDead && !m_IsPlayerBusy) //player's attacks
        {
            #region fall attack

            if (m_FallAttackCooldown < Time.time & PlayerStats.m_IsFallAttack)
            {
                if (InputControlManager.Instance.m_Joystick.LeftBumper & !m_Animator.GetBool("Ground"))
                {
                    StartCoroutine(StartFallAttack());
                }
            }

            if (m_IsFallAttack)
            {
                if (m_Animator.GetBool("Ground"))
                {
                    StopFallAttack();
                }
            }

            #endregion

            #region attack handler

            Attack(m_MeleeAttackCooldown, InputControlManager.Instance.m_Joystick.Action3, PerformMeleeAtack);

            Attack(m_RangeAttackCooldown, InputControlManager.Instance.m_Joystick.RightBumper, PerformRangeAttack);

            #endregion

        }
        else if (GameMaster.Instance.IsPlayerDead) //companion's abilities
        {
            if (m_InvisibleAbilityCooldown < Time.time) //if can use invisible ability
            {
                if (InputControlManager.Instance.m_Joystick.Action3.WasPressed) //if attack button pressed
                {
                    m_InvisibleAbilityCooldown = Time.time + PlayerStats.InvisibleTimeSpeed * 2; //set cooldown timer

                    UIManager.Instance.FallAttackCooldown(PlayerStats.InvisibleTimeSpeed * 2); //show cooldown on panel

                    StartCoroutine(StealthAbility(PlayerStats.InvisibleTimeSpeed)); //start invisible
                }
            }
        }

        #region critical health effect

        if (playerStats.CurrentHealth < 3 & !m_IsCreateCriticalHealthEffect)
        {
            m_IsCreateCriticalHealthEffect = true;
            DebuffPanel.Instance.ShowCriticalDamageSign();

            if (gameObject.activeSelf)
                StartCoroutine(CreateLowHealthEffect());
        }

        #endregion

        CameraRightStickControl();
    }

    #region low health effect

    //create low health particles on player
    private IEnumerator CreateLowHealthEffect()
    {
        //if current health less than 3
        if (playerStats.CurrentHealth < 3)
        {
            //create particle effect and destroy it after 3 sec
            var instLowHealthEffect = Instantiate(m_LowHealthEffect);
            instLowHealthEffect.transform.position = transform.position;

            Destroy(instLowHealthEffect, 3f);

            yield return new WaitForSeconds(Random.Range(.5f, 2f));

            //start create new low health particles
            StartCoroutine(CreateLowHealthEffect());
        }
        else //player is dead or over 2 health
        {
            m_IsCreateCriticalHealthEffect = false;
        }
    }

    //show low health effect
    private void OnEnable()
    {
        //if player have to show critical health effect
        if (playerStats.CurrentHealth < 3 & GameMaster.Instance.m_IsPlayerReturning)
        {
            m_IsCreateCriticalHealthEffect = true; //indicates that player creates particle critical effects

            DebuffPanel.Instance.ShowCriticalDamageSign(); //show again danger sign
            StartCoroutine(CreateLowHealthEffect()); //start showing critical health effect
        }

        //if player's performing fall attack
        if (m_IsFallAttack)
        {
            //stop it
            ReturnPlayersDefaultValues();
        }
    }

    #endregion


    //visualize melee attack range and fall attack range
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
            Gizmos.DrawWireCube(m_Center.position, new Vector3(3f, .5f, 1f));
        }
    }

    private void FixedUpdate()
    {
        //if player is hitted start to throw back him
        if (m_Animator.GetBool("Hit"))
        {
            GetComponent<Rigidbody2D>().velocity = 
                new Vector2(m_EnemyHitDirection * playerStats.m_ThrowBackX, playerStats.m_ThrowBackY);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //get throw direction if player touched enemy
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
        m_IsPlayerBusy = value;
    }

    #endregion
}

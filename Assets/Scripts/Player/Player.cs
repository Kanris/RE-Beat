using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {

    #region public fields

    public PlayerStats playerStats; //player stats

    [HideInInspector] public bool IsDamageFromFace; //is player take damage from face

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField, Range(-3, -20)] private float YFallDeath = -3f; //max y fall 

    [SerializeField] private GameObject m_AttackRangeAnimation; //player attack range
    [SerializeField] private AnimationClip m_AttackAnimation; //attack animation
    [SerializeField] private Transform m_AttackPosition; //attack position
    [SerializeField, Range(0.1f, 5f)] private float m_AttackRangeX; //attack range x
    [SerializeField, Range(0.1f, 5f)] private float m_AttackRangeY; //attack range y
    [SerializeField] private LayerMask m_WhatIsEnemy; //defines what is enemy
    [SerializeField] private string m_AttackSound = "Player Attack"; //player attack sound

    #endregion

    private Animator m_Animator; //player animator
    private float m_YPositionBeforeJump; //player y position before jump
    private bool isPlayerBusy = false; //is player busy right now (read map, read tasks etc.)
    private bool m_IsAttacking; //is player attacking
    private float m_AttackUpdateTime; //next attack time
    private Vector2 m_ThrowBackVector; //throw back values

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        Camera.main.GetComponent<Camera2DFollow>().SetTarget(transform); //set new camera target

        m_Animator = GetComponent<Animator>(); //reference to the player animator

        playerStats.Initialize(gameObject); //initialize player stats

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnGamePause += TriggerPlayerBussy;
        DialogueManager.Instance.OnDialogueInProgressChange += TriggerPlayerBussy;
        InfoManager.Instance.OnJournalOpen += TriggerPlayerBussy;

        GetComponent<PlatformerCharacter2D>().OnLandEvent += () => AudioManager.Instance.Play("Land"); 
    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        JumpHeightControl(); //check player jump height

        if (m_AttackUpdateTime < Time.time) //can player attack
        {
            if (!isPlayerBusy) //is not player bussy
            {
                if (CrossPlatformInputManager.GetButtonDown("Fire1")) //if player press attack button
                {
                    m_AttackUpdateTime = Time.time + PlayerStats.AttackSpeed; //next attack time
                    StartCoroutine(Attack()); //attack
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (m_Animator.GetBool("Hit")) //if someone hit player
        {
            if (m_ThrowBackVector == Vector2.zero) //if there is not throw back values
                m_ThrowBackVector = GetThrowBackVector();
            else
                GetComponent<Rigidbody2D>().velocity = m_ThrowBackVector;
        }
        else if (m_ThrowBackVector != Vector2.zero)
            m_ThrowBackVector = Vector2.zero;
    }

    private IEnumerator Attack()
    {
        m_AttackRangeAnimation.SetActive(true); //play attack animation

        AudioManager.Instance.Play(m_AttackSound); //attack sound

        var enemiesToDamage = Physics2D.OverlapBoxAll(m_AttackPosition.position, 
            new Vector2(m_AttackRangeX, m_AttackRangeY), 0, m_WhatIsEnemy); //check is there enemy in attack range

        //hit every enemy in attack range
        foreach (var enemy in enemiesToDamage)
        {
            float distance = enemy.Distance(GetComponent<CapsuleCollider2D>()).distance;
            
            playerStats.HitEnemy(enemy.GetComponent<EnemyStatsGO>().EnemyStats, GetHitZone(distance));
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

    private Vector2 GetThrowBackVector()
    {
        var xThrowValue = -playerStats.m_ThrowX;

        if (transform.localScale.x.CompareTo(1f) != 0)
        {
            xThrowValue *= -1;
        }

        if (!IsDamageFromFace)
        {
            xThrowValue *= -1;
        }

        return new Vector2(xThrowValue, playerStats.m_ThrowY);
    }

    private void JumpHeightControl()
    {
        //if player is not on the ground
        if (!m_Animator.GetBool("Ground")) 
        {
            //check is player move from y restrictions
            if (m_YPositionBeforeJump + YFallDeath > transform.position.y & !GameMaster.Instance.isPlayerDead)
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

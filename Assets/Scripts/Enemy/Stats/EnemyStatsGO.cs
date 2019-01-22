using UnityEngine;
using System.Collections;
using UnityStandardAssets._2D;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class EnemyStatsGO : MonoBehaviour {

    public delegate void VoidDelegateBool(bool value);
    public event VoidDelegateBool OnDroneDestroy; //call when drone is destroyed

    public enum EnemyType { Regular, Drone }
    public EnemyType m_EnemyType; //type of enemy (walk or fly)

    public GameObject m_GameObjectToDestroy; //object to destroy when enemy is dead
    public Enemy EnemyStats; //current enemy stats for example: hp, attack speed etc

    [Header("Drone stats")]
    [SerializeField, Range(0f, 10f)] private float DeathDetonationTimer = 2f; //time before destroying drone
    [SerializeField] private bool m_DestroyOnCollision = false; //is drone has to explode on collision
    [SerializeField] private LayerMask m_LayerMask; //enemies for drone

    [Header("UI")]
    [SerializeField] private GameObject m_HealthUI; //enemies health ui (available only when player has needed chip)
    [SerializeField] private Image m_CurrentHealthImage; //current enemy bar hp

    [Header("Effects")]
    [SerializeField] private GameObject GroundHitParticles; //particles that creates when drone hit ground
    [SerializeField] private GameObject m_HitParticles; //particles that creates when player hit enemy
    [SerializeField] private GameObject m_Scraps; //gameobject that creates when player destroy this enemy

     
    private Rigidbody2D m_Rigidbody; //current enemy rigidbody
    private Animator m_Animator; //current enemy animator

    private float m_DestroyTimer; //timer that indicates when drone is going to explode
    [HideInInspector] public bool m_IsDestroying = false; //is drone going to blow up

    private bool m_IsReceiveDamageFromDash; //indicates that enemy receive damage from dash

    #region initialize

    // Use this for initialization
    void Start() {

        InitializeStats(); //initialize enemy stats

        InitializeComponents(); //initialize animator and rigidbody variables

    }

    public void InitializeStats()
    {
        EnemyStats.Initialize(m_GameObjectToDestroy, GetComponent<Animator>()); //initialize stats
    }

    private void InitializeComponents()
    {
        //get components from current gameobject
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    #endregion

    #region health ui

    //change ui scale when enemy is flipped
    public void ChangeUIScale(float value)
    {
        m_HealthUI.transform.localScale = new Vector3(value, 1, 1);
    }

    //change health bar fill base on the current enemy hp
    private void DisplayHealthChange()
    {
        //if player can see enemy hp and HP ui is not displayed
        if (PlayerStats.m_IsCanSeeEnemyHP && !m_HealthUI.activeSelf && !m_IsDestroying)
            m_HealthUI.SetActive(true);

        //change enemy current hp bar
        m_CurrentHealthImage.fillAmount = (float)EnemyStats.CurrentHealth / (float)EnemyStats.MaxHealth;
    }

    #endregion

    //if drone is in destroy sequence destroy it after some amount of time (because he does not hit ground)
    private void Update()
    {
        if (m_IsDestroying)
        {
            if (m_DestroyTimer < Time.time)
            {
                m_IsDestroying = false;
                StartCoroutine(DestroyDrone());
            }
        }
    }

    //general take damage method
    public void TakeDamage(PlayerStats playerStats, int zone, int damageAmount = 0, bool isBulletDamage = false)
    {
        //if enemy's shield created and TakeDamage is not called by bullet
        if (EnemyStats.CreatedShield != null && !isBulletDamage)
        {
            //apply debuff on player
            EnemyStats.CreatedShield.ApplyDebuff();
        }
        //if enemy does not have active shies - receive damage
        else if (GetComponent<EnemyStatsGO>().enabled) //if enemy stats is active and enemy can receive damage
        {
            //apply damage base on the type of the enemy
            //regular enemy receive damage by basic player's stats
            if (m_EnemyType == EnemyType.Regular)
            {
                if (playerStats != null) //calculate damage base on the combo
                    playerStats.HitEnemy(EnemyStats, zone);
                else //receive regular damage
                {
                    var throwbackValueX = PlayerStats.m_ThrowEnemyX;

                    if (isBulletDamage)
                        throwbackValueX = 0;

                    Debug.LogError("Regular damage throwback value> " + throwbackValueX);

                    EnemyStats.TakeDamage(damageAmount, throwbackValueX, 0);
                }

                //create effects base on the current health
                //if current health greater than 0
                if (EnemyStats.CurrentHealth > 0)
                    CreateHitParticles(); //create hit particles
                else //if current health is less or equal to zero
                    CreateScraps(); //create scraps gameobject
            }
            //drone enemy receive damage 1 by attack
            else
            {
                //if drone is not in destory sequence
                if (!m_IsDestroying)
                {
                    //if drone has 1 health
                    if (EnemyStats.CurrentHealth == 1)
                    {
                        HideHealthUI(); //hide health ui if it's active
                        //start destroy sequence
                        DetroySequence();
                    }
                    //if drone greater than 1 health
                    else
                    {
                        CreateHitParticles(); //create hit particles effect
                        EnemyStats.TakeDamage(1, 0, 0); //receive 1 damage
                    }
                }
            }

            //display ui health change
            if (m_CurrentHealthImage != null) DisplayHealthChange();
        }
    }

    #region effects

    //create scraps gameobject that aims player
    private void CreateScraps()
    {
        if (m_Scraps != null)
        {
            //get player's transform from gamemaster or if gamemaster has null reference find player on scene
            var target = GameMaster.Instance.m_Player.transform.GetChild(0).transform;

            //create scrap gameobject on scene and...
            Instantiate(m_Scraps, transform.position, Quaternion.identity) //game master instantiate
                .GetComponent<ScrapObject>().SetTarget(target, EnemyStats.DropScrap); //aim it on player
        }
    }

    //create hit particles effect
    private void CreateHitParticles()
    {
        //if there is hit particles in editor
        if (m_HitParticles != null)
        {
            var hitParticles = Instantiate(m_HitParticles); //create hit particles on scene
            hitParticles.transform.position = transform.position; //place it on enemy's gameobject

            Destroy(hitParticles, 2f); //destroy them after 2 seconds
        }
    }

    #endregion

    #region collisions

    private void OnCollisionEnter2D(Collision2D collision)
    {
        InvokeCollisionByType(collision); //execute collision logic base on the enemy's type
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!m_IsReceiveDamageFromDash) //if enemy is not received damage from dash, but player still near him
        {
            InvokeCollisionByType(collision); //execute collision logic base on the enemy's type
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_IsReceiveDamageFromDash = false; //indicates the player is no longer deal damage with dash
    }

    //determines what collision method need to call
    private void InvokeCollisionByType(Collision2D collision)
    {
        if (m_EnemyType == EnemyType.Drone) //if enemy's type - drone
            OnDroneCollision(collision); //invoke drone collision method
        else //if enemy's type - regular
            OnRegularCollision(collision); //invoke regular enemy collision method
    }

    private void OnRegularCollision(Collision2D collision)
    {
        //if player collide with enemy
        if (collision.transform.CompareTag("Player"))
        {
            var isPlayerDashing = collision.gameObject.GetComponent<Animator>().GetBool("Dash"); //indicates is player dashing

            //if player dashing and he has invincible dash chip
            if (PlayerStats.m_IsInvincibleWhileDashing && isPlayerDashing)
            {
                //player do not receive any damage
                Camera.main.GetComponent<Camera2DFollow>().PlayHitEffect();
            } 
            else
            {
                //player receive damage from enemy
                collision.transform.GetComponent<Player>()?.playerStats.HitPlayer(EnemyStats.DamageAmount);
            }

            //if player can damage enemy while dashing
            if (PlayerStats.m_IsDamageEnemyWhileDashing && isPlayerDashing)
            {
                TakeDamage(null, 1, PlayerStats.DamageAmount / 3); //receive damage in zone 1

                m_IsReceiveDamageFromDash = true; //indicate that enemy receive damage from dash
            }
        }
    }

    private void OnDroneCollision(Collision2D collision)
    {
        //if player in drone's collision and drone is not in destroy sequence
        if (collision.transform.CompareTag("Player") & !m_IsDestroying)
        {
            var isPlayerDashing = collision.gameObject.GetComponent<Animator>().GetBool("Dash");

            //if player dashing and has invincible dash chip
            if (isPlayerDashing && PlayerStats.m_IsInvincibleWhileDashing)
            {
                //player do not receive any damage
                Camera.main.GetComponent<Camera2DFollow>().PlayHitEffect();
            }
            else //player receive damage
            {
                collision.transform.GetComponent<Player>().playerStats.HitPlayer(EnemyStats.DamageAmount);
            }

            //if player can deal damage while dashing
            if (isPlayerDashing && PlayerStats.m_IsDamageEnemyWhileDashing)
            {
                TakeDamage(null, 1); //receive 1 damage
                m_IsReceiveDamageFromDash = true; //indicates that drone receive damage from dash
            }

            //if drone has to explode when collide with player
            if (m_DestroyOnCollision)
            {
                m_IsDestroying = true; //indicates that drone is going to destroy
                StartCoroutine(DestroyDrone()); //destroy drone
            }
        }

        //if drone hit ground and he is in destroy sequence
        if (collision.gameObject.layer == 14 & m_IsDestroying)
        {
            StartCoroutine(DestroyDrone(DeathDetonationTimer)); //start destroy timer with DeathDetonationTimer amount of time
        }
        //drone hit ground but is not destroying
        else if (collision.gameObject.layer == 14)
        {
            Destroy(
                Instantiate(GroundHitParticles, collision.contacts[0].point, Quaternion.identity),
                1f); //create ground hit particles
        }
    }

    #endregion

    #region drone's methods
    //start destroy sequence
    private void DetroySequence()
    {
        m_Animator.SetTrigger("Destroy"); //play destroy animation

        m_IsDestroying = true; //indicates that drone is going to explode
        m_Rigidbody.sharedMaterial = null; //remove bouncy material from rigidbody
        m_Rigidbody.gravityScale = 3f; //add gravity to rigidbody, so drone cann start to fall
        m_Rigidbody.constraints = RigidbodyConstraints2D.None; //remove any constraints (so drone will rotate when player move him with his collider)

        Destroy(GetComponent<TrailRenderer>()); //remove trail from gameobject

        m_DestroyTimer = 3f + Time.time; //set timer after 3 seconds drone will explode no mater he hit ground or not

        OnDroneDestroy?.Invoke(true); //indicates that drone start destroy sequence
    }

    //receive information about all enemies (from layerMask) and deal damage
    private void DamageInArea()
    {
        var targetsInCircle = Physics2D.OverlapCircleAll(transform.position, 2, m_LayerMask); // find all "enemies" in circle range

        foreach (var target in targetsInCircle)
        {
            //if world object in explosion area
            if (target.GetComponent<WorldObjectStats>() != null)
            {
                //deal damage to the world object
                target.GetComponent<WorldObjectStats>().TakeDamage();
            }
            //if player or enemy is in area
            else
            {
                //calculate hit direction
                var hitDirection = (target.transform.position - transform.position).x > 0f ? 1 : -1;

                //if player in range
                if (target.GetComponent<Player>() != null)
                {
                    //get player stats
                    var playerStats = target.GetComponent<Player>().playerStats;

                    target.GetComponent<Player>().m_EnemyHitDirection = hitDirection; //hit direction
                    playerStats.HitPlayer(EnemyStats.DamageAmount); //apply damage to the player
                    playerStats.DebuffPlayer(DebuffPanel.DebuffTypes.Defense, 5f); //apply defense debuf on player (receive double amount of damage when hitted)
                }
                //if enemy is in area and it is not this drone
                else if (target.GetComponent<EnemyStatsGO>() != null && target.gameObject != gameObject)
                {
                    var enemyStats = target.GetComponent<EnemyStatsGO>(); //get hitted enemy stats

                    if (enemyStats.GetComponent<EnemyMovement>() != null) //if enemy has movement script
                        enemyStats.GetComponent<EnemyMovement>().m_Direction = hitDirection; //apply throw back direction

                    enemyStats.TakeDamage(null, 1, 20); //deal damage to the hitted enemy
                }
            }
        }
    }

    //destroy drone
    private IEnumerator DestroyDrone(float waitTimeBeforeDestroy = 0.01f)
    {
        yield return new WaitForSeconds(waitTimeBeforeDestroy); //wait seconds before destroy

        DamageInArea(); //deal damage to all enemies in circle area

        //if drone has attached death particles
        if (EnemyStats.DeathParticle != null)
        {
            //show them on scene
            Destroy(
                Instantiate(EnemyStats.DeathParticle, transform.position, Quaternion.identity), 2f);
            EnemyStats.DeathParticle = null;
        }

        //create scraps gameobject
        CreateScraps();

        //destroy this drone
        //EnemyStats.TakeDamage(1);
        Destroy(m_GameObjectToDestroy);
    }
    #endregion

    private void HideHealthUI()
    {
        if (m_HealthUI.activeSelf) //hide health ui
        {
            m_HealthUI.SetActive(false);
        }
    }

    #region test methods

    [ContextMenu("CreateShieldOnEnemy")]
    public void CreateShield()
    {
        EnemyStats.CreateShield();
    }

    #endregion
}

using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Rigidbody2D))]
public class FallBlock : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField] private float IdleTime = 4f; //block idle time
    [SerializeField] private float FallTime = 2f; //block fall time
    [SerializeField, Range(-40f, 40f)] private float m_YPosition = -10f; //force to move fallblock

    [Header("Effects")]
    [SerializeField] private PlayerInTrigger m_CamShakeArea; //trigger to check is player near fallblock
    [SerializeField] private Audio m_HitAudio; //ground hit audio effect
    [SerializeField] private GameObject m_HitGroundParticles; //ground hit particles effect
    [SerializeField] private Transform m_ParticlePosition; //position where to create particle effect

    #endregion

    private Rigidbody2D m_Rigidbody; //block rigidbody
    private float m_UpdateTime; //change state time
    private bool m_IsIdle; //is block idling

    private bool m_IsPlayerInCamShakeArea; //indicates is player around fall block

    #endregion

    #region private methods

    #region initialize

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>(); //get rigidbody component
        m_CamShakeArea.OnPlayerInTrigger += SetIsPlayerInCamShakeArea; //subscribe to the player in trigger
    }

    #endregion

    //move block by timer
    private void FixedUpdate()
    {
        if (m_UpdateTime <= Time.time) //if need to change state
        {
            m_UpdateTime = Time.time;

            m_IsIdle = !m_IsIdle; //change state

            if (m_IsIdle) //is idle state
            {
                CreateHitGroundEffect(); //play hit ground effect
                m_UpdateTime += IdleTime; //add idle time
            }
            else
            {
                m_UpdateTime += FallTime; //add fall time
            }

            MoveBlock(); //move or stop block (base on state)
        }
    }

    //play hit ground sound, particles and cam shake if player near fallblock
    private void CreateHitGroundEffect()
    {
        //if player near fall block
        if (m_IsPlayerInCamShakeArea)
        {
            //if block is hit ground
            if (m_YPosition > 0 && m_IsIdle)
            {   
                //create particles effect
                var hitGroundEffect = Instantiate(m_HitGroundParticles);
                hitGroundEffect.transform.position = m_ParticlePosition.position;
                Destroy(hitGroundEffect, 1.6f);

                //shake camera
                Camera.main.GetComponent<Camera2DFollow>().Shake(.08f, .08f);

                //play hit ground sound
                AudioManager.Instance.Play(m_HitAudio);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !m_IsIdle) //if player in block's trigger
        {
            var playerStats = collision.GetComponent<Player>().playerStats;

            playerStats.TakeDamage(1, 0, 0); //player take damage
            playerStats.ReturnPlayerOnReturnPoint(); //return player on return point
        }
    }

    //move block up/down base on m_YPosition
    private void MoveBlock()
    {
        var moveVector = Vector2.zero; //stop block's moving

        if (!m_IsIdle) //if block is not idle
        {
            moveVector = new Vector2(0f, m_YPosition); //get move vector
            m_YPosition *= -1; //get next y value
        }

        m_Rigidbody.velocity = moveVector; //move block
    }

    //set field that indicates if player near fallblock
    private void SetIsPlayerInCamShakeArea(bool value, Transform target)
    {
        m_IsPlayerInCamShakeArea = value;
    }

    #endregion
}

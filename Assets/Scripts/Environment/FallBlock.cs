using UnityEngine;
using UnityStandardAssets._2D;

public class FallBlock : MonoBehaviour {

    #region private fields

    #region serialize fields

    [Header("Stats")]
    [SerializeField, Range(0f, 10f)] private float IdleTime = 4f; //block idle time
    [SerializeField, Range(1f, 20f)] private float m_Speed = 2f; //force to move fallblock

    [Header("Points")]
    [SerializeField] private Transform m_Points; //points between which fallblock moves
    [SerializeField, Range(0, 1)] private int m_CurrentIndex = 0; //to what point should fallblock moves first

    [Header("Effects")]
    [SerializeField] private PlayerInTrigger m_CamShakeArea; //trigger to check is player near fallblock
    [SerializeField] private Audio m_HitAudio; //ground hit audio effect
    [SerializeField] private GameObject m_HitGroundParticles; //ground hit particles effect
    [SerializeField] private Transform m_ParticlePosition; //position where to create particle effect

    #endregion
    
    private float m_UpdateTime; //change state time
    private bool m_IsIdle; //is block idling

    private bool m_IsPlayerInCamShakeArea; //indicates is player around fall block
    private float m_SizeFromCenterToEdge = 0f; //size from center to the fall block edge

    #endregion

    #region private methods

    #region initialize

    private void Start()
    {
        m_CamShakeArea.OnPlayerInTrigger += SetIsPlayerInCamShakeArea; //subscribe to the player in trigger
        m_SizeFromCenterToEdge = GetComponent<BoxCollider2D>().size.y; //get size
    }

    #endregion

    //move block by timer
    private void Update()
    {
        if (m_UpdateTime <= Time.time) //if need to change state
        {
            m_IsIdle = false; //change state

            m_SizeFromCenterToEdge *= -1;
        }

        if (!m_IsIdle)
        {
            transform.position = Vector2.MoveTowards(transform.position, m_Points.GetChild(m_CurrentIndex).position, Time.deltaTime * m_Speed);

            if (Vector2.Distance(transform.position, m_Points.GetChild(m_CurrentIndex).position) <= m_SizeFromCenterToEdge)
            {
                m_IsIdle = true;
                m_UpdateTime = IdleTime + Time.time;

                if (m_CurrentIndex == 1)
                    CreateHitGroundEffect();

                GetNextDestination();
            }
        }
    }

    private void GetNextDestination()
    {
        m_CurrentIndex++;

        if (m_CurrentIndex >= m_Points.childCount)
        {
            m_CurrentIndex = 0;
        }
    }

    //play hit ground sound, particles and cam shake if player near fallblock
    private void CreateHitGroundEffect()
    {
        //if player near fall block
        if (m_IsPlayerInCamShakeArea)
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!m_IsIdle) //if fall block is not in idle
        {
            //if player in block's trigger
            if (collision.CompareTag("Player"))
            {
                var playerStats = collision.GetComponent<Player>().playerStats;

                playerStats.TakeDamage(1, 0, 0); //player take damage
                playerStats.ReturnPlayerOnReturnPoint(); //return player on return point
            }
            //or if enemy in block's trigger
            else if (collision.CompareTag("Enemy"))
            {
                collision.GetComponent<EnemyStatsGO>().EnemyStats.KillObject(); //destroy enemy
            }
        }
    }

    //set field that indicates if player near fallblock
    private void SetIsPlayerInCamShakeArea(bool value, Transform target)
    {
        m_IsPlayerInCamShakeArea = value;
    }

    #endregion
}

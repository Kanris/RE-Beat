using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class Barrier : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField, Range(1, 10)] private float IdleTime = 5f; //time when barrier is inactive
    [SerializeField, Range(1, 10)] private float ActiveTime = 3f; //time while barrier is active
    [SerializeField, Range(0, 6)] private int DamageAmount = 1; //barrier damage amount

    #endregion

    private Animator m_Animator; //barrier animator
    private Collider2D m_BarrierCollider; //barrier collider
    private float m_UpdateTime; //check time
    private bool m_IsIdle = true; //is barrier in idle

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    void Start () {

        m_Animator = GetComponent<Animator>(); //reference to the animator
        m_BarrierCollider = GetComponent<Collider2D>(); //reference to the collider

    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        if (m_UpdateTime <= Time.time) //if need to change barrier state
        {
            m_IsIdle = !m_IsIdle; //change current state

            m_UpdateTime = Time.time;

            if (m_IsIdle) //is state is idle
            {
                m_UpdateTime += IdleTime; //add idle time
                EndAnimation(); //play end animation
            }
            else //if active state
            {
                m_UpdateTime += ActiveTime; //add active time
                StartAnimation(); //play start animation
            }

            ChangeColliderState(!m_IsIdle); //hide or enable barrier collider
        }

	}


    private void PlayAnimation(string name)
    {
        m_Animator.SetTrigger(name);
    }

    private void ChangeColliderState(bool state)
    {
        m_BarrierCollider.enabled = state;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player hit barrier collider
        {
            collision.gameObject.GetComponent<Player>().playerStats.HitPlayer(DamageAmount); //damage player
        }
    }

    #endregion

    #region public methods (animations)

    public void StartAnimation()
    {
        PlayAnimation("Start");
    }

    public void ContinueAnimation()
    {
        PlayAnimation("Continue");
    }

    public void EndAnimation()
    {
        PlayAnimation("End");
    }

    public void IdleAnimation()
    {
        PlayAnimation("Idle");
    }

    #endregion
}

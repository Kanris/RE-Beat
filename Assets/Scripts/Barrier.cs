using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class Barrier : MonoBehaviour {

    [SerializeField, Range(1, 10)] private float IdleTime = 5f;
    [SerializeField, Range(1, 10)] private float ActiveTime = 3f;
    [SerializeField, Range(1, 6)] private int DamageAmount = 1;

    private Animator m_Animator;
    private Collider2D m_BarrierCollider;
    private float m_UpdateTime;
    private bool m_IsIdle = true;

    #region Initialize
    // Use this for initialization
    void Start () {

        InitializeAnimator();
        InitializeCollider();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void InitializeCollider()
    {
        m_BarrierCollider = GetComponent<Collider2D>();
    }
    #endregion

    // Update is called once per frame
    void Update () {
		
        if (m_UpdateTime <= Time.time)
        {
            m_IsIdle = !m_IsIdle;

            m_UpdateTime = Time.time;

            if (m_IsIdle)
            {
                m_UpdateTime += IdleTime;
                EndAnimation();
            }
            else
            {
                m_UpdateTime += ActiveTime;
                StartAnimation();
            }

            ChangeColliderState(!m_IsIdle);
        }

	}

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

    private void PlayAnimation(string name)
    {
        m_Animator.SetTrigger(name);
    }

    private void ChangeColliderState(bool state)
    {
        m_BarrierCollider.enabled = state;
    }

    #region 

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);
        }
    }

    #endregion
}

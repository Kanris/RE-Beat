using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(BoxCollider2D))]
public class DisappearPlatform : MonoBehaviour {

    #region enum

    public enum DisappearPlatformType { Trigger, OnTimer } //platform type
    public DisappearPlatformType PlatformType; //current platform type

    #endregion

    #region serialize fields

    [SerializeField, Range(0.5f, 10f)] private float m_DisappearTime; //how long platform is "transparent"
    [SerializeField, Range(0.5f, 10f)] private float m_IdleTime; //how long is platform in transparent or in solid state

    #endregion

    #region private fields

    private Animator m_Animator; //reference to the disappear platform animation
    private Collider2D m_BoxCollider; //platform collider
    private float m_UpdateTime; //loop time
    private bool m_IsIdle = true; //first state is idle
    private bool m_IsDisappear; //is platform disappear
    private bool m_IsPlayerNear; //if player on platform (for trigger platform type)

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_BoxCollider = GetComponent<Collider2D>();
    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        if (PlatformType == DisappearPlatformType.OnTimer | m_IsPlayerNear) //if platform has timer or player is on platform (Trigger platform type)
        {
            if (m_UpdateTime <= Time.time) //platform need to change state
            {
                m_IsIdle = !m_IsIdle; //change state
                m_UpdateTime = Time.time;

                if (m_IsIdle) //if platform need to wait 
                {
                    m_UpdateTime += m_IdleTime; //add idle time
                    IdleAnimation(); //play idle animation
                }
                else //is platform is disappearing
                {
                    m_UpdateTime += m_DisappearTime; //add disappearing time
                    SetAnimator("Disappearing"); //play disappearing animation
                }
            }
        }
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (PlatformType == DisappearPlatformType.Trigger) //if platform type is trigger
        {
            if (collision.transform.CompareTag("Player") & !m_IsPlayerNear) //if player step on the platform
            {
                m_IsPlayerNear = true; //notify that player is on the platform
            }
        }
    }

    private void IdleAnimation()
    {
        m_IsDisappear = !m_IsDisappear; //change state

        if (m_IsDisappear) //if need to Disappear
        {
            SetAnimator("Disappear"); //player Disappear animation
            SetCollider(false); //remove platform collider
            m_IsIdle = false; //change idle state
        }
        else
        {
            SetAnimator("Idle"); //play idle animation
            SetCollider(true); //enable platform collider

            if (m_IsPlayerNear) //if player was on the platform
            {
                m_IsPlayerNear = false;
                m_UpdateTime = Time.time;
            }
        }
    }

    private void SetCollider(bool value)
    {
        m_BoxCollider.enabled = value;
    }

    private void SetAnimator(string name)
    {
        m_Animator.SetTrigger(name);
    }

    #endregion
}

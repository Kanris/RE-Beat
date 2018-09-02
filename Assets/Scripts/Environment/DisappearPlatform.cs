using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(BoxCollider2D))]
public class DisappearPlatform : MonoBehaviour {

    #region enum

    public enum DisappearPlatformType { Trigger, OnTimer }
    public DisappearPlatformType PlatformType;

    #endregion

    #region serialize fields

    [SerializeField, Range(0.5f, 10f)] private float m_DisappearTime;
    [SerializeField, Range(0.5f, 10f)] private float m_IdleTime;

    #endregion

    #region private fields

    private Animator m_Animator;
    private Collider2D m_BoxCollider;
    private float m_UpdateTime;
    private bool m_IsIdle = true;
    private bool m_IsDisappear;
    private bool m_IsPlayerNear;

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        InitializeAnimator();
        InitializeCollider();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void InitializeCollider()
    {
        m_BoxCollider = GetComponent<Collider2D>();
    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        if (PlatformType == DisappearPlatformType.OnTimer | m_IsPlayerNear)
        {
            if (m_UpdateTime <= Time.time)
            {
                m_IsIdle = !m_IsIdle;
                m_UpdateTime = Time.time;

                if (m_IsIdle)
                {
                    m_UpdateTime += m_IdleTime;
                    IdleAnimation();
                }
                else
                {
                    m_UpdateTime += m_DisappearTime;
                    SetAnimator("Disappearing");
                }
            }
        }
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (PlatformType == DisappearPlatformType.Trigger)
        {
            if (collision.transform.CompareTag("Player") & !m_IsPlayerNear)
            {
                m_IsPlayerNear = true;
            }
        }
    }

    private void IdleAnimation()
    {
        m_IsDisappear = !m_IsDisappear;

        if (m_IsDisappear)
        {
            SetAnimator("Disappear");
            SetCollider(false);
            m_IsIdle = false;
        }
        else
        {
            SetAnimator("Idle");
            SetCollider(true);

            if (m_IsPlayerNear)
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DisappearPlatform : MonoBehaviour {

    #region enum

    public enum DisappearPlatformType { Trigger, OnTimer } //platform type
    public DisappearPlatformType PlatformType; //current platform type

    #endregion

    #region serialize fields

    [SerializeField, Range(0.5f, 10f)] private float m_DisappearTime; //how long platform is "transparent"
    [SerializeField, Range(0.5f, 10f)] private float m_IdleTime; //how long is platform in transparent or in solid state

    [Header("Next platform")]
    [SerializeField] private GameObject m_NextPlatform;
    [SerializeField] private bool m_IsNextPlatform;

    [Header("Activate")]
    [SerializeField] private bool m_IsActivateByCompanion = false;

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

    private void Awake()
    {
        //get components on this gameobject
        m_Animator = GetComponent<Animator>();
        m_BoxCollider = GetComponent<Collider2D>();

        //if current platform must be actviate by another platform (hide this platform)
        if (m_IsNextPlatform)
            gameObject.SetActive(false);

        //if platform has to be active by companion
        if (m_IsActivateByCompanion)
        {
            IdleAnimation(); //"hide" platform (remove collider and change alpha)
        }
    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        if (!m_IsActivateByCompanion) //is companion do not have to activate platform
        {
            if ((PlatformType == DisappearPlatformType.OnTimer || m_IsPlayerNear) && !m_IsNextPlatform) //if platform has timer or player is on platform (Trigger platform type)
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
	}

    private void OnEnable()
    {
        if (m_IsNextPlatform)
        {
            StartCoroutine(DisableAfterTime());

            SetAnimator("Disappearing"); //play Disappear animation
        }
    }

    private IEnumerator DisableAfterTime(float time = 2f)
    {
        yield return new WaitForSeconds(time);

        m_IsPlayerNear = false;
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (PlatformType == DisappearPlatformType.Trigger) //if platform type is trigger
        {
            if (collision.transform.CompareTag("Player") & !m_IsPlayerNear) //if player step on the platform
            {
                m_IsPlayerNear = true; //notify that player is on the platform

                if (m_NextPlatform != null)     
                    m_NextPlatform.SetActive(true);
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
        m_Animator?.SetTrigger(name);
    }

    #endregion

    public void SetActivateByCompanion(bool value)
    {
        m_IsActivateByCompanion = value;

        if (!value)
        {
            SetAnimator("Idle"); //play idle animation
            SetCollider(true); //enable platform collider
        }
    }
}

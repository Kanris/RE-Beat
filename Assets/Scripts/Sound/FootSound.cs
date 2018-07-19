using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSound : MonoBehaviour {

    public string Sound;

    private float updateTime = 0.2f;
    private float currentUpdateTime = 0.2f;
    private Animator m_Animator;

    private void Start()
    {
        InitializeAnimator();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("FootSound: Can't find Animator on GameObject");
        }
    }

    private void Update()
    {
        if (currentUpdateTime <= Time.time)
        {
            currentUpdateTime = updateTime + Time.time;

            ManageFootstepsSound();
        }
    }

    private void ManageFootstepsSound()
    {
        var onGround = m_Animator.GetBool("Ground");

        if (onGround)
        {
            var isMoving = m_Animator.GetFloat("Speed") > 0.01f;

            if (isMoving)
            {
                AudioManager.Instance.Play(Sound);
            }
            else
            {
                AudioManager.Instance.Stop(Sound);
            }
        }
        else
        {
            AudioManager.Instance.Stop(Sound);
        }
    }

}

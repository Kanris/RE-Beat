using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSound : MonoBehaviour {

    public string Sound;
    private Animator m_Animator;

    #region Initialize

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
    #endregion

    private void Update()
    {
        if (m_Animator.GetBool("Ground"))
        {
            if (m_Animator.GetFloat("Speed") > 0.01f)
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

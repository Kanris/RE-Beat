using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootSound : MonoBehaviour {

    #region fields

    public string Sound;

    private Animator m_Animator;

    #endregion

    #region Initialize

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    #endregion

    #region private methods

    private void Update()
    {
        if (m_Animator.GetBool("Ground")) //player is on the ground
        {
            if (m_Animator.GetFloat("Speed") > 0.01f) //play walk sound
            {
                AudioManager.Instance.Play(Sound);
            }
            else //player is not moving - stop playing
            {
                AudioManager.Instance.Stop(Sound);
            }
        }
        else //player is not on the ground
        {
            AudioManager.Instance.Stop(Sound);
        }
    }

    #endregion
}

using UnityEngine;

public class MistFader : MonoBehaviour {

    #region serialize fields

    [SerializeField] private DisableBackground m_DisableBackground;

    #endregion

    #region public methods

    public void AnimationComplete()
    {
        if (m_DisableBackground != null)
        {
            m_DisableBackground.AnimationComplete();
        }
        else
        {
            Debug.LogError("MistFader.AnimationComplete: m_DisableBackground is not initialized");
        }
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistFader : MonoBehaviour {

    [SerializeField] private DisableBackground m_DisableBackground;

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


}

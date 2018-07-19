using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFader : MonoBehaviour {

    private void AnimationComplete()
    {
        ScreenFaderManager.Instance.AnimationComplete();
    }
}

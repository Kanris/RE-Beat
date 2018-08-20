using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour {

    [SerializeField] private Light LightToControl;

    public void ChangeColorIntensity(int value)
    {
        if (LightToControl != null)
        {
            LightToControl.intensity = value;
        }
    }

}

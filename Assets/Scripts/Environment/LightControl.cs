using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour {

    [SerializeField] private Light LightToControl;
    [SerializeField] private bool ControlWithoutAnimator = false;
    [SerializeField, Range(1, 20)] private float BlinkAfter = 3f;
    [SerializeField, Range(1, 50)] private float IntensivityIteration = 20f;
    [SerializeField, Range(1, 10)] private int Iterations = 4;

    private float m_UpdateTimer = 0f;

    private void Start()
    {
        if (ControlWithoutAnimator)
        {
            m_UpdateTimer = BlinkAfter;
            LightToControl.intensity = 0f;
        }
    }

    public void ChangeColorIntensity(int value)
    {
        if (LightToControl != null)
        {
            LightToControl.intensity = value;
        }
    }

    private void Update()
    {
        if (ControlWithoutAnimator)
        {
            if (m_UpdateTimer <= Time.time)
            {
                m_UpdateTimer = Time.time + BlinkAfter;

                LightToControl.gameObject.SetActive(true);
                StartCoroutine(LightBlink());
            }
        }
    }

    private IEnumerator LightBlink(int multiplier = 1)
    {
        for (int index = 0; index < Iterations; index++)
        {
            LightToControl.intensity += IntensivityIteration * multiplier;
            yield return new WaitForSeconds(0.2f);
        }

        if (multiplier > 0)
            StartCoroutine(LightBlink(-1));
    }

}

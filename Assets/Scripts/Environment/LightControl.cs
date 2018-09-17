using System.Collections;
using UnityEngine;

public class LightControl : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField] private Light LightToControl; //light to change it's "volume"
    [SerializeField] private bool ControlWithoutAnimator = false; //is light can be controlled without animation
    [SerializeField, Range(1, 20)] private float BlinkAfter = 3f; //update time
    [SerializeField, Range(1, 50)] private float IntensivityIteration = 20f; //itensivity each iteration
    [SerializeField, Range(1, 10)] private int Iterations = 4; //change light value iterations

    #endregion

    private float m_UpdateTimer = 0f; //time checker

    #endregion

    #region private methods

    private void Start()
    {
        if (ControlWithoutAnimator) //light have to be controled without animation
        {
            m_UpdateTimer = BlinkAfter;
            LightToControl.intensity = 0f; //change default intensity
            LightToControl.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (ControlWithoutAnimator) //if light controlled without animation
        {
            if (m_UpdateTimer <= Time.time) //need to change state
            {
                m_UpdateTimer = Time.time + BlinkAfter; //set up next check time

                StopAllCoroutines();
                StartCoroutine(LightBlink()); //blink animation
            }
        }
    }

    private IEnumerator LightBlink(int multiplier = 1)
    {
        //change light intensity
        for (int index = 0; index < Iterations; index++)
        {
            LightToControl.intensity += IntensivityIteration * multiplier;
            yield return new WaitForSeconds(0.2f);
        }

        //if need to change light intensity back
        if (multiplier > 0)
            StartCoroutine(LightBlink(-1));
    }

    #endregion

    #region public methods

    //change light intensity from animation
    public void ChangeColorIntensity(float value)
    {
        if (LightToControl != null)
        {
            LightToControl.intensity = value;
        }
    }

    #endregion
}

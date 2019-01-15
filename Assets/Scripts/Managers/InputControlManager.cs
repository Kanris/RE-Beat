using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using InControl;

public class InputControlManager : MonoBehaviour {

    [HideInInspector] public InputDevice m_Joystick;
    private float m_VibrateTimer;
    private bool m_IsVibrate;

    private GameObject lastselect;

    #region singleton

    public static InputControlManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    #endregion

    private void Start()
    {
        m_Joystick = InputManager.ActiveDevice;
    }

    // Update is called once per frame
    private void Update()
    {
        m_Joystick = InputManager.ActiveDevice;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(lastselect);
        }
        else
        {
            lastselect = EventSystem.current.currentSelectedGameObject;
        }
    }

    public static bool IsCanUseSubmitButton()
    {
        if (!PauseMenuManager.IsPauseOpen)
        {
            if (!InfoManager.IsJournalOpen)
            {
                return true;
            }
        }

        return false;
    }

    #region joystick
    public void StartJoystickVibrate(float intensity, float time)
    {
        if (m_IsVibrate)
        {
            StopJoystickVibrate();
        }

        m_Joystick.Vibrate(intensity);
        m_IsVibrate = true;

        StartCoroutine(GamepadVibrate(time));
    }

    private IEnumerator GamepadVibrate(float time)
    {
        var currentTimeVibration = 0f;

        while (currentTimeVibration < time)
        {
            currentTimeVibration += 0.01f;

            yield return new WaitForSecondsRealtime(0.01f);
        }

        if (m_IsVibrate)
        {
            StopJoystickVibrate();
        }
    }

    private void StopJoystickVibrate()
    {
        m_IsVibrate = false;
        m_Joystick.Vibrate(0);
    }

    public static bool IsUpperButtonsPressed()
    {
        var leftStickValue = Instance.m_Joystick.LeftStickY.Value > 0 ?
            Mathf.Abs(Instance.m_Joystick.LeftStickY.Value - 1f) < 0.01f : false;

        return leftStickValue || Instance.m_Joystick.DPadUp.WasPressed;
    }

    public static bool IsAttackButtonsPressed()
    {
        return Instance.m_Joystick.RightBumper.WasPressed || Instance.m_Joystick.Action3.WasPressed;
    }

    #endregion

}

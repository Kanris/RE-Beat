using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using InControl;

public class InputControlManager : MonoBehaviour {

    private InputDevice m_Joystick;
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

    #region Gamepad

    #region movement

    public float GetHorizontalValue()
    {
        var horizontalValue = Mathf.Abs(m_Joystick.LeftStickX) > .3f
                                    ? m_Joystick.LeftStickX : 0f;

        //if left stick isn't pressed try to get value from dpad
        if (horizontalValue == 0)
            horizontalValue = m_Joystick.DPadX;

        return horizontalValue;
    }

    public float GetVerticalValue()
    {
        var verticalValue = m_Joystick.LeftStickY.Value;

        if (verticalValue == 0)
            verticalValue = Instance.m_Joystick.DPadDown;

        return verticalValue;
    }

    public bool IsJumpPressed()
    {
        return m_Joystick.Action1.WasPressed;
    }

    public bool IsDashPressed()
    {
        return m_Joystick.RightTrigger.WasPressed;
    }

    #endregion

    #region attack

    public bool IsAttackPressed()
    {
        return m_Joystick.Action3.WasPressed;
    }

    public bool IsShootPressed()
    {
        return m_Joystick.RightBumper.WasPressed;
    }

    public bool IsFallAttackPressed()
    {
        return m_Joystick.LeftBumper.WasPressed;
    }

    #endregion

    #region actions

    public bool IsPickupReleased()
    {
        return m_Joystick.Action4.WasReleased;
    }

    public bool IsPickupPressing()
    {
        return m_Joystick.Action4.IsPressed;
    }

    public bool IsPickupPressed()
    {
        return m_Joystick.Action4.WasPressed;
    }

    public bool IsSubmitPressed()
    {
        return m_Joystick.Action4.WasPressed;
    }

    //b button on gamepad
    public bool IsBackPressed()
    {
        return m_Joystick.Action2.WasPressed;
    }

    public bool IsBackMenuPressed()
    {
        return m_Joystick.GetControl(InputControlType.Back).WasPressed;
    }

    public bool IsStartMenuPressed()
    {
        return m_Joystick.GetControl(InputControlType.Start).WasPressed;
    }

    public bool IsLeftBumperPressed()
    {
        return m_Joystick.LeftBumper.WasPressed;
    }

    public bool IsRightBumperPressed()
    {
        return m_Joystick.RightBumper.WasPressed;
    }

    #endregion

    #region right stick

    public bool IsRightStickPressed()
    {
        return m_Joystick.RightStick.State;
    }

    public float GetHorizontalRightStickValue()
    {
        return m_Joystick.RightStickX.Value;
    }

    public float GetVerticalRightStickValue()
    {
        return m_Joystick.RightStickY.Value;
    }

    #endregion


    public static bool IsUpperButtonsPressed()
    {
        var leftStickValue = Instance.m_Joystick.LeftStickY.Value > 0 ?
            Mathf.Abs(Instance.m_Joystick.LeftStickY.Value - 1f) < 0.01f : false;

        return (leftStickValue & Instance.m_Joystick.LeftStickY.WasPressed) || Instance.m_Joystick.DPadUp.WasPressed;
    }

    public static bool IsAttackButtonsPressed()
    {
        return Instance.m_Joystick.RightBumper.WasPressed || Instance.m_Joystick.Action3.WasPressed;
    }

    #region vibration

    public void StartGamepadVibration(float intensity, float time)
    {
        if (m_IsVibrate)
        {
            StopGamepadVibration();
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
            StopGamepadVibration();
        }
    }

    private void StopGamepadVibration()
    {
        m_IsVibrate = false;
        m_Joystick.Vibrate(0);
    }

    #endregion

    #endregion

}

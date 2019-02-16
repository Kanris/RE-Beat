using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using InControl;

public class InputControlManager : MonoBehaviour {

    private InputDevice m_Gamepad; //current active input

    private float m_RumbleTime; //how long to rumble
    private bool m_IsRumble; //is gamepad rumbling

    private GameObject m_LastSelectedEventItem; //last item selected (if eventsystem loosing ui focus)

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
        m_Gamepad = InputManager.ActiveDevice; //get current active input
    }

    // Update is called once per frame
    private void Update()
    {
        m_Gamepad = InputManager.ActiveDevice; //get current active input (if player start using keyboard or gamepad)

        //set previous item for eventsystem is it's empty
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(m_LastSelectedEventItem);
        }
        else //get current selected item in event system
        {
            m_LastSelectedEventItem = EventSystem.current.currentSelectedGameObject;
        }
    }

    //indicates if player can use submit button
    public bool IsCanUseSubmitButton()
    {
        //if pause menu is not open
        if (!PauseMenuManager.IsPauseOpen)
        {
            //if journal is not open
            if (!InfoManager.IsJournalOpen)
            {
                return true;
            }
        }

        return false;
    }

    #region Gamepad

    #region movement

    //get horizontal value from stick or dpad
    public float GetHorizontalValue()
    {
        //if left stick isn't pressed try to get value from dpad
        var horizontalValue = Mathf.Abs(m_Gamepad.LeftStickX.Value) > .3f
                                    ? m_Gamepad.LeftStickX.Value : m_Gamepad.DPadX;

        return horizontalValue;
    }

    //get vertical value from stick or dpad
    public float GetVerticalValue()
    {
        var verticalValue = Mathf.Abs(m_Gamepad.LeftStickY.Value) > .3f
                                   ? m_Gamepad.LeftStickY.Value : Instance.m_Gamepad.DPadDown;
            
        return verticalValue;
    }

    //is jump button pressed
    public bool IsJumpPressed()
    {
        return m_Gamepad.Action1.WasPressed;
    }

    //is dash button pressed
    public bool IsDashPressed()
    {
        return m_Gamepad.RightBumper.WasPressed;
    }

    #endregion

    #region attack

    //is main attack button pressed
    public bool IsAttackPressed()
    {
        return m_Gamepad.Action3.WasPressed;
    }

    //is shoot button pressed
    public bool IsShootPressed()
    {
        return m_Gamepad.RightTrigger.WasPressed;
    }

    //is fall attack button pressed
    public bool IsFallAttackPressed()
    {
        return m_Gamepad.LeftBumper.WasPressed;
    }

    #endregion

    #region actions

    //pick up item (box, magnetic box) button detection 
    public bool IsPickupPressed()
    {
        return m_Gamepad.Action2.WasPressed;
    }

    public bool IsSubmitReleased()
    {
        return m_Gamepad.Action1.WasReleased;
    }

    public bool IsSubmitPressing()
    {
        return m_Gamepad.Action1.IsPressed;
    }


    public bool IsSubmitPressed()
    {
        return m_Gamepad.Action1.WasPressed;
    }

    //b button on gamepad
    public bool IsBackPressed()
    {
        return m_Gamepad.Action2.WasPressed;
    }

    public bool IsBackMenuPressed()
    {
        return m_Gamepad.GetControl(InputControlType.Back).WasPressed;
    }

    public bool IsStartMenuPressed()
    {
        return m_Gamepad.GetControl(InputControlType.Start).WasPressed;
    }

    public bool IsLeftBumperPressed()
    {
        return m_Gamepad.LeftBumper.WasPressed;
    }

    public bool IsRightBumperPressed()
    {
        return m_Gamepad.RightBumper.WasPressed;
    }

    #endregion

    #region right stick

    public bool IsRightStickPressed()
    {
        return m_Gamepad.RightStick.State;
    }

    public float GetHorizontalRightStickValue()
    {
        return m_Gamepad.RightStickX.Value;
    }

    public float GetVerticalRightStickValue()
    {
        return m_Gamepad.RightStickY.Value;
    }

    #endregion

    //calculate is up button pressed or not
    public bool IsUpperButtonsPressed()
    {
        var leftStickValue = Instance.m_Gamepad.LeftStickY.Value > 0 && Mathf.Abs(Instance.m_Gamepad.LeftStickY.Value - 1f) < 0.01f;

        return (leftStickValue && Instance.m_Gamepad.LeftStickY.WasPressed) || Instance.m_Gamepad.DPadUp.WasPressed;
    }

    //if player shooting or attacking
    public bool IsAttackButtonsPressed()
    {
        return Instance.IsAttackPressed() || Instance.IsShootPressed();
    }

    #region vibration

    public void StartGamepadVibration(float intensity, float time)
    {
        /*
        if (m_IsRumble)
        {
            StopGamepadVibration();
        }

        m_Gamepad.Vibrate(intensity);
        m_IsRumble = true;

        StartCoroutine(GamepadVibrate(time));*/
    }

    private IEnumerator GamepadVibrate(float time)
    {
        var currentTimeVibration = 0f;

        while (currentTimeVibration < time)
        {
            currentTimeVibration += 0.01f;

            yield return new WaitForSecondsRealtime(0.01f);
        }

        if (m_IsRumble)
        {
            StopGamepadVibration();
        }
    }

    private void StopGamepadVibration()
    {
        m_IsRumble = false;
        m_Gamepad.Vibrate(0);
    }

    #endregion

    #endregion

}

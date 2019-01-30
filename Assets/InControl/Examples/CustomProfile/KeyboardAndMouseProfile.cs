using System;
using System.Collections;
using UnityEngine;
using InControl;


namespace CustomProfileExample
{
	// This custom profile is enabled by adding it to the Custom Profiles list
	// on the InControlManager component, or you can attach it yourself like so:
	// InputManager.AttachDevice( new UnityInputDevice( "KeyboardAndMouseProfile" ) );
	// 
	public class KeyboardAndMouseProfile : UnityInputDeviceProfile
	{
		public KeyboardAndMouseProfile()
		{
			Name = "Keyboard/Mouse";
			Meta = "A keyboard and mouse combination profile appropriate for FPS.";

			// This profile only works on desktops.
			SupportedPlatforms = new[]
			{
				"Windows",
				"Mac",
				"Linux"
			};

			Sensitivity = 1.0f;
			LowerDeadZone = 0.0f;
			UpperDeadZone = 1.0f;

			ButtonMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "Attack - Keyboard",
					Target = InputControlType.Action3,
					// KeyCodeButton fires when any of the provided KeyCode params are down.
					Source = MouseButton0
				},
				new InputControlMapping
				{
					Handle = "Shoot - Keyboard",
					Target = InputControlType.RightTrigger,
					Source = MouseButton1
				},
                new InputControlMapping
				{
					Handle = "Dash",
					Target = InputControlType.RightBumper,
					Source = KeyCodeButton(KeyCode.LeftShift)
				},
                new InputControlMapping
                {
                    Handle ="Jump down attack",
                    Target = InputControlType.LeftBumper,
                    Source = KeyCodeButton(KeyCode.Q)

                },
                new InputControlMapping
				{
					Handle = "Jump",
					Target = InputControlType.Action1,
					Source = KeyCodeButton( KeyCode.Space )
				},
                new InputControlMapping
				{
					Handle = "Left Journal",
					Target = InputControlType.LeftBumper,
					// KeyCodeComboButton requires that all KeyCode params are down simultaneously.
					Source = KeyCodeButton( KeyCode.Q)
				},
                new InputControlMapping
                {
                    Handle = "Right Journal",
                    Target = InputControlType.RightBumper,
					// KeyCodeComboButton requires that all KeyCode params are down simultaneously.
					Source = KeyCodeButton( KeyCode.E )
                },
                new InputControlMapping
                {
                    Handle = "Open/Close Journal",
                    Target = InputControlType.Back,
					// KeyCodeComboButton requires that all KeyCode params are down simultaneously.
					Source = KeyCodeButton( KeyCode.Tab )
                },
                new InputControlMapping
                {
                    Handle = "Pause",
                    Target = InputControlType.Start,
					// KeyCodeComboButton requires that all KeyCode params are down simultaneously.
					Source = KeyCodeButton( KeyCode.Escape )
                },
                new InputControlMapping
                {
                    Handle = "Back",
                    Target = InputControlType.Action2,
					// KeyCodeComboButton requires that all KeyCode params are down simultaneously.
					Source = KeyCodeButton( KeyCode.Escape )
                },
                new InputControlMapping
                {
                    Handle = "Submit",
                    Target = InputControlType.Action2,
					// KeyCodeComboButton requires that all KeyCode params are down simultaneously.
					Source = KeyCodeButton( KeyCode.F )
                },
            };

			AnalogMappings = new[]
			{
				new InputControlMapping {
					Handle = "Move X Alternate",
					Target = InputControlType.LeftStickX,
					Source = KeyCodeAxis( KeyCode.A, KeyCode.D )
				},
				new InputControlMapping {
					Handle = "Move Y Alternate",
					Target = InputControlType.LeftStickY,
					Source = KeyCodeAxis( KeyCode.S, KeyCode.W )
				}
			};
		}
	}
}


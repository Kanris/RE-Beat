using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        private PlatformerCharacter2D m_Character;
        private bool m_Jump;
        private bool m_Dash;
        private float m_UpdateDashTime;

        public bool IsCanJump;

        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();

            IsCanJump = true;

            m_UpdateDashTime = 0f;
        }

        private void Update()
        {
            if (IsCanJump)
            {
                if (!m_Jump)
                {
                    // Read the jump input in Update so button presses aren't missed.
                    m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                }
            }

            if (!m_Dash & m_UpdateDashTime < Time.time)
            {
                m_Dash = CrossPlatformInputManager.GetButtonDown("Shift"); //TODO: replace with CrossPlatformInput

                if (m_Dash) m_UpdateDashTime = Time.time + 1f;
            }
        }

        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = Input.GetKey(KeyCode.LeftControl);
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            // Pass all parameters to the character control script.
            m_Character.Move(h, crouch, m_Jump, m_Dash);

            m_Jump = false;
            m_Dash = false;
        }
    }
}

using System;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        private PlatformerCharacter2D m_Character;
        private Animator m_CharacterAnimator;
        private bool m_Jump;
        private bool m_Dash;
        private float m_UpdateDashTime;

        private float m_VibrateTimer;
        private bool m_WasJump;    

        //public bool IsCanMove;
        public bool IsCanJump;

        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();
            m_CharacterAnimator = GetComponent<Animator>();

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
                    m_Jump = InputControlManager.Instance.IsJumpPressed();
                }
            }

            if (!m_Dash & m_UpdateDashTime < Time.time)
            {
                m_Dash = InputControlManager.Instance.IsDashPressed();

                if (m_Dash) m_UpdateDashTime = Time.time + .5f;
            }
        }

        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = false;

            //get left stick movement value
            float h = InputControlManager.Instance.GetHorizontalValue();

            // Pass all parameters to the character control script.
            if (m_Character.enabled) m_Character.Move(h, crouch, m_Jump, m_Dash);

            m_Jump = false;
            m_Dash = false;
        }
    }
}

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
       
        public bool IsCanJump;

        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();

            IsCanJump = true;
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
        }

        private void SecondJump()
        {
            m_Character.m_Grounded = false;
            m_Character.m_Anim.SetBool("Ground", false);
            m_Character.m_Rigidbody2D.AddForce(new Vector2(0f, m_Character.m_JumpForce));
        }

        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = Input.GetKey(KeyCode.LeftControl);
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            // Pass all parameters to the character control script.
            m_Character.Move(h, crouch, m_Jump);
            m_Jump = false;
        }
    }
}

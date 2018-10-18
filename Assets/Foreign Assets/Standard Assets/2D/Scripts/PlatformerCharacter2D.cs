using System;
using System.Collections;
using UnityEngine;
using Unity;

namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        public delegate void VoidDelegate();
        public event VoidDelegate OnLandEvent;

        public float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character
        [SerializeField] private GameObject JumpPlatformPrefab;

        public Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        public Transform m_CeilingCheck;   // A position marking where to check for ceilings
        public float m_JumpForce = 400f;   // Amount of force added when the player jumps.

        const float k_GroundedRadius = .05f; // Radius of the overlap circle to determine if grounded
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up

        private Rigidbody2D m_Rigidbody2D;
        private bool m_Grounded;            // Whether or not the player is grounded.
        private Animator m_Anim;            // Reference to the player's animator component.
        private bool m_IsHaveDoubleJump;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.
        private bool m_IsDashing;                 // A mask determining what is ground to the character
        private GameObject m_JumpPlatform;

        private void Awake()
        {
            // Setting up references.
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();

            InitializeJumpPlatform();

            OnLandEvent += ShowDustEffect;
            InfoManager.Instance.OnJournalOpen += OnInfoManagerOpen;
        }

        private void InitializeJumpPlatform()
        {
            m_JumpPlatform = Instantiate(JumpPlatformPrefab);
            m_JumpPlatform.SetActive(false);
        }

        private void OnDestroy()
        {
            if (m_JumpPlatform != null)
                Destroy(m_JumpPlatform.gameObject);
        }

        private void FixedUpdate()
        {
            m_Grounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                    m_Grounded = true;
            }
            m_Anim.SetBool("Ground", m_Grounded);

            // Set the vertical animation
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, Mathf.Clamp(m_Rigidbody2D.velocity.y, -10, 15));
            m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);
        }

        public void Move(float move, bool crouch, bool jump, bool dash)
        {
            // If crouching, check to see if the character can stand up
            if (!crouch && m_Anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
                {
                    crouch = true;
                }
            }

            // Set whether or not the character is crouching in the animator
            m_Anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if (m_Grounded || m_AirControl)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move*m_CrouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                m_Anim.SetFloat("Speed", Mathf.Abs(move));

                // Move the character
                m_Rigidbody2D.velocity = new Vector2(move*m_MaxSpeed, m_Rigidbody2D.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
                    // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
            }
            // If the player should jump...
            if (m_Grounded && jump && m_Anim.GetBool("Ground"))
            {
                // Add a vertical force to the player.
                m_Grounded = false;
                m_Anim.SetBool("Ground", false);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));

                ShowDustEffect();
                
                m_IsHaveDoubleJump = true;
            }
            else if (!m_Anim.GetBool("Ground") & m_IsHaveDoubleJump && jump)
            {
                m_Rigidbody2D.velocity = Vector2.zero;
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce - 100f));

                m_IsHaveDoubleJump = false;
                m_Anim.Play("Jump", -1, 0f);

                StartCoroutine(DoubleJumpPlatform());
            }

            if (dash && m_Anim.GetFloat("Speed") > 0.01f)
            {
                m_Anim.SetBool("Dash", true);

                m_IsDashing = true;

                ShowDashEffect();

                StartCoroutine(StopDash());
            }

            if (m_IsDashing)
            {
                var multiplier = 1;

                if (!m_FacingRight)
                    multiplier = -1;

                m_Rigidbody2D.AddForce(new Vector2(10f * multiplier, 0f), ForceMode2D.Impulse);
            }
        }

        private IEnumerator DoubleJumpPlatform()
        {
            m_JumpPlatform.transform.position = m_GroundCheck.transform.position;
            m_JumpPlatform.SetActive(true);
            yield return new WaitForSeconds(0.8f);
            m_JumpPlatform.SetActive(false);
        }

        private void Landed()
        {
            if (OnLandEvent != null)
                OnLandEvent();
        }

        private void ShowDustEffect()
        {
            var dustEffect = Resources.Load("Effects/Dust") as GameObject;
            var dustGO = Instantiate(dustEffect);
            dustGO.transform.position = m_GroundCheck.position;

            Destroy(dustGO, 1.5f);
        }

        private void ShowDashEffect()
        {
            var multiplier = -1;

            if (!m_FacingRight)
                multiplier = 1;

            var dashEffect = Resources.Load("Effects/DashEffect") as GameObject;
            var instantiateDashEffect = Instantiate(dashEffect);

            instantiateDashEffect.transform.position = transform.position;
            instantiateDashEffect.transform.rotation = 
                instantiateDashEffect.transform.rotation * Quaternion.Euler(0, 90 * multiplier, 0);

            Destroy(instantiateDashEffect, 1f);
        }

        private IEnumerator StopDash()
        {
            yield return new WaitForSeconds(0.1f);

            m_Anim.SetBool("Dash", false);

            m_IsDashing = false;
        }

        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        private void OnInfoManagerOpen(bool value)
        {
            if (value)
                m_MaxSpeed = 3f;
            else
                m_MaxSpeed = 4f;
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using Unity;
using System.Linq;

namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        public delegate void VoidDelegate();
        public event VoidDelegate OnLandEvent;

        [Header("Support gameobjects")]
        [SerializeField] private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        [SerializeField] private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        [SerializeField] private Transform m_FrontWallCheck; // A position marking where to check for wall in front of wall
        [SerializeField] private Transform m_BackWallCheck;  // A position marking where to check for wall behind the player

        [Header("Movement stats")]
        public float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField] private float m_JumpForce = 400f;   // Amount of force added when the player jumps.
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character    

        [Header("Visual Effects")]
        [SerializeField] private GameObject JumpPlatformPrefab;
        [SerializeField] private GameObject m_LandEffect;
        [SerializeField] private GameObject m_WallDustEffect;
        [SerializeField] private GameObject m_DashEffect;

        [Header("Audio")]
        [SerializeField] private Audio m_LandAudio;
        [SerializeField] private Audio m_DashAudio;
        [SerializeField] private Audio m_DoubleJumpAudio;

        [HideInInspector] public bool m_IsHaveDoubleJump;

        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up

        private Rigidbody2D m_Rigidbody2D;
        private bool m_Grounded;            // Whether or not the player is grounded.
        private Animator m_Anim;            // Reference to the player's animator component.
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.
        private GameObject m_JumpPlatform;
        private bool m_IsOnJumpBox; //indicates is player on jump box and can perform jump
        private bool m_IsOnWall; //indicates is player jump near wall
        private int m_IsWallBehind; //indicates is wall behind the player (0 is haven't use jump from back; 1 - jump from back can be used; 2 - is waiting to refresh (can't use jump from wall))
        private Vector2 m_PositionForWallDust; //position to spawn dust
        
        private void Awake()
        {
            // Setting up references.
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();

            InitializeJumpPlatform();
        }

        private void Start()
        {
            OnLandEvent += ShowDustEffect;
        }

        private void InitializeJumpPlatform()
        {
            if (JumpPlatformPrefab != null)
            {
                m_JumpPlatform = Instantiate(JumpPlatformPrefab);
                m_JumpPlatform.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (m_JumpPlatform != null)
                Destroy(m_JumpPlatform.gameObject);
        }

        private void OnEnable()
        {
            m_Rigidbody2D.gravityScale = 3f; //if player while dashed hit "killing ground" return gravity to default value
            transform.parent.SetParent(null); //if player was on platform
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(m_GroundCheck.position, new Vector3(.3f, .1f, 1)); //x = .3f to disable wall jump
            Gizmos.DrawWireCube(m_FrontWallCheck.position, new Vector3(.1f, .5f, 1));
            Gizmos.DrawWireCube(m_BackWallCheck.position, new Vector3(.1f, .5f, 1));
        }

        private void FixedUpdate()
        {
            m_Grounded = false;
            m_IsOnWall = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            //Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            var colliders = Physics2D.OverlapBoxAll(m_GroundCheck.position, new Vector2(.3f, .1f), 0, m_WhatIsGround)
                                     .Where(x => x != gameObject)
                                     .Count();

            if (colliders > 0)
            {
                m_Grounded = true;
                m_IsHaveDoubleJump = true;
            }

            m_Anim.SetBool("Ground", m_Grounded); //play ground animation

            if (!m_Grounded && !m_IsOnJumpBox)
            {
                var m_WallColliders = Physics2D.OverlapBoxAll(m_FrontWallCheck.position, new Vector2(.1f, .5f), 0, m_WhatIsGround)
                                               .Where(x => x != gameObject)
                                               .Count();

                if (m_WallColliders > 0)
                {
                    m_IsOnWall = true;
                    m_IsWallBehind = 0;
                }
                else if (m_IsWallBehind == 0)
                {
                    var m_WallBackColliders = Physics2D.OverlapBoxAll(m_BackWallCheck.position, new Vector2(.1f, .5f), 0, m_WhatIsGround)
                                                       .Where(x => x != gameObject)
                                                       .Count();

                    if (m_WallBackColliders > 0)
                    {
                        m_IsWallBehind = 1;
                    }
                }
            }
            //else if (m_Grounded && m_IsWallBehind)
            //    m_IsWallBehind = false;

            // Set the vertical animation
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, Mathf.Clamp(m_Rigidbody2D.velocity.y, -30, 15));
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
                m_Rigidbody2D.velocity = new Vector2(move * m_MaxSpeed, m_Rigidbody2D.velocity.y);

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

            if (!m_Anim.GetBool("FallAttack")) //player is not performe fall attack
            {
                // If the player should jump...
                if ((m_Grounded || m_IsOnWall || m_IsWallBehind == 1) && jump)
                {
                    ShowDustEffect();
                    InputControlManager.Instance.StartGamepadVibration(1, 0.1f);

                    //check is user used jump when wall was behind him
                    if (m_IsWallBehind == 1)
                        m_IsWallBehind = 2; //do not allow to use jump from wall (when it's behind) 

                    // Add a vertical force to the player.
                    m_Grounded = false;
                    m_Anim.SetBool("Ground", false);
                    m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
                    m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                }
                //If the player should double jump
                else if (PlayerStats.m_IsCanDoubleJump && m_JumpPlatform != null)
                {
                    //player is not on the ground and have double jump
                    if (!m_Grounded && m_IsHaveDoubleJump && jump && !m_IsOnJumpBox)
                    {
                        m_IsHaveDoubleJump = false;

                        m_Rigidbody2D.velocity = Vector2.zero;
                        m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce - 100f));

                        InputControlManager.Instance.StartGamepadVibration(1, 0.1f);

                        ShowDoubleJumpEffect();
                    }
                }

                if (PlayerStats.m_IsCanDash && m_DashEffect != null)
                {
                    if (dash && m_Anim.GetFloat("Speed") > 0.01f)
                    {
                        m_Anim.SetBool("Dash", true);

                        StartCoroutine(ShowDashEffect());

                        InputControlManager.Instance.StartGamepadVibration(1, 0.2f);
                    }

                    if (m_Anim.GetBool("Dash") && !m_Anim.GetBool("Hit"))
                    {
                        var multiplier = !m_FacingRight ? -1 : 1;
                        m_Rigidbody2D.velocity = new Vector2(10f * multiplier, 0f);
                    }
                }
            }

        }

        public void SetPlayerOnJumpBox(bool value)
        {
            m_IsOnJumpBox = value;
        }

        public void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        #region effects

        private void ShowDustEffect()
        {
            AudioManager.Instance.Play(m_LandAudio); //play dust effect

            if (m_IsOnWall || m_IsWallBehind == 1)
                Destroy(Instantiate(m_WallDustEffect, m_PositionForWallDust.Subtract(y: -0.05f), Quaternion.identity), 2f);
            else
                Destroy(Instantiate(m_LandEffect, m_GroundCheck.position, Quaternion.identity), 2f); //destroy particle gameobject after 3sec
                
        }

        public void OnLandEffect()
        {
            OnLandEvent?.Invoke();
        }

        private void ShowDoubleJumpEffect()
        {
            AudioManager.Instance.Play(m_DoubleJumpAudio); //play double jump audio

            m_Anim.Play("Jump", -1, 0f); //play jump animation again

            //place jump platform on player's feet
            m_JumpPlatform.transform.position = m_GroundCheck.transform.position;

            //show platform
            m_JumpPlatform.SetActive(false);
            m_JumpPlatform.SetActive(true);
        }

        private IEnumerator ShowDashEffect()
        {
            m_Rigidbody2D.gravityScale = 0f;

            AudioManager.Instance.Play(m_DashAudio); //play dash audio

            var dashMaterial = GetComponent<SpriteRenderer>().material; //material for echo effect
            
            var echoAmount = 4;
            var timeBetweenEcho = .05f;

            Camera.main.GetComponent<Camera2DFollow>().Shake(.08f, timeBetweenEcho * echoAmount);

            //create echo
            for (var count = 0; count < echoAmount && 
                            (!m_Anim.GetBool("Hit") && m_Anim.GetBool("Dash")); count++)
            {
                var instantiateDashEffect = Instantiate(m_DashEffect); //create echo effect

                instantiateDashEffect.transform.position = transform.position; //on player's current position
                instantiateDashEffect.transform.localScale = transform.localScale; //flip echo
                instantiateDashEffect.GetComponent<SpriteRenderer>().material = dashMaterial; //change echo material

                Destroy(instantiateDashEffect, 1); //destroy echo

                yield return new WaitForSeconds(timeBetweenEcho); //wait timer before create next echo
            }

            ReturnDefaultValuesAfterDash();
        }

        private void ReturnDefaultValuesAfterDash()
        {
            m_Rigidbody2D.gravityScale = 3f;

            m_Anim.SetBool("Dash", false);

            m_Rigidbody2D.velocity = Vector2.zero;
        }

        #endregion

        private IEnumerator OnCollisionEnter2D(Collision2D collision)
        {
            if (m_Anim.GetBool("Dash") && !((m_WhatIsGround & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer))
            {
                yield return new WaitForEndOfFrame();

                m_Anim.SetBool("Dash", false);
                m_Rigidbody2D.gravityScale = 3f;

                m_Rigidbody2D.velocity = Vector2.zero;

                Camera.main.GetComponent<Camera2DFollow>().StopShake();
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (m_IsOnWall && ((m_WhatIsGround & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer))
            {
                m_PositionForWallDust = collision.contacts[0].point;
            }
        }
    }
}

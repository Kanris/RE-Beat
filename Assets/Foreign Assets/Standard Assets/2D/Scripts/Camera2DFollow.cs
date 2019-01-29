using UnityEngine;
using System.Collections;

namespace UnityStandardAssets._2D
{
    public class Camera2DFollow : MonoBehaviour
    {
        public Transform target; //target to follow
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;

        [Header("Camera bounds")]
        public BoxCollider2D m_CameraBounds; //camera bounds in scene

        private float m_OffsetZ;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_CurrentVelocity;
        private Vector3 m_LookAheadPos;

        private float leftBound, rightBound, bottomBound, topBound;

        private float m_DefaultCamSize;    
        //smooth zoom camera
        private const float m_DurationTime = 1f;
        private bool m_Transition;
        private float elapsed = 0.0f;
        private float m_CamSize;
        private float m_CurrentCameraSize;

        private float m_CamShakeAmount;

        private void Start()
        {
            m_DefaultCamSize = Camera.main.orthographicSize;

            SetCameraBounds();
        }

        private void SetCameraBounds()
        {
            float camExtentV = Camera.main.orthographicSize;
            float camExtentH = (camExtentV * Screen.width) / Screen.height;

            var levelBounds = m_CameraBounds.bounds;

            leftBound = levelBounds.min.x + camExtentH;
            rightBound = levelBounds.max.x - camExtentH;
            bottomBound = levelBounds.min.y + camExtentV;
            topBound = levelBounds.max.y - camExtentV;
        }

        // Update is called once per frame
        private void Update()
        {
            if (target != null)
            {
                // only update lookahead pos if accelerating or changed direction
                float xMoveDelta = (target.position - m_LastTargetPosition).x;

                bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

                if (updateLookAheadTarget)
                {
                    m_LookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
                }
                else
                {
                    m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.unscaledDeltaTime * lookAheadReturnSpeed);
                }

                Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
                Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping, Mathf.Infinity, Time.unscaledDeltaTime);

                newPos = new Vector3(Mathf.Clamp(newPos.x, leftBound, rightBound), 
                                        Mathf.Clamp(newPos.y, bottomBound, topBound), newPos.z);

                transform.position = newPos;

                m_LastTargetPosition = target.position;   
            }

            if (m_Transition)
            {
                elapsed += Time.unscaledDeltaTime / m_DurationTime;

                Camera.main.orthographicSize = Mathf.Lerp(m_CurrentCameraSize, m_CamSize, elapsed);

                m_Transition &= elapsed <= 1.0f;
            }
        }

        public IEnumerator SetTarget(Transform player)
        {
            target = player;
            m_LastTargetPosition = target.position;
            m_OffsetZ = (transform.position - target.position).z;

            Camera.main.orthographicSize = m_DefaultCamSize;

            damping = 0f; //move fast camera to the player

            yield return new WaitForSeconds(10f);

            damping = .2f; //return smoothing
        }

        public void ChangeTarget(Transform newTarget)
        {
            target = newTarget;
            m_LastTargetPosition = target.position;
            m_OffsetZ = (transform.position - target.position).z;
        }

        public void SetCameraSize(float size = 0f)
        {
            m_Transition = true;

            m_CamSize = size > 0f ? size : m_DefaultCamSize;

            m_CurrentCameraSize = Camera.main.orthographicSize;
        }

        #region camera effects

        #region shake

        //play shake effect
        public void Shake(float amount, float length)
        {
            m_CamShakeAmount = amount;

            InvokeRepeating("DoShake", 0f, 0.01f);
            Invoke("StopShake", length);
        }

        //shake camera
        private void DoShake()
        {
            if (m_CamShakeAmount > 0)
            {
                var camPosition = Camera.main.transform.position;

                var offsetX = Random.value * m_CamShakeAmount * 2 - m_CamShakeAmount;
                var offsetY = Random.value * m_CamShakeAmount * 2 - m_CamShakeAmount;

                camPosition.x += offsetX;
                camPosition.y += offsetY;

                Camera.main.transform.position = camPosition;
            }
        }

        //stop shaking
        public void StopShake()
        {
            CancelInvoke("DoShake");
        }

        #endregion

        //play hit effect
        public void PlayHitEffect()
        {
            StartCoroutine(PlayCameraHitAnimation());

            InputControlManager.Instance.StartGamepadVibration(2f, .5f);
        }

        //play low health effect
        public void PlayLowHealthEffect()
        {
            Camera.main.GetComponent<Kino.DigitalGlitch>().enabled = true;
        }

        //stop playing low health effect
        public void StopLowHealthEffect()
        {
            Camera.main.GetComponent<Kino.DigitalGlitch>().enabled = false;
        }

        //play revive effect
        public IEnumerator PlayReviveEffect()
        {
            InputControlManager.Instance.StartGamepadVibration(2f, .5f);

            Camera.main.GetComponent<Kino.AnalogGlitch>().enabled = true;

            Camera.main.GetComponent<Kino.AnalogGlitch>().verticalJump = .2f;

            yield return new WaitForSeconds(0.5f);

            Camera.main.GetComponent<Kino.AnalogGlitch>().verticalJump = 0f;

            Camera.main.GetComponent<Kino.AnalogGlitch>().enabled = false;
        }

        private IEnumerator PlayCameraHitAnimation()
        {
            Camera.main.GetComponent<Kino.AnalogGlitch>().enabled = true;

            yield return new WaitForSeconds(0.5f);

            Camera.main.GetComponent<Kino.AnalogGlitch>().enabled = false;
        }

        #endregion
    }
}

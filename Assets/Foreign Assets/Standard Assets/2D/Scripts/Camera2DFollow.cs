using UnityEngine;
using System.Collections;

namespace UnityStandardAssets._2D
{
    public class Camera2DFollow : MonoBehaviour
    {
        public Transform target;
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;

        [Header("Camera bounds")]
        public BoxCollider2D m_CameraBounds;

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

        private void Start()
        {
            m_DefaultCamSize = Camera.main.orthographicSize;

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
                    m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
                }

                Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
                Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

                newPos = new Vector3(Mathf.Clamp(newPos.x, leftBound, rightBound), 
                                        Mathf.Clamp(newPos.y, bottomBound, topBound), newPos.z);

                transform.position = newPos;

                m_LastTargetPosition = target.position;   
            }

            if (m_Transition)
            {
                elapsed += Time.fixedDeltaTime / m_DurationTime;

                Camera.main.orthographicSize = Mathf.Lerp(m_DefaultCamSize, m_CamSize, elapsed);

                if (elapsed > 1.0f)
                {
                    m_Transition = false;
                }
            }
        }

        public IEnumerator SetTarget(Transform player)
        {
            target = player;
            m_LastTargetPosition = target.position;
            m_OffsetZ = (transform.position - target.position).z;

            Camera.main.orthographicSize = m_DefaultCamSize;

            damping = 0f;

            yield return null;

            damping = .2f;
        }

        public void SetCameraSize(float size = 0f)
        {
            m_Transition = true;

            m_CamSize = size > 0f ? size : m_DefaultCamSize;
        }

        #region camera effects

        public void PlayHitEffect()
        {
            StartCoroutine(PlayCameraHitAnimation());
        }

        public void PlayLowHealthEffect()
        {
            Camera.main.GetComponent<Kino.DigitalGlitch>().enabled = true;
        }

        public void StopLowHealthEffect()
        {
            Camera.main.GetComponent<Kino.DigitalGlitch>().enabled = false;
        }

        public IEnumerator PlayReviveEffect()
        {
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

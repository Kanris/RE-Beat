using System;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class Camera2DFollow : MonoBehaviour
    {
        public Transform target;
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;

        private float m_OffsetZ;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_CurrentVelocity;
        private Vector3 m_LookAheadPos;

        private float m_UpdateSearchTime = 0f;
        [SerializeField, Range(0, -100)] private float YMinusPosition = -1f;
        [SerializeField, Range(0, 100)] private float YPlusPosition = 3f;
        [SerializeField, Range(0, 100)] private float XPlusPosition = 3f;

        // Use this for initialization
        private void Start()
        {
            if (target != null)
            {
                m_LastTargetPosition = target.position;
                m_OffsetZ = (transform.position - target.position).z;
                transform.parent = null;     
            }
            else
            {
                SearchForTarget();
            }

        }


        // Update is called once per frame
        private void Update()
        {
            if (target == null)
            {
                if (m_UpdateSearchTime < Time.time)
                {
                    SearchForTarget();
                }
            }
            else
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

                newPos = new Vector3(Mathf.Clamp(newPos.x, -Mathf.Infinity, XPlusPosition), 
                                     Mathf.Clamp(newPos.y, YMinusPosition, YPlusPosition), newPos.z);

                transform.position = newPos;

                m_LastTargetPosition = target.position;   
            }
        }

        public void ChangeCameraPosition(Vector2 position)
        {
            if (target == null)
            {
                transform.position = new Vector3(position.x, position.y, transform.position.z);
            }
        }

        public void ChangeCameraTarget(Transform newTarget)
        {
            if (newTarget != null)
            {
                target = newTarget;
            }
            else
            {
                Debug.LogError("Camera2DFollow.ChangeCameraTarget: newTarget is null");
            }
        }

        private void SearchForTarget()
        {
            var player = GameObject.FindWithTag("Player");

            if (player == null)
                m_UpdateSearchTime = Time.time + 1f;
            else
            {
                target = player.transform;
                m_OffsetZ = (transform.position - target.position).z;
            }

        }
    }
}

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class DroneKamikaze : MonoBehaviour {

    private Rigidbody2D m_Rigidbody;
    private Vector2 m_PreviousPosition;
    private bool m_IsDestroying = false; //is drone going to blow up
    private float m_UpdateTimer = 0f;

    #region initialize

    // Use this for initialization
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        GetComponent<DroneStats>().OnDroneDestroy += SetOnDestroy;

        MoveInRandomDirection();
    }

    #endregion

    private void FixedUpdate()
    {
        m_Rigidbody.velocity =
                        new Vector2(Mathf.Clamp(m_Rigidbody.velocity.x, -5f, 5f), 
                        Mathf.Clamp(m_Rigidbody.velocity.y, -5f, 5f));

        if (m_UpdateTimer < Time.time & !m_IsDestroying)
        {
            if (m_PreviousPosition == m_Rigidbody.position)
            {
                MoveInRandomDirection();
            }
            else
            {
                m_UpdateTimer = Time.time + 0.1f;
                m_PreviousPosition = m_Rigidbody.position;
            }
        }
    }

    private void MoveInRandomDirection()
    {
        var randX = Random.Range(0, 2);
        var randY = Random.Range(0, 2);

        m_Rigidbody.velocity = new Vector2(randX == 0 ? -2f : 2f, randY == 0 ? -2f : 2f);
    }

    private void SetOnDestroy(bool value)
    {
        m_IsDestroying = value;
    }
}

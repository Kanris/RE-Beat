using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingPlatform : MonoBehaviour {

    public enum FlyingPlatformType { Vertical, Horizontal }
    public enum DirectionType { LeftDown, RightUp }

    [SerializeField] private FlyingPlatformType PlatformType;
    [SerializeField, Range(1, 20)] private float MovingTime = 1f;
    [SerializeField, Range(1, 20)] private float IdleTime = 3f;
    [SerializeField] private DirectionType Direction;

    private Vector3 m_MoveVector;
    private bool m_Idle;
    private float m_UpdateTime = 0f;
    private float m_StartDirection = -1f;

    // Use this for initialization
    void Start () {

        InitializeStartDirection();

        InitializeDirection();

    }

    private void InitializeDirection()
    {
        switch (PlatformType)
        {
            case FlyingPlatformType.Horizontal:
                m_MoveVector = Vector3.right;
                break;
            case FlyingPlatformType.Vertical:
                m_MoveVector = Vector3.up;
                break;
        }

        m_MoveVector *= m_StartDirection;
    }

    private void InitializeStartDirection()
    {
        switch (Direction)
        {
            case DirectionType.LeftDown:
                m_StartDirection = 1f;
                break;
            case DirectionType.RightUp:
                m_StartDirection = -1f;
                break;
        }
    }
	
	// Update is called once per frame
	private void FixedUpdate () {

        if (m_UpdateTime <= Time.time)
        {
            m_Idle = !m_Idle;
            m_UpdateTime = Time.time;

            if (m_Idle)
            { 
                ChangeDirection();
                m_UpdateTime += IdleTime;
            }
            else
                m_UpdateTime += MovingTime;
        }
        else
        {
            if (!m_Idle)
            {
                transform.position += m_MoveVector * Time.fixedDeltaTime * 1.5f;
            }
        }

	}

    private void ChangeDirection()
    {
        m_MoveVector = -m_MoveVector;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
        else if (collision.transform.CompareTag("Ground"))
        {
            ChangeDirection();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}

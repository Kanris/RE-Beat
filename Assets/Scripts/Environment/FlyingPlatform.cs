using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingPlatform : MonoBehaviour {

    #region enum

    public enum FlyingPlatformType { Vertical, Horizontal } //flying platform type
    [SerializeField] private FlyingPlatformType PlatformType;

    public enum DirectionType { LeftDown, RightUp } //move direction
    [SerializeField] private DirectionType Direction;

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField, Range(1, 20)] private float MovingTime = 1f; //how long is platform moving
    [SerializeField, Range(1, 20)] private float IdleTime = 3f; //how long is platform idle

    #endregion

    private Vector3 m_MoveVector; //platform move vectore
    private bool m_Idle; //is platform idling
    private float m_UpdateTime = 0f; //change state timer
    private float m_StartDirection = -1f; //move direction

    #endregion

    #region private methods

    #region initialize

    // Use this for initialization
    private void Start () {

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

    #endregion

    // Update is called once per frame
    private void FixedUpdate () {

        if (m_UpdateTime <= Time.time) //if need to change state
        {
            m_Idle = !m_Idle; //change state
            m_UpdateTime = Time.time;

            if (m_Idle) //is state is idle
            { 
                ChangeDirection(); //change platform direction
                m_UpdateTime += IdleTime; //idle pause
            }
            else
                m_UpdateTime += MovingTime; //moving time
        }
        else if (!m_Idle) //if platform is moving
        {
            transform.position += m_MoveVector * Time.fixedDeltaTime * 1.5f;
        }

	}

    private void ChangeDirection()
    {
        m_MoveVector = -m_MoveVector; //change platform move direction
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player is on the platform
        {
            collision.transform.parent.SetParent(transform); //attach player to the platform so he will be moved with platform
        }
        else if (collision.transform.CompareTag("Item"))
        {
            collision.transform.SetParent(transform); //attach item to the platform so he will be moved with platform
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player is leave platform
        {
            collision.transform.parent.SetParent(null); //detach player from the platform
        }
    }

    #endregion

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingPlatform : MonoBehaviour {

    #region private fields

    #region serialize fields
    
    [Header("Stats")]
    [SerializeField, Range(1, 20)] private float m_IdleTime = 3f; //how long is platform idle
    [SerializeField, Range(1f, 20f)] private float m_Speed = 2f; 

    [Header("Destination")]
    [SerializeField] private Transform m_Points;
    [SerializeField, Range(0, 1)] private int m_CurrentIndex = 0;
    #endregion

    private Vector3 m_MoveVector; //platform move vectore
    private bool m_Idle; //is platform idling
    private float m_UpdateTime = 0f; //change state timer

    #endregion

    #region private methods

    // Update is called once per frame
    private void Update () {

        if (m_UpdateTime <= Time.time) //if need to change state
        {
            m_Idle = !m_Idle; //change state
        }

        if (!m_Idle) //if platform is moving
        {
            transform.position = Vector2.MoveTowards(transform.position, m_Points.GetChild(m_CurrentIndex).position, Time.deltaTime * m_Speed);

            if (transform.position == m_Points.GetChild(m_CurrentIndex).position)
            {
                m_UpdateTime = m_IdleTime + Time.time;
                m_Idle = true;

                GetNextDestination();
            }
        }

	}

    private void GetNextDestination()
    {
        m_CurrentIndex++;

        if (m_CurrentIndex >= m_Points.childCount)
        {
            m_CurrentIndex = 0;
        }
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

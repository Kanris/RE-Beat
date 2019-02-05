using System.Collections;
using UnityEngine;

public class Saw : MonoBehaviour {

    #region private fields

    [Header("Stats")]
    [SerializeField, Range(0, 10)] private int DamageAmount = 1; //saw damage amount

    [Header("Points")]
    [SerializeField] private Transform m_Points; //points between saw has to move
    [SerializeField, Range(0, 1)] private int m_CurrentIndex = 0; //where saw move first (next determine by GetNextDestination method)
    [SerializeField, Range(0f, 10f)] private float m_Speed = 1.5f;  //saw moving speed
    
    [Header("Additional")]
    [SerializeField] private bool m_IsNotMove = false; //is saw has ti move

    private Animator m_Animator; //saw animator

    #endregion

    #region private methods

    private void Start()
    {
        m_Animator = GetComponent<Animator>(); //initialize saw animator
        SawAnimation(true); //move saw
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player in saw colision
        {
            //return player to return point
            collision.gameObject.GetComponent<Player>().playerStats.ReturnPlayerOnReturnPoint();
        }
    }

    private void Update()
    {
        if (!m_IsNotMove) //if saw without trigger
        {
            //move saw to the current poin
            transform.position = Vector2.MoveTowards(transform.position, m_Points.GetChild(m_CurrentIndex).position, Time.deltaTime * m_Speed);

            //if saw is on current point
            if (transform.position == m_Points.GetChild(m_CurrentIndex).position)
            {
                //get next destination point
                GetNextDestination();
            }
        }
    }

    //get next destination point
    private void GetNextDestination()
    {
        //get next index
        m_CurrentIndex++;

        //if index is greater or equal than transforms in m_Points
        if (m_CurrentIndex >= m_Points.childCount)
        {
            //back to first point
            m_CurrentIndex = 0;
        }
    }

    //set saw moving animation
    private void SawAnimation(bool value)
    {
        m_Animator.SetBool("Move", value); //set saw animation
    }

    private void OnValidate()
    {
        //if saw shouldn't move
        if (m_IsNotMove)
        {
            //hide move points
            m_Points.gameObject.SetActive(false);
        }
    }

    #endregion
}


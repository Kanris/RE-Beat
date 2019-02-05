using System.Collections;
using UnityEngine;

public class Saw : MonoBehaviour {

    #region private fields

    [Header("Stats")]
    [SerializeField, Range(0, 10)] private int DamageAmount = 1; //saw damage amount

    [Header("Points")]
    [SerializeField] private Transform m_Points;
    [SerializeField, Range(0, 1)] private int m_CurrentIndex = 0;
    [SerializeField, Range(0f, 10f)] private float m_Speed = 1.5f; 
    
    [Header("Additional")]
    [SerializeField] private bool m_IsNotMove = false;
    [SerializeField] private bool WithTrigger = false; //is saw have to move with trigger

    private Animator m_Animator; //saw animator

    #endregion

    #region private methods

    private void Start()
    {
        m_Animator = GetComponent<Animator>(); //initialize saw animator

        if (!WithTrigger) //if saw without trigger
            SawAnimation(true); //move saw
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().playerStats.ReturnPlayerOnReturnPoint();
        }
    }

    private void Update()
    {
        if (!WithTrigger && !m_IsNotMove) //if saw without trigger
        {
            transform.position = Vector2.MoveTowards(transform.position, m_Points.GetChild(m_CurrentIndex).position, Time.deltaTime * m_Speed);

            if (transform.position == m_Points.GetChild(m_CurrentIndex).position)
            {
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

    private void SawAnimation(bool value)
    {
        m_Animator.SetBool("Move", value); //set saw animation
    }

    private void OnValidate()
    {
        if (m_IsNotMove)
        {
            m_Points.gameObject.SetActive(false);
        }
    }

    #endregion

    #region public methods

    public IEnumerator MoveWithHide(int whereToMove)
    {
        SawAnimation(true); //show saw move animation
        GetComponent<CircleCollider2D>().enabled = true; //enable saw collider

        /*var whereToMoveX = 2f * whereToMove; //get where to move

        Move(0f, 1f); //move saw from ground
        yield return new WaitForSeconds(0.5f);

        Move(whereToMoveX, 0f); //move to the trigger
        yield return new WaitForSeconds(SawMoveTime);

        Move(-whereToMoveX, 0f); //move back from trigger
        yield return new WaitForSeconds(SawMoveTime);

        Move(0f, -1f); //hide saw in ground
        yield return new WaitForSeconds(0.5f);

        GetComponent<CircleCollider2D>().enabled = false; //disable saw collider

        StopMove(); //stop saw moving*/

        yield return new WaitForEndOfFrame();
    }

    #endregion
}


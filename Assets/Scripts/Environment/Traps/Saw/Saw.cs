using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Saw : MonoBehaviour {

    #region private fields

    #region serialize fields

    #region enum
    public enum Direction { Horizontal, Vertical, Idle } //saw move direction
    [Header("Saw properties")]
    public Direction direction; //this saw direction

    public enum WhereToMove { LeftDown, RightUp } //where to move
    public WhereToMove whereToMove; //where this saw have to move

    #endregion
    [SerializeField, Range(0.5f, 20f)] private float SawMoveTime = 2.5f; //saw move time
    [SerializeField, Range(1f, 5f)] private float MoveVelocity = 2f; //saw move speed
    [SerializeField, Range(0, 10)] private int DamageAmount = 2; //saw damage amount

    [Header("Is Saw bound with trigger")]
    [SerializeField] private bool WithTrigger = false; //is saw have to move with trigger

    #endregion

    private Animator m_SawAnimator; //saw animator
    private Rigidbody2D m_Rigidbody; //saw rigidbody
    private float m_MoveTime = 0f; //saw moving time
    //where to move
    private float m_PosX = 0f; 
    private float m_PosY = 0f;
    //previous player throw position
    private float m_PrevThrowX; 
    private float m_PrevThrowY;

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        m_SawAnimator = GetComponent<Animator>(); //initialize saw animator

        m_Rigidbody = GetComponent<Rigidbody2D>(); //initialize saw rigidbody

        InitializeMovement(); //initialize where saw have to move

        if (!WithTrigger) //if saw without trigger
            SawAnimation("Move"); //move saw
    }

    private void InitializeMovement()
    {
        if (direction == Direction.Horizontal) // if saw move horizontaly
        {
            m_PosX = MoveVelocity; 

            if (whereToMove == WhereToMove.LeftDown)
                m_PosX *= -1;
        }
        else if (direction == Direction.Vertical)  // if saw move verticaly
        {
            m_PosY = MoveVelocity;

            if (whereToMove == WhereToMove.LeftDown)
                m_PosY *= -1;
        }
    }

    #endregion

    #region collision

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);
        }
    }
    #endregion

    #region saw move

    private void StopMove()
    {
        m_Rigidbody.velocity = Vector2.zero; //stop saw velocity

        SawAnimation("Idle"); //play idle animation
    }

    private void Move(float posX, float posY)
    {
        m_Rigidbody.velocity = new Vector2(posX, posY); //change saw velocity
    }

    #endregion

    private void FixedUpdate()
    {
        if (!WithTrigger) //if saw without trigger
        {
            if (m_MoveTime <= Time.time) //if saw have to change direction
            {
                m_MoveTime = Time.time + SawMoveTime; //set up next change direction time

                Move(m_PosX, m_PosY); //move saw
                ChangePositionDirection(); //change next saw direction
            }
        }
    }

    private void SawAnimation(string animation)
    {
        m_SawAnimator.SetTrigger(animation); //set saw animation
    }

    private void ChangePositionDirection()
    {
        m_PosX *= -1;
        m_PosY *= -1;
    }

    #endregion

    #region public methods

    public IEnumerator MoveWithHide(int whereToMove)
    {
        SawAnimation("Move"); //show saw move animation
        GetComponent<CircleCollider2D>().enabled = true; //enable saw collider

        var whereToMoveX = 2f * whereToMove; //get where to move

        Move(0f, 1f); //move saw from ground
        yield return new WaitForSeconds(0.5f);

        Move(whereToMoveX, 0f); //move to the trigger
        yield return new WaitForSeconds(SawMoveTime);

        Move(-whereToMoveX, 0f); //move back from trigger
        yield return new WaitForSeconds(SawMoveTime);

        Move(0f, -1f); //hide saw in ground
        yield return new WaitForSeconds(0.5f);

        GetComponent<CircleCollider2D>().enabled = false; //disable saw collider

        StopMove(); //stop saw moving
    }

    #endregion
}


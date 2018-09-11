using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class Saw : MonoBehaviour {

    public enum Direction { Horizontal, Vertical, Idle }
    public Direction direction;

    public enum WhereToMove { LeftDown, RightUp }
    public WhereToMove whereToMove;

    [SerializeField, Range(0.5f, 20f)] private float SawMoveTime = 2.5f;
    [SerializeField, Range(1, 10)] private int DamageAmount = 2;
    [SerializeField, Range(1f, 5f)] private float MoveVelocity = 2f;
    [SerializeField] private bool WithTrigger = false;
    [SerializeField, Range(1f, 10f)] private float m_ThrowX = 5f;
    [SerializeField, Range(1f, 10f)] private float m_ThrowY = 3f;

    private Animator m_SawAnimator;
    private Rigidbody2D m_Rigidbody;
    private float m_MoveTime = 0f;
    private float m_PosX = 0f;
    private float m_PosY = 0f;

    private float m_PrevThrowX;
    private float m_PrevThrowY;

    #region Initialize

    private void Start()
    {
        InitializeAnimator();

        InitializeRigidbody();

        InitializeMovement();

        if (!WithTrigger)
            SawAnimation("Move");
    }

    private void InitializeAnimator()
    {
        m_SawAnimator = GetComponent<Animator>();
    }

    private void InitializeRigidbody()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void InitializeMovement()
    {
        if (direction == Direction.Horizontal)
        {
            m_PosX = MoveVelocity;

            if (whereToMove == WhereToMove.LeftDown)
                m_PosX *= -1;
        }
        else if (direction == Direction.Vertical)
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
            var playerStats = collision.gameObject.GetComponent<Player>().playerStats;

            m_PrevThrowX = playerStats.m_ThrowX;
            m_PrevThrowY = playerStats.m_ThrowY;

            playerStats.m_ThrowX = m_ThrowX;
            playerStats.m_ThrowY = m_ThrowY;

            playerStats.TakeDamage(DamageAmount);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            var playerStats = collision.gameObject.GetComponent<Player>().playerStats;

            playerStats.m_ThrowX = m_PrevThrowX;
            playerStats.m_ThrowY = m_PrevThrowY;
        }
    }
    #endregion

    #region saw move

    public IEnumerator MoveWithHide(int whereToMove)
    {
        SawAnimation("Move");
        AddSawCollision(true);

        var whereToMoveX = 2f * whereToMove;

        Move(0f, 1f);
        yield return new WaitForSeconds(0.5f);

        Move(whereToMoveX, 0f);
        yield return new WaitForSeconds(SawMoveTime);

        Move(-whereToMoveX, 0f);
        yield return new WaitForSeconds(SawMoveTime);

        Move(0f, -1f);
        yield return new WaitForSeconds(0.5f);

        AddSawCollision(false);

        StopMove();
    }

    private void StopMove()
    {
        m_Rigidbody.velocity = Vector2.zero;

        SawAnimation("Idle");
    }

    private void Move(float posX, float posY)
    {
        m_Rigidbody.velocity = new Vector2(posX, posY);
    }

    #endregion

    private void FixedUpdate()
    {
        if (!WithTrigger)
        {
            if (m_MoveTime <= Time.time)
            {
                m_MoveTime = Time.time + SawMoveTime;

                Move(m_PosX, m_PosY);
                ChangePositionDirection();
            }
        }
    }

    private void SawAnimation(string animation)
    {
        m_SawAnimator.SetTrigger(animation);
    }

    private void AddSawCollision(bool isAdd)
    {
        if (isAdd)
            gameObject.AddComponent<CircleCollider2D>();
        else
            Destroy(GetComponent<CircleCollider2D>());
    }

    private void ChangePositionDirection()
    {
        m_PosX = -m_PosX;
        m_PosY = -m_PosY;
    }
}


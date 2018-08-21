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

    private Animator m_SawAnimator;
    private Rigidbody2D m_Rigidbody;
    private bool m_IsMove;
    private float m_PosX = 0f;
    private float m_PosY = 0f;

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
            collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);
        }
    }

    #endregion

    #region saw move

    public IEnumerator MoveWithHide(int whereToMove)
    {
        SawAnimation("Move");
        AddSawCollision(true);

        var whereToMoveX = 2f * whereToMove;

        yield return Move(0f, 1f, 0.5f);

        yield return Move(whereToMoveX, 0f, SawMoveTime);

        yield return Move(-whereToMoveX, 0f, SawMoveTime);

        yield return new WaitForSeconds(0.1f);

        yield return Move(0f, -1f, 0.5f);

        SawAnimation("Idle");
        AddSawCollision(false);
    }

    private IEnumerator Move(float posX, float posY, float time)
    {
        m_IsMove = true;
        m_Rigidbody.velocity = new Vector2(posX, posY);

        yield return new WaitForSeconds(time);

        m_Rigidbody.velocity = Vector2.zero;
        m_IsMove = false;
    }

    #endregion

    private void FixedUpdate()
    {
        if (!WithTrigger)
        {
            if (!m_IsMove)
            {
                StartCoroutine(Move(m_PosX, m_PosY, SawMoveTime));
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


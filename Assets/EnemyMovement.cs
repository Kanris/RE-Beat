using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class EnemyMovement : MonoBehaviour {

    private Rigidbody2D m_Rigidbody2D;
    private Animator m_Animator;
    private float m_PosX = -1f;
    private bool isWaiting = false;
    private Vector2 m_PreviousPosition;

    private float Speed = 1f;
    private float IdleTime = 2f;

    // Use this for initialization
    void Start () {
        InitializeRigidBody();

        InitializeAnimator();
    }

    private void InitializeRigidBody()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (m_Rigidbody2D == null)
        {
            Debug.LogError("EnemyMovement.InitializeRigidBody: Can't find rigidbody on gameobject");
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("EnemyMovement.InitializeAnimator: Can't find animator on gameobject");
        }
    }

    private void FixedUpdate()
    {
        if (!isWaiting)
        {
            m_Rigidbody2D.position += new Vector2(m_PosX, 0) * Time.fixedDeltaTime * Speed;
            SetAnimation();

            if (m_Rigidbody2D.position == m_PreviousPosition & !isWaiting)
                StartCoroutine(Idle());
            else
                m_PreviousPosition = m_Rigidbody2D.position;
        }
    }

    private IEnumerator Idle()
    {
        isWaiting = true;
        m_PosX = -m_PosX;
        SetAnimation();

        yield return new WaitForSeconds(IdleTime);

        isWaiting = false;
    }

    private void SetAnimation()
    {
        m_Animator.SetBool("isWalking", !isWaiting);
        m_Animator.SetFloat("PosX", m_PosX);
    }
}

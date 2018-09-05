using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(BoxCollider2D))]
public class MoveGrass : MonoBehaviour {

    private Animator m_Animator;

    #region Initialize

    // Use this for initialization
    void Start () {

        InitializeAnimator();

    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayMoveAnimation();
        }
    }

    private void PlayMoveAnimation()
    {
        m_Animator.SetTrigger("Move");
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SawTrigger : MonoBehaviour {

    [SerializeField] private float SawMoveTime = 2.5f;

    private Saw m_Saw;
    private Animator m_Animator;
    private bool m_IsTriggered;
    private bool m_IsSawMove;

	// Use this for initialization
	void Start () {

        InitializeSaw();
        
        InitializeAnimator();
    }

    #region Initialize

    private void InitializeSaw()
    {
        m_Saw = transform.GetChild(0).gameObject.GetComponent<Saw>();

        if (m_Saw == null)
        {
            Debug.LogError("SawTrigger.InitializeSaw: Can't find saw in child");
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("SawTrigger.initialzieAnimator: Can't find animator on gameobject");
        }
    }

    #endregion

    #region trigger

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsTriggered)
        {
            m_IsTriggered = true;

            PlayTriggerSound();
            ButtonAnimation("Pressed");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsTriggered)
        {
            m_IsTriggered = true;

            PlayTriggerSound();
            ButtonAnimation("Pressed");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!m_IsSawMove & !m_IsTriggered)
                ButtonAnimation("Unpressed");
        }
    }

    #endregion

    private void FixedUpdate()
    {
        if (m_IsTriggered & !m_IsSawMove)
        {
            StartCoroutine(MoveSaw());
        }
    }

    private int WhereToMove()
    {
        if (m_Saw.transform.position.x > transform.position.x)
        {
            return -1;
        }

        return 1;
    }

    private IEnumerator MoveSaw()
    {
        m_IsSawMove = true;

        yield return m_Saw.MoveWithHide(WhereToMove());

        m_IsSawMove = false;
        m_IsTriggered = false;

        ButtonAnimation("Unpressed");
    }

    private void ButtonAnimation(string animation)
    {
        m_Animator.SetTrigger(animation);
    }

    private void PlayTriggerSound()
    {
        AudioManager.Instance.Play("Button Switch");
    }
}

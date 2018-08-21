using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SawTrigger : MonoBehaviour {

    [SerializeField] private float SawMoveTime = 2.5f;

    private GameObject m_Saw;
    private Animator m_SawAnimator;
    private PlayerStats m_Player;
    private Animator m_Animator;
    private bool m_IsTriggered;
    private bool m_IsSawMove;

	// Use this for initialization
	void Start () {

        InitializeSaw();

        InitializeSawAnimator();

        InitializeAnimator();
    }

    #region Initialize

    private void InitializeSaw()
    {
        m_Saw = transform.GetChild(0).gameObject;

        if (m_Saw == null)
        {
            Debug.LogError("SawTrigger.InitializeSaw: Can't find saw in child");
        }
    }

    private void InitializeSawAnimator()
    {
        m_SawAnimator = m_Saw.GetComponent<Animator>();

        if (m_SawAnimator == null)
        {
            Debug.LogError("SawTrigger.InitializeSawAnimator: Can't find animator on m_Saw or m_Saw is null");
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
            m_Player = collision.GetComponent<Player>().playerStats;
            m_IsTriggered = true;

            PlayTriggerSound();
            ButtonAnimation();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_Player = null;

            if (!m_IsSawMove)
                ButtonAnimation();
        }
    }

    #endregion

    private void FixedUpdate()
    {
        if (m_Player != null)
        {
            if (m_IsTriggered & !m_IsSawMove)
            {
                StartCoroutine(MoveSaw());
            }
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
        SawAnimation("Move");
        AddSawCollision(true);

        var whereToMoveX = 2f * WhereToMove();

        yield return Move(0f, 1f, 0.5f);

        yield return Move(whereToMoveX, 0f, SawMoveTime);
        
        yield return Move(-whereToMoveX, 0f, SawMoveTime);

        yield return new WaitForSeconds(0.1f);

        yield return Move(0f, -1f, 0.5f);

        SawAnimation("Idle");
        AddSawCollision(false);
        m_IsSawMove = false;
        m_IsTriggered = false;

        if (m_Player == null)
            ButtonAnimation();
    }

    #region Saw move

    private IEnumerator Move(float posX, float posY, float time)
    {
        if (m_Saw != null)
        {
            m_Saw.GetComponent<Rigidbody2D>().velocity = new Vector2(posX, posY);

            yield return new WaitForSeconds(time);

            m_Saw.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }

    #endregion

    private void AddSawCollision(bool isAdd)
    {
        if (isAdd)
            m_Saw.AddComponent<CircleCollider2D>();
        else
            Destroy(m_Saw.GetComponent<CircleCollider2D>());
    }

    private void SawAnimation(string animation)
    {
        m_SawAnimator.SetTrigger(animation);
    }

    private void ButtonAnimation()
    {
        var animationTrigger = m_Player != null ? "Pressed" : "Unpressed";

        m_Animator.SetTrigger(animationTrigger);
    }

    private void PlayTriggerSound()
    {
        AudioManager.Instance.Play("Button Switch");
    }
}

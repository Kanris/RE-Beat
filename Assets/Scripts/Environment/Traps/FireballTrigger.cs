using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FireballTrigger : MonoBehaviour {

    public enum Direction { left, right }
    public Direction FireballDirection;

    [SerializeField] private int Count = 3;

    private Animator m_Animator;
    private Transform m_ThrowerTransform;
    private bool isCreatingFireballs;
    private bool isPlayerNear;

    #region Initialize

    private void Start()
    {
        InitializeAnimator();

        InitializeThrowerTransform();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponentInChildren<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("FireballTrigger.InitializeAnimator: Can't find animator");
        }
    }

    private void InitializeThrowerTransform()
    {
        m_ThrowerTransform = transform.GetChild(0);

        if (m_ThrowerTransform == null)
        {
            Debug.LogError("FireballTrigger.InitializeThrowerTransform: Can't find child transform");
        }
    }
    #endregion

    #region trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !isCreatingFireballs & !isPlayerNear)
        {
            isPlayerNear = true;

            ButtonAnimation();
            PlayTriggerSound();
            PrepareFireball();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & isPlayerNear)
        {
            isPlayerNear = false;

            if (!isCreatingFireballs)
                ButtonAnimation();
        }
    }

    #endregion

    #region fireball
    private void PrepareFireball()
    {
        if (!isCreatingFireballs)
        {
            isCreatingFireballs = true;

            var fireballDirection = GetFireballDirection();

            var fireballGameObject = Resources.Load("Fireball") as GameObject;

            StartCoroutine(CreateFireballs(fireballDirection, fireballGameObject));
        }
    }

    private IEnumerator CreateFireballs(Vector3 fireballDirection, GameObject fireballGameObject)
    {
        if (fireballGameObject != null)
        {
            for (int index = 0; index < Count; index++)
            {
                var fireball = Instantiate(fireballGameObject, m_ThrowerTransform.position + fireballDirection, m_ThrowerTransform.rotation);
                fireball.GetComponent<Fireball>().Direction = fireballDirection;

                yield return new WaitForSeconds(1f);
            }
        }

        ButtonAnimation();
        isCreatingFireballs = false;
    }

    private Vector3 GetFireballDirection()
    {
        var fireballDirection = Vector3.right;

        if (FireballDirection == Direction.left)
        {
            fireballDirection = Vector3.left;
        }

        return fireballDirection;
    }

    #endregion

    private void ButtonAnimation()
    {
        var animationTrigger = isPlayerNear ? "Pressed" : "Unpressed";

        m_Animator.SetTrigger(animationTrigger);
    }

    private void PlayTriggerSound()
    {
        AudioManager.Instance.Play("Button Switch");
    }
}

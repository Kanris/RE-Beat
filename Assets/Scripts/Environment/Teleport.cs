using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
public class Teleport : MonoBehaviour {

    [SerializeField] private Transform target;

    private bool m_IsPlayerNear = false;
    private Animator m_Animator;
    private GameObject m_InteractionButton;
    private GameObject m_Player;

    private void Start()
    {
        InitializeAnimator();

        InitializeInteractionButton();

        SetActiveInteractionButton(false);
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Teleport.InitializeAnimator: Can't find animator component on object");
        }
    }

    private void InitializeInteractionButton()
    {
        var interactionUI = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionUI, transform);

    }

    private void Update()
    {
        if (m_IsPlayerNear)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                StartCoroutine(TeleportPlayer());
            }
        }
    }

    private IEnumerator TeleportPlayer()
    {
        if (target != null)
        {
            m_Animator.SetBool("Teleport", true);
            AudioManager.Instance.Play("Teleport");
            m_IsPlayerNear = false;
            m_Player.SetActive(false);

            StartCoroutine(ScreenFaderManager.Instance.FadeToBlack());

            yield return new WaitForSeconds(0.8f);

            m_Player.transform.position = target.position;

            yield return new WaitForSeconds(0.8f);

            SetActiveInteractionButton(false);
            m_Player.SetActive(true);
            m_Animator.SetBool("Teleport", false);

            StartCoroutine(ScreenFaderManager.Instance.FadeToClear());
        }
        else
        {
            Debug.LogError("Teleport.TeleportPlayer: Can't teleport player without target.");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            m_IsPlayerNear = true;
            m_Player = collision.gameObject;
            SetAnimationTrigger("PlayerNear");
            SetActiveInteractionButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsPlayerNear)
        {
            m_IsPlayerNear = false;
            m_Player = null;
            SetAnimationTrigger("Idle");
            SetActiveInteractionButton(false);
        }
    }

    private void SetAnimationTrigger(string trigger)
    {
        m_Animator.SetTrigger(trigger);
    }


    private void SetActiveInteractionButton(bool isActive)
    {
        if (m_InteractionButton != null)
        {
            m_InteractionButton.SetActive(isActive);
        }
    }
}

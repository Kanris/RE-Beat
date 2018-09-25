using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
public class Teleport : MonoBehaviour {

    #region private fields

    [SerializeField] private Transform target; //where to teleport player
    [SerializeField] private Audio TeleportAudio;

    private Animator m_Animator; //teleport animator
    private GameObject m_InteractionButton; //teleport ui
    private GameObject m_Player; //player reference

    #endregion

    #region private methods

    #region initialize

    private void Start()
    {
        m_Animator = GetComponent<Animator>(); //reference to the teleport animator

        InitializeInteractionButton(); //initialize teleport ui

        SetActiveInteractionButton(false); //hide teleport ui
    }

    private void InitializeInteractionButton()
    {
        var interactionUI = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionUI, transform);

    }

    #endregion

    private void Update()
    {
        if (m_Player != null) //if player is near teleport
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player is pressed submit button
            {
                StartCoroutine(TeleportPlayer()); //start teleport
            }
        }
    }

    private IEnumerator TeleportPlayer()
    {
        if (target != null) //if there is destination
        {
            m_Animator.SetBool("Teleport", true); //show teleport animation
            AudioManager.Instance.Play(TeleportAudio); //play teleport sound
            m_Player.SetActive(false); //hide player

            StartCoroutine(ScreenFaderManager.Instance.FadeToBlack()); //show black screen

            yield return new WaitForSeconds(0.8f); //wait before teleport

            m_Player.transform.position = target.position; //teleport player

            yield return new WaitForSeconds(0.8f); //wait before clear screen

            m_Player.SetActive(true); //show player
            m_Animator.SetBool("Teleport", false); //stop teleprt animation

            StartCoroutine(ScreenFaderManager.Instance.FadeToClear()); //show sceen to the player

            ResetToDefaultState();
        }
        else
        {
            Debug.LogError("Teleport.TeleportPlayer: Can't teleport player without target.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near teleport
        {
            m_Player = collision.gameObject; //get reference to the player gameobject
            SetAnimationTrigger("PlayerNear"); //show teleport animation
            SetActiveInteractionButton(true); //show teleport ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is leave teleport trigger
        {
            ResetToDefaultState();
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

    private void ResetToDefaultState()
    {
        m_Player = null; //remove player reference
        SetAnimationTrigger("Idle"); //return to the idle animation
        SetActiveInteractionButton(false); //hide ui buttons
    }

    #endregion
}

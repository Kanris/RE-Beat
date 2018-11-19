using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Teleport : MonoBehaviour {

    #region private fields

    [SerializeField] private Transform m_TeleportTo; //where to teleport player

    [Header("Effects")]
    [SerializeField] private Audio TeleportAudio;
    [SerializeField] private GameObject m_InteractionUI;
    
    private GameObject m_InteractionButton; //teleport ui
    private GameObject m_Player; //player reference

    #endregion

    #region private methods

    #region initialize

    private void Start()
    {
        InitializeInteractionButton(); //initialize teleport ui

        SetActiveInteractionButton(false); //hide teleport ui
    }

    private void InitializeInteractionButton()
    {
        m_InteractionButton = Instantiate(m_InteractionUI, transform);
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
        if (m_TeleportTo != null) //if there is destination
        {
            AudioManager.Instance.Play(TeleportAudio); //play teleport sound
            m_Player.SetActive(false); //hide player

            StartCoroutine(ScreenFaderManager.Instance.FadeToBlack()); //show black screen

            yield return new WaitForSeconds(0.8f); //wait before teleport

            m_Player.transform.position = m_TeleportTo.position; //teleport player

            yield return new WaitForSeconds(0.8f); //wait before clear screen

            m_Player.SetActive(true); //show player

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
        SetActiveInteractionButton(false); //hide ui buttons
    }

    #endregion
}

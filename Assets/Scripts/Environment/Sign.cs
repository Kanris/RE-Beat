using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

public class Sign : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField] private string SignName; //sign name
    [SerializeField, TextArea(2, 10)] private string SignText; //sign text to display

    [Header("Effects")]
    [SerializeField] private GameObject m_InteractionUI;

    #endregion

    private GameObject m_InteractionButton; //sign ui
    private Platformer2DUserControl m_Player; //player control
    private bool m_IsSentenceShowInProgress; //indicates is dialogue still in progress

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    private void Start () {

        InitializeInteractionButton(); //initialize sign ui

        m_InteractionButton.SetActive(false); //hide ui

        DialogueManager.Instance.OnDialogueInProgressChange += ChangeIsInProgress; //subscribe to dialogue in progress
    }

    private void InitializeInteractionButton()
    {
        m_InteractionButton = Instantiate(m_InteractionUI, transform) as GameObject;
    }

    #endregion

    // Update is called once per frame
    void Update () {
		
        if (m_Player != null) //if player is near the sign
        {
            if (!m_IsSentenceShowInProgress) //if dialogue is not in progress
            {
                if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player want to read the sign
                {
                    EnableUserControl(false); //disable user controll
                    m_InteractionButton.SetActive(false); //hide sign ui
                    StartCoroutine(DialogueManager.Instance.DisplaySingleSentence(SignText, SignName, transform)); //show sign text

                } else if (!m_Player.enabled) //if player control is disabled
                {
                    EnableUserControl(true); //enable it
                }
                else if (!m_InteractionButton.activeSelf) //if sign ui is hidden
                {
                    m_InteractionButton.SetActive(true); //show sign ui
                }
            }
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player == null) //if player enter to the sign trigger
        {
            var playerControl = collision.GetComponent<Platformer2DUserControl>(); //get reference to the player control
            SetUpSign(true, playerControl); 
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player != null) //if player leave sign trigger
        {
            SetUpSign(false, null);
        }
    }

    private void EnableUserControl(bool active)
    {
        if (!active) //if player shouldn't move
            m_Player.GetComponent<PlatformerCharacter2D>().Move(0f, false, false, false); //stop player movement

        if (m_Player != null & m_Player.enabled != active)
            m_Player.enabled = active; //change control state
    }

    private void SetUpSign(bool value, Platformer2DUserControl player)
    {
        m_InteractionButton.SetActive(value); //show or hide sign ui
        m_Player = player; //reference to the player's control
    }

    private void ChangeIsInProgress(bool value)
    {
        m_IsSentenceShowInProgress = value;
    }

    #endregion
}

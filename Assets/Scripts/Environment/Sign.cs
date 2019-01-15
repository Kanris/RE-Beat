using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private Transform m_Player; //player control
    private bool m_IsSentenceShowInProgress; //indicates is dialogue still in progress

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    private void Start () {

        InitializeInteractionButton(); //initialize sign ui

        m_InteractionButton.SetActive(false); //hide ui

        DialogueManager.Instance.OnDialogueInProgressChange += ChangeIsInProgress; //subscribe to dialogue in progress

        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetFloat("Speed", Random.Range(.8f, 1.2f));
        }
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
                if (InputControlManager.IsUpperButtonsPressed()) //if player want to read the sign
                {
                    EnableUserControl(false); //disable user controll
                    m_InteractionButton.SetActive(false); //hide sign ui
                    StartCoroutine(DialogueManager.Instance.DisplaySingleSentence(SignText, SignName, transform)); //show sign text

                } else if (!m_Player.GetComponent<Platformer2DUserControl>().IsCanJump) //if player control is disabled
                {
                    EnableUserControl(true); //enable it
                }
                else if (!m_InteractionButton.activeSelf)
                {
                    m_InteractionButton.SetActive(true);
                }
            }
        }
        else if (m_InteractionButton.activeSelf)
        {
            m_InteractionButton.SetActive(false);
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player == null) //if player enter to the sign trigger
        {
            m_InteractionButton.SetActive(true); //show or hide sign ui
            m_Player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player != null) //if player leave sign trigger
        {
            m_InteractionButton.SetActive(false); //show or hide sign ui
            m_Player = null;
        }
    }

    //enable or disable character control
    private void EnableUserControl(bool active)
    {
        if (!active) //if player shouldn't controll character
        {
            m_Player.GetComponent<PlatformerCharacter2D>().Move(0f, false, false, false); //stop any character movement
            m_Player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            m_Player.GetComponent<Animator>().SetBool("Ground", true);

            m_Player.GetComponent<Platformer2DUserControl>().IsCanJump = false;
        }
        else
            m_Player.GetComponent<Platformer2DUserControl>().IsCanJump = true;

        PauseMenuManager.Instance.SetIsCantOpenPauseMenu(!active);
    }

    private void ChangeIsInProgress(bool value)
    {
        m_IsSentenceShowInProgress = value;
    }

    #endregion
}

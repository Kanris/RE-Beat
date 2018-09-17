using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;
using UnityEditor;

public class DialogueTrigger : MonoBehaviour {

    #region public fields

    public Dialogue dialogue; //npc dialogue

    #endregion

    #region private fields

    private GameObject m_UI; //dialogue ui
    private Platformer2DUserControl m_Player; //player's control
    private bool m_IsDialogueInProgress; //is dialogue in progress

    #endregion

    #region Initialize

    private void Start()
    {
        InitializeInteractionButton(); //initialize npc ui (name and interaction button)

        DisplayUI(false); //hide npc ui

        DialogueManager.Instance.OnDialogueInProgressChange += ChangeDialogueInProcess; //watch if dialogue is started or finished
    }

    private void InitializeInteractionButton()
    {
        var ui = Resources.Load("UI/NPCUI") as GameObject;
        m_UI = Instantiate(ui, transform);
    }

    #endregion

    #region private fields

    // Update is called once per frame
    private void Update () {
		
        if (m_Player != null) //if player is near
        {
            if (!m_IsDialogueInProgress) //if dialogue is not in progress
            {
                if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player want to start dialogue
                {
                    DialogueManager.Instance.StartDialogue(transform.name, dialogue); //start dialogue
                    DisplayUI(false); //disable npc ui

                    if (!dialogue.IsDialogueFinished) //if dialogue is not saved
                        GameMaster.Instance.SaveState<int>(gameObject.name, 0, GameMaster.RecreateType.Dialogue); //save dialogue state
                }

                if (!m_Player.enabled) //if dialogue is not in progress and player havn't character control
                {
                    EnableUserControl(true); //enable character control
                }

                if (!m_UI.activeSelf) //if dialogue is not in progress, player is near show and NPC ui isn't shown
                {
                    DisplayUI(true); //show NPC ui
                }
            }
            else if (m_Player.enabled) //if dialogue in progress and player still have character control
                EnableUserControl(false); //disable character control
        }
	}

    //enable or disable character control
    private void EnableUserControl(bool active)
    {
        if (!active) //if player shouldn't controll character
            m_Player.GetComponent<PlatformerCharacter2D>().Move(0f, false, false, false); //stop any character movement

        if (m_Player != null & m_Player.enabled != active) //disable or enable character control
            m_Player.enabled = active;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near npc
        {
            m_Player = collision.GetComponent<Platformer2DUserControl>(); //get character control script
            DisplayUI(true); //show npc ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player was near
        {
            m_Player = null; //delete reference to the character control script
            DisplayUI(false); //hide npc ui
        }
    }

    //show or hide npc ui
    private void DisplayUI(bool isActive)
    {
        if (m_UI != null) //if npc ui was initialized
        {
            m_UI.SetActive(isActive); //show or hide npc ui
        }
        else //if npc ui wasn't initialized
        {
            Debug.LogError("DialogueTrigger.DisplayInteractionButton: m_InteractionButton is not initialized."); //show error
        }
    }

    //change state of the m_IsDialogueInProgress value
    private void ChangeDialogueInProcess(bool value)
    {
        m_IsDialogueInProgress = value;
    }

    #endregion
}

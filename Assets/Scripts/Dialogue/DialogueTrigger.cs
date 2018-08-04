using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;
using UnityEditor;

public class DialogueTrigger : MonoBehaviour {

    public Dialogue dialogue;

    [HideInInspector] public bool m_isPlayerNear = false;

    private GameObject m_UI;
    private GameObject m_Player;

    private void Start()
    {
        InitializeInteractionButton();

        DisplayUI(false);
    }

    private void InitializeInteractionButton()
    {
        var ui = Resources.Load("UI/NPCUI") as GameObject;
        m_UI = Instantiate(ui, transform);
    }

    // Update is called once per frame
    void Update () {
		
        if (m_isPlayerNear)
        {
            if (!DialogueManager.Instance.isDialogueInProgress)
            {
                if (CrossPlatformInputManager.GetButtonDown("Submit"))
                {
                    StartDialogue();
                    GameMaster.Instance.SaveDialogueState(gameObject.name);
                }
                else
                {
                    if (!m_UI.activeSelf)
                        DisplayUI(true);

                    EnableUserControl(true);
                }
            }
            else
                EnableUserControl(false);
        }
	}

    private void EnableUserControl(bool active)
    {
        if (!active)
            m_Player.GetComponent<PlatformerCharacter2D>().Move(0f, false, false);

        if (m_Player != null & m_Player.GetComponent<Platformer2DUserControl>().enabled != active)
            m_Player.GetComponent<Platformer2DUserControl>().enabled = active;
    }

    private void StartDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);

        DisplayUI(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_isPlayerNear)
        {
            m_isPlayerNear = true;
            m_Player = collision.gameObject;
            DisplayUI(m_isPlayerNear);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_isPlayerNear)
        {
            m_isPlayerNear = false;
            m_Player = null;
            DisplayUI(m_isPlayerNear);
        }
    }

    private void DisplayUI(bool isActive)
    {
        if (m_UI != null)
        {
            m_UI.SetActive(isActive);
        }
        else
        {
            Debug.LogError("DialogueTrigger.DisplayInteractionButton: m_InteractionButton is not initialized.");
        }
    }
}

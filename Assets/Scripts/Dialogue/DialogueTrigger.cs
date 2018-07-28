using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {

    public bool m_isDialogueFinished = false;

    [SerializeField] private Dialogue dialogue;
    [SerializeField] private Dialogue repeatDialogue;

    private bool m_isPlayerNear = false;
    private GameObject m_InteractionButton;

    private void Start()
    {
        InitializeInteractionButton();

        DisplayInteractionButton();
    }

    private void InitializeInteractionButton()
    {
        if (transform.childCount > 0)
        {
            m_InteractionButton = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("DialogueTrigger.InitializeInteractionButton: DialogueTrigger has no interaction button");
        }
    }

    // Update is called once per frame
    void Update () {
		
        if (m_isPlayerNear)
        {
            if (!DialogueManager.Instance.isDialogueInProgress)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartDialogue();
                }
            }
        }
        else
        {
            if (DialogueManager.Instance.isDialogueInProgress)
            {
                DialogueManager.Instance.StopDialogue();
            }
        }

	}

    private void StartDialogue()
    {
        var dialogueToStart = m_isDialogueFinished ? repeatDialogue : dialogue;

        DialogueManager.Instance.StartDialogue(dialogueToStart, this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_isPlayerNear)
        {
            m_isPlayerNear = true;
            DisplayInteractionButton();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_isPlayerNear)
        {
            m_isPlayerNear = false;
            DisplayInteractionButton();
        }
    }

    private void DisplayInteractionButton()
    {
        if (m_InteractionButton != null)
        {
            m_InteractionButton.SetActive(m_isPlayerNear);
        }
        else
        {
            Debug.LogError("DialogueTrigger.DisplayInteractionButton: m_InteractionButton is not initialized.");
        }
    }

    private void StopDialogue()
    {
        if (!m_isDialogueFinished)
            m_isDialogueFinished = true;
    }
}

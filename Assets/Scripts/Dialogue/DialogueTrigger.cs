using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {

    [HideInInspector] public bool m_isDialogueFinished = false;

    [SerializeField] private Dialogue dialogue;
    [SerializeField] private Dialogue repeatDialogue;

    private bool m_isPlayerNear = false;
    private GameObject m_UI;

    private void Start()
    {
        InitializeInteractionButton();

        DisplayUI(false);
    }

    private void InitializeInteractionButton()
    {
        if (transform.childCount > 0)
        {
            m_UI = transform.GetChild(0).gameObject;
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
                else
                {
                    if (!m_UI.activeSelf)
                        DisplayUI(true);
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

        DisplayUI(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_isPlayerNear)
        {
            m_isPlayerNear = true;
            DisplayUI(m_isPlayerNear);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_isPlayerNear)
        {
            m_isPlayerNear = false;
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

    private void StopDialogue()
    {
        if (!m_isDialogueFinished)
            m_isDialogueFinished = true;
    }
}

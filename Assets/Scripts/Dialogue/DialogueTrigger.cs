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
    private Platformer2DUserControl m_Player;
    private bool m_IsDialogueInProcess;

    private void Start()
    {
        InitializeInteractionButton();

        DisplayUI(false);

        DialogueManager.Instance.OnDialogueInProgressChange += ChangeDialogueInProcess;
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var ui = Resources.Load("UI/NPCUI") as GameObject;
        m_UI = Instantiate(ui, transform);
    }

    #endregion

    // Update is called once per frame
    void Update () {
		
        if (m_isPlayerNear)
        {
            if (!m_IsDialogueInProcess)
            {
                if (CrossPlatformInputManager.GetButtonDown("Submit"))
                {
                    StartDialogue();
                    GameMaster.Instance.SaveState<int>(gameObject.name, 0, GameMaster.RecreateType.Dialogue);
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

        if (m_Player != null & m_Player.enabled != active)
            m_Player.enabled = active;
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
            m_Player = collision.GetComponent<Platformer2DUserControl>();
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

    private void ChangeDialogueInProcess(bool value)
    {
        m_IsDialogueInProcess = value;
    }
}

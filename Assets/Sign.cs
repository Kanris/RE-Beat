using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

public class Sign : MonoBehaviour {

    [SerializeField] private string SignName;
    [SerializeField, TextArea(2, 10)] private string SignText;

    private GameObject m_InteractionButton;
    private Platformer2DUserControl m_Player;
    private bool m_IsSentenceShowInProgress;

	// Use this for initialization
	void Start () {

        InitializeInteractionButton();

        SetActiveInteractionButton(false);

        DialogueManager.Instance.OnDialogueInProgressChange += ChangeIsInProgress;
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var interactionButtonInResources = Resources.Load("UI/InteractionUI");
        m_InteractionButton = Instantiate(interactionButtonInResources, transform) as GameObject;
    }

    #endregion

    // Update is called once per frame
    void Update () {
		
        if (m_Player != null)
        {
            if (!m_IsSentenceShowInProgress)
            {
                if (CrossPlatformInputManager.GetButtonDown("Submit"))
                {
                    EnableUserControl(false);
                    SetActiveInteractionButton(false);
                    StartCoroutine(DialogueManager.Instance.DisplaySentence(SignText, SignName));

                } else if (!m_Player.enabled)
                {
                    EnableUserControl(true);
                }
                else if (!m_InteractionButton.activeSelf)
                {
                    SetActiveInteractionButton(true);
                }
            }
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player == null)
        {
            var playerControl = collision.GetComponent<Platformer2DUserControl>();
            SetUpSign(true, playerControl);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player != null)
        {
            SetUpSign(false, null);
        }
    }

    private void EnableUserControl(bool active)
    {
        if (!active)
            m_Player.GetComponent<PlatformerCharacter2D>().Move(0f, false, false);

        if (m_Player != null & m_Player.enabled != active)
            m_Player.enabled = active;
    }

    private void SetUpSign(bool value, Platformer2DUserControl player)
    {
        SetActiveInteractionButton(value);
        m_Player = player;
    }

    private void SetActiveInteractionButton(bool value)
    {
        m_InteractionButton.SetActive(value);
    }

    private void ChangeIsInProgress(bool value)
    {
        m_IsSentenceShowInProgress = value;
    }
}

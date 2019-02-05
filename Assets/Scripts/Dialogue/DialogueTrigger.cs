using UnityEngine;
using UnityStandardAssets._2D;

public class DialogueTrigger : MonoBehaviour {

    #region public fields

    public Dialogue dialogue; //npc dialogue

    [SerializeField] private InteractionUIButton m_InteractionUIButton;

    #endregion

    #region private fields
    
    private Transform m_Player; //player's control
    private bool m_IsDialogueInProgress; //is dialogue in progress

    #endregion

    #region Initialize

    private void Start()
    {
        m_InteractionUIButton.PressInteractionButton = StartDialogue;
        DisplayUI(false); //hide npc ui

        DialogueManager.Instance.OnDialogueInProgressChange += ChangeDialogueInProcess; //watch if dialogue is started or finished

        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetFloat("Speed", Random.Range(.8f, 1.2f));
        }
    }

    #endregion

    #region private methods

    // Update is called once per frame
    private void Update () {
		
        if (m_Player != null && InputControlManager.Instance.IsCanUseSubmitButton()) //if player is near
        {
            if (!m_IsDialogueInProgress) //if dialogue is not in progress
            {
                 if (!m_Player.GetComponent<Platformer2DUserControl>().IsCanJump)
                {
                    EnableUserControl(true);
                }
            }
        }
        else if (m_InteractionUIButton.ActiveSelf())
        {
            DisplayUI(false);
        }
    }

    private void StartDialogue()
    {
        if (!m_IsDialogueInProgress)
        {
            DisplayUI(false); //disable npc ui
            EnableUserControl(false);

            DialogueManager.Instance.StartDialogue(transform.name, dialogue, transform, m_Player.gameObject.transform); //start dialogue

            if (!dialogue.IsDialogueFinished) //if dialogue is not saved
                GameMaster.Instance.SaveState<int>(gameObject.name, 0, GameMaster.RecreateType.Dialogue); //save dialogue state
        }
    }

    //enable or disable character control
    private void EnableUserControl(bool active)
    {
        if (!active) //if player shouldn't controll character
        {
            m_Player.GetComponent<PlatformerCharacter2D>().Move(0f, false, false, false); //stop any character movement
            m_Player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        m_Player.GetComponent<Platformer2DUserControl>().IsCanJump = active;
        PauseMenuManager.Instance.SetIsCantOpenPauseMenu(!active);

        DisplayUI(active);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.GetComponent<Animator>().GetBool("Ground")
                && !m_IsDialogueInProgress && !m_InteractionUIButton.ActiveSelf()) //if player is near npc
        {
            m_Player = collision.transform; //get character control script
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
        m_InteractionUIButton.SetIsPlayerNear(isActive);
        m_InteractionUIButton.SetActive(isActive); //show or hide npc ui
    }

    //change state of the m_IsDialogueInProgress value
    private void ChangeDialogueInProcess(bool value)
    {
        m_IsDialogueInProgress = value;
    }

    #endregion
}

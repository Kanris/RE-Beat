using UnityEngine;
using UnityStandardAssets._2D;

public class DialogueTrigger : MonoBehaviour {

    #region public fields

    public Dialogue dialogue; //npc dialogue

    [SerializeField] private GameObject m_NPCUI;

    #endregion

    #region private fields
    
    private PlatformerCharacter2D m_Player; //player's control
    private bool m_IsDialogueInProgress; //is dialogue in progress

    #endregion

    #region Initialize

    private void Start()
    {
        DisplayUI(false); //hide npc ui

        DialogueManager.Instance.OnDialogueInProgressChange += ChangeDialogueInProcess; //watch if dialogue is started or finished
    }

    #endregion

    #region private fields

    // Update is called once per frame
    private void Update () {
		
        if (m_Player != null & MouseControlManager.IsCanUseSubmitButton()) //if player is near
        {
            if (!m_IsDialogueInProgress) //if dialogue is not in progress
            {
                if (GameMaster.Instance.m_Joystick.LeftStickY > .9f || GameMaster.Instance.m_Joystick.DPadUp.WasPressed) //if player want to start dialogue
                {
                    DisplayUI(false); //disable npc ui
                    DialogueManager.Instance.StartDialogue(transform.name, dialogue, transform, m_Player.gameObject.transform); //start dialogue

                    if (!dialogue.IsDialogueFinished) //if dialogue is not saved
                        GameMaster.Instance.SaveState<int>(gameObject.name, 0, GameMaster.RecreateType.Dialogue); //save dialogue state
                }

                if (!m_Player.enabled) //if dialogue is not in progress and player havn't character control
                {
                    EnableUserControl(true); //enable character control
                    DisplayUI(true); //show NPC ui
                }
            }
            else if (m_Player.enabled) //if dialogue in progress and player still have character control
                EnableUserControl(false); //disable character control
        }
        else if (m_NPCUI.activeSelf)
        {
            m_NPCUI.SetActive(false);
        }
    }

    //enable or disable character control
    private void EnableUserControl(bool active)
    {
        if (!active) //if player shouldn't controll character
        {
            m_Player.Move(0f, false, false, false); //stop any character movement
            m_Player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            m_Player.GetComponent<Animator>().SetBool("Ground", true);
        }

        if (m_Player != null & m_Player.enabled != active) //disable or enable character control
        {
            m_Player.GetComponent<Player>().TriggerPlayerBussy(!active);
            m_Player.enabled = active;

            PauseMenuManager.Instance.SetIsCantOpenPauseMenu(!active);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near npc
        {
            m_Player = collision.GetComponent<PlatformerCharacter2D>(); //get character control script
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
        m_NPCUI.SetActive(isActive); //show or hide npc ui
    }

    //change state of the m_IsDialogueInProgress value
    private void ChangeDialogueInProcess(bool value)
    {
        m_IsDialogueInProgress = value;
    }

    #endregion
}

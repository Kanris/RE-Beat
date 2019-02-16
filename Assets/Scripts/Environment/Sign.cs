using UnityEngine;
using UnityStandardAssets._2D;

public class Sign : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField] private string SignName; //sign name
    [SerializeField, TextArea(2, 10)] private string SignText; //sign text to display

    [Header("Additional")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton;

    #endregion
    
    private Transform m_Player; //player control
    private bool m_IsSentenceShowInProgress; //indicates is dialogue still in progress

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    private void Start () {

        m_InteractionUIButton.PressInteractionButton = StartDialogue;
        m_InteractionUIButton.SetActive(false); //hide ui

        DialogueManager.Instance.OnDialogueInProgressChange += ChangeIsInProgress; //subscribe to dialogue in progress

        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().SetFloat("Speed", Random.Range(.8f, 1.2f));
    }

    #endregion

    // Update is called once per frame
    void Update () {
		
        if (m_Player != null) //if player is near the sign
        {
            if (!m_IsSentenceShowInProgress && !m_Player.GetComponent<Platformer2DUserControl>().IsCanJump) //if dialogue is not in progress
            {
                EnableUserControl(true);
            }
        }
        else if (m_InteractionUIButton.ActiveSelf())
        {
            m_InteractionUIButton.SetActive(false);
        }
	}

    private void StartDialogue()
    {
        if (m_Player != null) //if player is near the sign
        {
            if (!m_IsSentenceShowInProgress) //if dialogue is not in progress
            {
                EnableUserControl(false);
                StartCoroutine(DialogueManager.Instance.DisplaySingleSentence(SignText, SignName, transform)); //show sign text
            }
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

        m_InteractionUIButton.SetActive(active); //hide sign ui
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (m_Player == null)
        {
            if (collision.CompareTag("Player") && collision.GetComponent<Animator>().GetBool("Ground") && !m_IsSentenceShowInProgress)
            {
                m_Player = collision.transform;

                m_InteractionUIButton.SetIsPlayerNear(true);
                m_InteractionUIButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leave sign trigger
        {
            m_InteractionUIButton.SetIsPlayerNear(false);
            m_InteractionUIButton.SetActive(false); //show or hide sign ui

            m_Player = null;
        }
    }

    private void ChangeIsInProgress(bool value)
    {
        m_IsSentenceShowInProgress = value;
    }

    #endregion
}

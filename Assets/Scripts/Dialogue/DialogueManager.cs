using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;

public class DialogueManager : MonoBehaviour {

    #region Singleton
    public static DialogueManager Instance; //singleton instance
    [SerializeField] private Audio UIClickAudio;

    private void Awake()
    {
        if (Instance != null) //if instance has reference
        {
            if (Instance != this) //if instance is not reference to this gameobject
            {
                Destroy(gameObject); //destroy this gameobject
            }
        }
        else //if instance hadn't initialized
        {
            Instance = this; //reference to this
            DontDestroyOnLoad(this); //dont destroy this gameobject on load
        }
    }
    #endregion

    #region public delegate

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnDialogueInProgressChange; //notify if dialogue is in progress or not

    #endregion

    #region private fields

    private Queue<Sentence> m_Sentences = new Queue<Sentence>(); //sentence queue
    private Dialogue m_Dialogue; //current dialogue
    private Sentence m_CurrentSentence; //current sentece
    private bool m_IsSentenceTyping; //is manager typing sentence
    private bool m_AnwswerChoose; //is player have to choose the answer
    private bool m_IsDialogueInProgress; //is dialogue in progress
    private bool m_DisplayingSingleSentence; //if displaying sign text

    #region serialize fields

    [SerializeField] private GameObject m_DialogueUI; //dialogue ui
    [SerializeField] private GameObject m_Buttons; //answer buttons
    [SerializeField] private TextMeshProUGUI m_FirstButton; //first answer ui text 
    [SerializeField] private TextMeshProUGUI m_SecondButton; //second answer ui text
    [SerializeField] private TextMeshProUGUI m_Text; //text to display sentences
    [SerializeField] private TextMeshProUGUI m_NameText; //text to display npc name
    [SerializeField] private GameObject m_NextImage; //image that notify player that sentence is over

    #endregion

    #endregion

    #region private methods

    // Update is called once per frame
    private void Update()
    {
        if (m_IsDialogueInProgress) //if is dialogue in progress
        {
            if (CrossPlatformInputManager.GetButtonDown("Jump")) //if jump button pressed
            {
                if (m_IsSentenceTyping) //if sentence is still typing
                {
                    m_IsSentenceTyping = false; //stop typying sentence and display it all
                }
                else if (!m_AnwswerChoose & !m_DisplayingSingleSentence) //if player dont have to choose the answer and sentence is display
                {
                    DisplayNextSentence(); //show next sentece

                } else if (m_DisplayingSingleSentence) //if displaying sign text
                {
                    ChangeIsDialogueInProgress(false); //dialogue is not in ptrogress
                    m_DisplayingSingleSentence = false; //dialogue is not displaying sign text

                    m_DialogueUI.SetActive(false); //hide dialogue ui
                }
            }
        }
    }

    //type sentence
    private IEnumerator TypeSentenceWithAnswer(string sentence)
    {
        yield return TypeSentence(sentence);

        if (m_AnwswerChoose) //if player have to choose the answer
        {
            SetButtonsText(m_CurrentSentence.firstAnswer, m_CurrentSentence.secondAnswer); //initialize buttons text and show buttons grid
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        m_Text.text = string.Empty; //clear main dialogue text
        m_NextImage.SetActive(false); //hide next image

        m_IsSentenceTyping = true; //notify that sentence is typing

        var typeSentence = LocalizationManager.Instance.GetDialogueLocalizedValue(sentence);

        //start type sentence
        foreach (var letter in typeSentence)
        {
            if (m_IsSentenceTyping) //if player didn't press skip button
                yield return new WaitForSeconds(0.05f); //wait 0.05s until type letter

            m_Text.text += letter; //type letter
        }

        m_IsSentenceTyping = false; //notify that sentence typing is over
        m_NextImage.SetActive(true); //show next image
    }

    private void DisplayNextSentence()
    {
        if (m_IsDialogueInProgress) //if dialogue is in progress
        {
            if (m_Sentences.Count == 0) //if queue is empty
            {
                StopDialogue(); //stop dialogue
            }
            else //if queue still have items
            {
                var currentSentenceToDisplay = m_Sentences.Dequeue(); //get next sentence to display
                
                if (!string.IsNullOrEmpty(currentSentenceToDisplay.firstAnswer)) //if player will have to choose the answers
                {
                    m_AnwswerChoose = true; //notify update that player have to choose the anser
                    m_CurrentSentence = currentSentenceToDisplay; //save reference to the current sentence
                }

                StartCoroutine(TypeSentenceWithAnswer(currentSentenceToDisplay.DisplaySentence)); //display current sentence
            }
        }
    }

    //set up answers
    private void SetButtonsText(string firstButtonText, string secondButtonText)
    {
        m_FirstButton.text = LocalizationManager.Instance.GetDialogueLocalizedValue(firstButtonText);
        m_SecondButton.text = LocalizationManager.Instance.GetDialogueLocalizedValue(secondButtonText);

        m_Buttons.SetActive(true);
    }

    //notify subscribers that dialogue in progress or not
    private void ChangeIsDialogueInProgress(bool value)
    {
        m_IsDialogueInProgress = value;

        if (OnDialogueInProgressChange != null)
            OnDialogueInProgressChange(value);
    }

    #region Sound

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null) //if audiomanager instance initialized
        {
            AudioManager.Instance.Play(UIClickAudio); //play ui click sound
        }
        else
        {
            Debug.LogError("StartScreenManager.PlayClickSound: Audiomanager.Instance is equal to null");
        }
    }

    #endregion

    private void ReInitializeDialogueQueue(Sentence[] dialogueToDisplay)
    {
        m_Sentences.Clear(); //clear sentences queue

        //save sentences to display in queue
        foreach (var sentence in dialogueToDisplay)
        {
            m_Sentences.Enqueue(sentence);
        }

        DisplayNextSentence(); //show sentence
    }

    #endregion

    #region public methods

    public void StartDialogue(string npcName, Dialogue dialogue)
    {
        if (dialogue != null) //if dialogue is not empty
        {
            ChangeIsDialogueInProgress(true); //notify that dialogue is in progress
            m_Dialogue = dialogue; //save dialogue reference
            m_NameText.text = npcName; //display npc name

            m_Sentences.Clear(); //clear sentences queue

            var dialogueToDisplay = m_Dialogue.IsDialogueFinished
                ? m_Dialogue.RepeatSentences : m_Dialogue.MainSentences; //choose sentences to display

            //save sentences to display in queue
            foreach (var sentence in dialogueToDisplay)
            {
                m_Sentences.Enqueue(sentence);
            }

            m_DialogueUI.SetActive(true); //show dialogue ui
            DisplayNextSentence(); //show sentence
        }
        else
        {
            Debug.LogError("DialogueManager.StartDialogue: Can't start empty dialogue");
        }
    }

    public IEnumerator DisplaySingleSentence(string sentence, string name)
    {
        m_DialogueUI.SetActive(true); //show dialogue ui
        ChangeIsDialogueInProgress(true); //notify that dialogue is in progress
        m_DisplayingSingleSentence = true; //notify update that single sentence is displaying
        m_NameText.text = name; //show sign text

        yield return TypeSentence(sentence); //start typing sentece
    }

    public void StopDialogue()
    {
        m_Dialogue.IsDialogueFinished = true; //save that dialogue is finished
        m_AnwswerChoose = false; //player don't have to choose the answer
        ChangeIsDialogueInProgress(false); //notify that dialogue is complete

        m_DialogueUI.SetActive(false); //hide dialogue ui
    }

    //get player answer
    public void GetAnswer(bool isFirst)
    {
        PlayClickSound(); //play button click sound

        var sentenceToStart = isFirst ? m_CurrentSentence.firstSentence : m_CurrentSentence.secondSentence; //if player pressed first button return firstSentence array; if player pressed second button return secondSentence array

        m_AnwswerChoose = false; //player had choose the answer

        ReInitializeDialogueQueue(sentenceToStart); //reinitialize queue

        m_Buttons.SetActive(false); //hide buttons
    }

    #endregion
}

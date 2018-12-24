using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour {

    #region Singleton
    public static DialogueManager Instance; //singleton instance

    [Header("Audio")]
    [SerializeField] private Audio UIClickAudio;
    [SerializeField] private Audio PrintTextAudio;

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

    [Header("UI")]
    [SerializeField] private GameObject m_DialogueUI; //dialogue ui
    [SerializeField] private Animator m_DialogueBackgroundAnimator;
    [Header("Buttons")]
    [SerializeField] private GameObject m_Buttons; //answer buttons
    [SerializeField] private GameObject m_FirstButtonGO;
    [SerializeField] private GameObject m_SecondButtonGO;
    [Header("Text")]
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
            if (GameMaster.Instance.m_Joystick.Action1.WasPressed & MouseControlManager.IsCanUseSubmitButton()) //if jump button pressed
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
                    m_DisplayingSingleSentence = false; //dialogue is not displaying sign text

                    StartCoroutine(AnimateDialogueHide());
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
            ShowAnswerButtons(m_CurrentSentence.firstAnswer, m_CurrentSentence.secondAnswer); //initialize buttons text and show buttons grid
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        m_Text.text = string.Empty; //clear main dialogue text
        m_NextImage.SetActive(false); //hide next image

        m_IsSentenceTyping = true; //notify that sentence is typing

        var typeSentence = LocalizationManager.Instance.GetDialogueLocalizedValue(sentence);

        var isTagFound = false;

        AudioManager.Instance.Play(PrintTextAudio);

        //start type sentence
        foreach (var letter in typeSentence)
        {
            if (letter == '<')
                isTagFound = true;

            if (!isTagFound)
            {
                if (m_IsSentenceTyping) //if player didn't press skip button
                    yield return new WaitForSeconds(0.05f); //wait 0.05s until type letter

                GameMaster.Instance.StartJoystickVibrate(.1f, 0.01f);
            }
            else if (letter == '>')
                isTagFound = false;

            m_Text.text += letter; //type letter
        }

        AudioManager.Instance.Stop(PrintTextAudio);

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
    private void ShowAnswerButtons(string firstButtonText, string secondButtonText)
    {
        m_FirstButton.text = LocalizationManager.Instance.GetDialogueLocalizedValue(firstButtonText);
        m_SecondButton.text = LocalizationManager.Instance.GetDialogueLocalizedValue(secondButtonText);

        m_FirstButton.color = m_SecondButton.color = m_SecondButton.color.ChangeColor(a: 0f);

        m_Buttons.SetActive(true);

        StartCoroutine(AnimateButtonsAppearance());
    }

    private IEnumerator AnimateButtonsAppearance()
    {
        m_FirstButtonGO.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        m_SecondButtonGO.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(m_FirstButton.transform.parent.gameObject);
    }

    //notify subscribers that dialogue in progress or not
    private void ChangeIsDialogueInProgress(bool value)
    {
        m_IsDialogueInProgress = value;

#if MOBILE_INPUT
        if (!value)
            MobileButtonsManager.Instance.HideOnlyNeedButtons();
#endif

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

    public void StartDialogue(string npcName, Dialogue dialogue, 
                                Transform parentTransform, Transform playerTransform)
    {
        if (dialogue != null) //if dialogue is not empty
        {
#if MOBILE_INPUT
            MobileButtonsManager.Instance.ShowOnlyNeedButtons(jump: true);
#endif
            m_Text.gameObject.SetActive(true);

            transform.position = parentTransform.position.Add(y: 2.2f);
            m_Buttons.transform.position = playerTransform.position.Add(x: 1.8f, y: 0f);

            ChangeIsDialogueInProgress(true); //notify that dialogue is in progress
            m_Dialogue = dialogue; //save dialogue reference
            m_NameText.text = "C:\\Users\\<color=yellow>" + npcName + "</color>:"; //display npc name

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

    public IEnumerator DisplaySingleSentence(string sentence, string name, Transform parentTransform)
    {

#if MOBILE_INPUT
        MobileButtonsManager.Instance.ShowOnlyNeedButtons(jump: true);
#endif

        m_Text.gameObject.SetActive(true);

        transform.position = parentTransform.position.Add(y: 2.2f);

        m_DialogueUI.SetActive(true); //show dialogue ui
        ChangeIsDialogueInProgress(true); //notify that dialogue is in progress
        m_DisplayingSingleSentence = true; //notify update that single sentence is displaying
        m_NameText.text = "C:\\Users\\<color=yellow>" + name + "</color>:"; //show sign text

        yield return TypeSentence(sentence); //start typing sentece
    }

    public void StopDialogue()
    {
        m_Dialogue.IsDialogueFinished = true; //save that dialogue is finished
        m_AnwswerChoose = false; //player don't have to choose the answer

        StartCoroutine(AnimateDialogueHide());
    }

    private IEnumerator AnimateDialogueHide()
    {
        m_DialogueBackgroundAnimator.SetTrigger("Disappear");

        m_NextImage.SetActive(false);
        m_Text.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame(); //apply disappear animation
        
        yield return new WaitForSeconds(m_DialogueBackgroundAnimator.GetCurrentAnimatorStateInfo(0).length);

        ChangeIsDialogueInProgress(false); //dialogue is not in ptrogress

        m_DialogueUI.SetActive(false); //hide dialogue ui
    }

    //get player answer
    public void GetAnswer(bool isFirst)
    {
        PlayClickSound(); //play button click sound

        var sentenceToStart = isFirst ? m_CurrentSentence.firstSentence : m_CurrentSentence.secondSentence; //if player pressed first button return firstSentence array; if player pressed second button return secondSentence array

        m_AnwswerChoose = false; //player had choose the answer

        ReInitializeDialogueQueue(sentenceToStart); //reinitialize queue

        m_FirstButtonGO.SetActive(false);
        m_SecondButtonGO.SetActive(false);
        m_Buttons.SetActive(false); //hide buttons
    }

    #endregion
}

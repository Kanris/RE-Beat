using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;

public class DialogueManager : MonoBehaviour {

    #region Singleton
    public static DialogueManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
    #endregion

    [HideInInspector] public bool isDialogueInProgress = false;

    private GameObject m_DialogueUI;
    private GameObject m_Buttons;
    private Queue<Sentence> m_Sentences = new Queue<Sentence>();
    private Dialogue m_Dialogue;
    private Sentence m_CurrentSentence;
    private Button m_FirstButton;
    private Button m_SecondButton;
    private bool m_IsSentenceTyping;
    private bool m_AnwswerChoose;

    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private TextMeshProUGUI m_NameText;
    [SerializeField] private GameObject m_NextImage;

    // Use this for initialization
    void Start () {

        InitializeDialogueUI();

        InitializeButtonsUI();

        SetActiveUI(false);

        SetActiveAnswerButtons(false);
    }

    private void InitializeDialogueUI()
    {
        if (transform.childCount > 0)
        {
            m_DialogueUI = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("DialogueManager.InitializeDialogueUI: Dialogue UI has no child.");
        }
    }

    private void InitializeButtonsUI()
    {
        if (transform.childCount > 1)
        {
            m_Buttons = transform.GetChild(1).gameObject;

            if (m_Buttons.transform.childCount > 1)
            {
                m_FirstButton = m_Buttons.transform.GetChild(0).GetComponent<Button>();
                m_SecondButton = m_Buttons.transform.GetChild(1).GetComponent<Button>();
            }
        }
        else
        {
            Debug.LogError("DialogueManager.InitializeDialogueUI: Dialogue UI has no child.");
        }
    }

    private void InitializeSentences()
    {
        m_Sentences.Clear();

        var dialogueToDisplay = m_Dialogue.IsDialogueFinished
            ? m_Dialogue.RepeatSentences : m_Dialogue.MainSentences;

        foreach (var sentence in dialogueToDisplay)
        {
            m_Sentences.Enqueue(sentence);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDialogueInProgress)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                if (m_IsSentenceTyping)
                    m_IsSentenceTyping = false;

                else if (!m_AnwswerChoose)
                    DisplayNextSentence();
            }
        }

    }

    private IEnumerator TypeSentence(string name, string sentence)
    {
        m_NameText.text = name;
        m_Text.text = "";
        SetActiveNextImage(false);

        m_IsSentenceTyping = true;

        foreach (var letter in sentence)
        {
            if (m_IsSentenceTyping)
                yield return new WaitForSeconds(0.05f);

            m_Text.text += letter;
        }


        if (m_AnwswerChoose)
        {
            SetActiveAnswerButtons(true);
            SetButtonsText(m_CurrentSentence.firstAnswer, m_CurrentSentence.secondAnswer);
        }

        m_IsSentenceTyping = false;
        SetActiveNextImage(true);
    }

    private string GetName(Sentence sentence)
    {
        return string.IsNullOrEmpty(sentence.Name) ? "Stranger" : sentence.Name;
    }

    private void SetActiveUI(bool isActive)
    {
        if (m_DialogueUI != null)
        {
            m_DialogueUI.SetActive(isActive);
        }
        else
            Debug.LogError("DialogueManager.SetActiveUI: m_DialogueUI is not initialized");
    }

    private void SetActiveNextImage(bool isActive)
    {
        m_NextImage.SetActive(isActive);
    }

    private void DialogueComplete()
    {
        m_Dialogue.IsDialogueFinished = true;
        StopDialogue();
    }

    private void DisplayNextSentence()
    {
        if (isDialogueInProgress)
        {
            if (m_Sentences.Count == 0)
            {
                DialogueComplete();
                return;
            }

            var sentence = m_Sentences.Dequeue();

            StopAllCoroutines();
            if (!string.IsNullOrEmpty(sentence.firstAnswer))
            {
                m_AnwswerChoose = true;
                m_CurrentSentence = sentence;
            }
            else
                SetActiveAnswerButtons(false);

            StartCoroutine(TypeSentence(GetName(sentence), sentence.DisplaySentence));
        }
    }

    private void SetActiveAnswerButtons(bool isActive)
    {
        m_Buttons.SetActive(isActive);
    }

    private void SetButtonsText(string button1, string button2)
    {
        m_FirstButton.GetComponentInChildren<Text>().text = button1;
        m_SecondButton.GetComponentInChildren<Text>().text = button2;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue != null)
        {
            isDialogueInProgress = true;
            m_Dialogue = dialogue;

            InitializeSentences();

            SetActiveUI(true);
            DisplayNextSentence();
        }
        else
        {
            Debug.LogError("DialogueManager.StartDialogue: Can't start empty dialogue");
        }
    }


    public void StartDialogue(Sentence[] dialogueToDisplay)
    {
        isDialogueInProgress = true;
        m_Sentences.Clear();

        foreach (var sentence in dialogueToDisplay)
        {
            m_Sentences.Enqueue(sentence);
        }

        SetActiveUI(true);
        DisplayNextSentence();
    }

    public void StopDialogue()
    {
        m_AnwswerChoose = false;
        isDialogueInProgress = false;

        SetActiveAnswerButtons(false);
        SetActiveUI(false);
    }

    public void GetAnswer(bool isFirst)
    {
        SetActiveAnswerButtons(false);

        var sentenceToStart = isFirst ? m_CurrentSentence.firstSentence : m_CurrentSentence.secondSentence;

        m_AnwswerChoose = false;

        StartDialogue(sentenceToStart);
    }
}

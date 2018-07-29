using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public bool isDialogueInProgress = false;

    private GameObject m_DialogueUI;
    private GameObject m_Buttons;
    private Queue<Sentence> sentences = new Queue<Sentence>();
    private Dialogue m_Dialogue;
    private Sentence m_CurrentSentence;
    private bool m_IsSentenceTyping;
    private bool m_AnwswerChoose;

    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private TextMeshProUGUI m_NameText;
    [SerializeField] private GameObject m_NextImage;
    [SerializeField] private Button m_FirstButton;
    [SerializeField] private Button m_SecondButton;


    // Use this for initialization
    void Start () {

        InitializeDialogueUI();

        InitializeButtonsUI();

        SetActiveUI(false);

        SetActiveButtons(false);
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
        }
        else
        {
            Debug.LogError("DialogueManager.InitializeDialogueUI: Dialogue UI has no child.");
        }
    }
	
	// Update is called once per frame
	void Update () {
		
        if (isDialogueInProgress)
        {
            if (Input.GetMouseButtonDown(0) & !m_AnwswerChoose)
            {
                if (m_IsSentenceTyping)
                    m_IsSentenceTyping = false;

                else 
                    DisplayNextSentence();
            }
        }

	}

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue != null)
        {
            isDialogueInProgress = true;
            m_Dialogue = dialogue;
            sentences.Clear();

            var dialogueToDisplay = m_Dialogue.IsDialogueFinished 
                ? m_Dialogue.RepeatSentences : m_Dialogue.MainSentences;

            foreach (var sentence in dialogueToDisplay)
            {
                sentences.Enqueue(sentence);
            }

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
        sentences.Clear();

        foreach (var sentence in dialogueToDisplay)
        {
            sentences.Enqueue(sentence);
        }

        SetActiveUI(true);
        DisplayNextSentence();
    }

    private void DisplayNextSentence()
    {
        if (isDialogueInProgress)
        {
            if (sentences.Count == 0)
            {
                DialogueComplete();
                return;
            }

            var sentence = sentences.Dequeue();

            StopAllCoroutines();
            if (!string.IsNullOrEmpty(sentence.firstAnswer))
            {
                m_AnwswerChoose = true;
                SetActiveButtons(true);
                m_CurrentSentence = sentence;
                SetButtonsText(sentence.firstAnswer, sentence.secondAnswer);
            }
            else
                SetActiveButtons(false);

            StartCoroutine(TypeSentence(GetName(sentence), sentence.DisplaySentence));
        }
    }

    private IEnumerator TypeSentence(string name, string sentence)
    {
        m_NameText.text = name;
        m_Text.text = "";
        SetActiveImage(false);

        m_IsSentenceTyping = true;
        foreach ( var letter in sentence )
        {
            if (m_IsSentenceTyping)
                yield return new WaitForSeconds(0.05f);

            m_Text.text += letter;
        }

        SetActiveImage(true);
        m_IsSentenceTyping = false;
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

    private void SetActiveImage(bool isActive)
    {
        m_NextImage.SetActive(isActive);
    }

    private void DialogueComplete()
    {        
        m_Dialogue.IsDialogueFinished = true;
        StopDialogue();
    }

    public void StopDialogue()
    {
        m_AnwswerChoose = false;
        isDialogueInProgress = false;

        SetActiveButtons(false);
        SetActiveUI(false);
    }

    private void SetActiveButtons(bool isActive)
    {
        m_Buttons.SetActive(isActive);
    }

    private void SetButtonsText(string button1, string button2)
    {
        m_FirstButton.GetComponentInChildren<Text>().text = button1;
        m_SecondButton.GetComponentInChildren<Text>().text = button2;
    }

    public void GetAnswer(bool isFirst)
    {
        SetActiveButtons(false);

        var sentenceToStart = isFirst ? m_CurrentSentence.firstSentence : m_CurrentSentence.secondSentence;

        m_AnwswerChoose = false;

        StartDialogue(sentenceToStart);
    }
}

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
    private Queue<Sentence> sentences = new Queue<Sentence>();
    private DialogueTrigger m_DialogueTrigger;
    private bool m_IsSentenceTyping;
    private bool m_NextString;
    private string m_NPCName;

    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private TextMeshProUGUI m_NameText;
    [SerializeField] private GameObject m_NextImage;

    // Use this for initialization
    void Start () {

        InitializeDialogueUI();

        SetActiveUI(false);
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
	
	// Update is called once per frame
	void Update () {
		
        if (isDialogueInProgress)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (m_IsSentenceTyping)
                    m_IsSentenceTyping = false;

                else if (m_NextString)
                    DisplayNextSentence();

                else
                    m_NextString = true;
            }
        }

	}

    public void StartDialogue(Dialogue dialogue, DialogueTrigger trigger)
    {
        if (dialogue != null)
        {
            isDialogueInProgress = true;
            m_DialogueTrigger = trigger;
            sentences.Clear();

            foreach (var sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }

            m_NPCName = string.IsNullOrEmpty(dialogue.Name) ? "Stranger" : dialogue.Name;
            SetActiveUI(true);
            DisplayNextSentence();
        }
        else
        {
            Debug.LogError("DialogueManager.StartDialogue: Can't start empty dialogue");
        }
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
            StartCoroutine(DisplaySentence(sentence));
        }
    }

    private IEnumerator DisplaySentence(Sentence sentence)
    {
        m_NextString = false;

        if (sentence.isPlayerFirst)
        {
            yield return DisplaySentence(PlayerStats.PlayerName, sentence.Player, m_NPCName, sentence.NPC);
        }
        else
        {
            yield return DisplaySentence(m_NPCName, sentence.NPC, PlayerStats.PlayerName, sentence.Player);
        }

        m_NextString = true;
    }

    private IEnumerator DisplaySentence(string firstName, string firstSentence, string secondName, string secondSentence)
    {
        if (!string.IsNullOrEmpty(firstSentence))
            yield return TypeSentence(firstName, firstSentence);

        while (!m_NextString & !string.IsNullOrEmpty(secondSentence))
            yield return null;

        if (!string.IsNullOrEmpty(secondSentence))
            yield return TypeSentence(secondName, secondSentence);
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
        m_DialogueTrigger.m_isDialogueFinished = true;

        StopDialogue();
    }

    public void StopDialogue()
    {
        isDialogueInProgress = false;
        SetActiveUI(false);
    }
}

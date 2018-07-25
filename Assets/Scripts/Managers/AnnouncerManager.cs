using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnnouncerManager : MonoBehaviour {

    public class Message
    {
        public string message = "Empty message";
        public float time = 1.5f;

        public Message() { }

        public Message(string message, float time = 1.5f)
        {
            this.message = message;
            this.time = time;
        }
    }

    #region Singleton
    public static AnnouncerManager Instance;

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

    private GameObject m_UI;
    private TextMeshProUGUI m_Text;
    private List<Message> m_MessagePipeline;
    private bool m_isShowingPipeline = false;

    // Use this for initialization
    void Start () {

        InitializeUI();

        InitializeText();

        if (m_UI != null) ActiveAnnouncerUI(false);

        m_MessagePipeline = new List<Message>();
    }

    private void InitializeUI()
    {
        if (transform.GetChild(0) != null)
        {
            m_UI = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("AnnouncerManager.InitializeUI: Can't find child");
        }
    }

    private void InitializeText()
    {
        if (m_UI != null)
        {
            m_Text = m_UI.GetComponentInChildren<TextMeshProUGUI>();

            if (m_Text == null)
            {
                Debug.LogError("AnnouncerManager.InitializeText: Can't find TextMeshProUGUI in m_UI child");
            }
        }
        else
        {
            Debug.LogError("AnnouncerManager.InitializeText: Can't initialize text, because m_UI is equals to null");
        }
    }
	
	public void DisplayAnnouncerMessage(Message message)
    {
        m_MessagePipeline.Add(message);

        if (!m_isShowingPipeline)
        {
            m_isShowingPipeline = true;
            StartCoroutine(DisplayMessage());
        }
    }

    private IEnumerator DisplayMessage()
    {
        if (!m_UI.activeSelf)
            ActiveAnnouncerUI(true);

        var itemToDisplay = m_MessagePipeline[0];

        m_Text.text = itemToDisplay.message;

        yield return new WaitForSeconds(itemToDisplay.time);

        m_MessagePipeline.RemoveAt(0);

        if (m_MessagePipeline.Count != 0)
            StartCoroutine(DisplayMessage());
        else
        {
            m_isShowingPipeline = false;
            ActiveAnnouncerUI(false);
        }
    }

    private void ActiveAnnouncerUI(bool active)
    {
        m_UI.SetActive(active);
    }
}

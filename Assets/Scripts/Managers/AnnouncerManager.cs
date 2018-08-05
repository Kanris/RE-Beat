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

        #region Initialize

        InitializeUI();

        InitializeMessageAnnouncer();

        InitializeSceneAnnouncer();

        #endregion
    }
    #endregion

    private GameObject m_MessageAnnouncer;
    private TextMeshProUGUI m_TextMessage;
    private GameObject m_SceneAnnouncer;
    private TextMeshProUGUI m_TextScene;

    private GameObject m_UI;
    private List<Message> m_MessagePipeline;
    private bool m_isShowingPipeline = false;

    // Use this for initialization
    void Start () {

        ActiveSceneAnnouncer(false);

        ActiveMessageAnnouncer(false);

        if (m_UI != null) ActiveAnnouncerUI(false);

        m_MessagePipeline = new List<Message>();
    }

    #region Initialize

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

    private void InitializeMessageAnnouncer()
    {
        if (m_UI.transform.childCount > 1)
        {
            m_MessageAnnouncer = m_UI.transform.GetChild(0).gameObject;

            m_TextMessage = m_MessageAnnouncer.GetComponentInChildren<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("AnnouncerManager.InitializeMessageAnnouncer: m_UI has only one children");
        }
    }

    private void InitializeSceneAnnouncer()
    {
        if (m_UI.transform.childCount > 1)
        {
            m_SceneAnnouncer = m_UI.transform.GetChild(1).gameObject;

            m_TextScene = m_SceneAnnouncer.GetComponentInChildren<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("AnnouncerManager.InitializeSceneAnnouncer: m_UI has only one children");
        }
    }

    #endregion

    public void DisplayAnnouncerMessage(Message message)
    {
        m_MessagePipeline.Add(message);

        if (!m_isShowingPipeline)
        {
            m_isShowingPipeline = true;
            StartCoroutine(DisplayMessage());
        }
    }

    public void DisplaySceneName(string sceneName)
    {
        StartCoroutine(DisplayScene(sceneName));
    }

    private IEnumerator DisplayScene(string sceneName, float timeToDisplay = 2f)
    {
        ActiveAnnouncerUI(true);
        ActiveSceneAnnouncer(true);

        m_TextScene.text = sceneName;

        yield return new WaitForSeconds(timeToDisplay);

        ActiveSceneAnnouncer(false);
        ActiveAnnouncerUI(false);
    }

    private IEnumerator DisplayMessage()
    {
        if (!m_UI.activeSelf)
        {
            ActiveAnnouncerUI(true);
        }

        if (!m_MessageAnnouncer.activeSelf)
            ActiveMessageAnnouncer(true);

        var itemToDisplay = m_MessagePipeline[0];

        m_TextMessage.text = itemToDisplay.message;

        yield return new WaitForSeconds(itemToDisplay.time);

        m_MessagePipeline.RemoveAt(0);

        if (m_MessagePipeline.Count != 0)
            StartCoroutine(DisplayMessage());
        else
        {
            m_isShowingPipeline = false;
            ActiveAnnouncerUI(false);
            ActiveMessageAnnouncer(false);
        }
    }

    private void ActiveAnnouncerUI(bool active)
    {
        m_UI.SetActive(active);
    }

    private void ActiveMessageAnnouncer(bool active)
    {
        m_MessageAnnouncer.SetActive(active);
    }

    private void ActiveSceneAnnouncer(bool active)
    {
        m_SceneAnnouncer.SetActive(active);
    }
}

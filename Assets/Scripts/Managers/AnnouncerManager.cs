using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnnouncerManager : MonoBehaviour {

    #region message class

    public class Message
    {
        public string message = "Empty message"; //message to display
        public float time = 1.5f; //time to display

        public Message() { }

        public Message(string message, float time = 1.5f)
        {
            this.message = message;
            this.time = time;
        }
    }

    #endregion

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

    #region private fields

    #region serialize fields

    [SerializeField] private GameObject m_UI; //announcer ui

    [SerializeField] private GameObject m_MessageAnnouncer;
    [SerializeField] private TextMeshProUGUI m_TextMessage;

    [SerializeField] private GameObject m_SceneAnnouncer;
    [SerializeField] private TextMeshProUGUI m_TextScene;

    #endregion

    private List<Message> m_MessagePipeline; //display message pipeline
    private bool m_isShowingPipeline = false; //is currently showing pipeline

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    private void Start () {

        ActiveSceneAnnouncer(false); //hide scene announcer
         
        ActiveMessageAnnouncer(false); //hide message announcer

        ActiveAnnouncerUI(false); //hide ui

        m_MessagePipeline = new List<Message>(); //initialize pipeline
    }

    #endregion

    private IEnumerator DisplayScene(string sceneName, float timeToDisplay = 1f)
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

    #endregion

    #region public methods

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

    #endregion
}

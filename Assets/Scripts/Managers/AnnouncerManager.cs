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
    
    [SerializeField] private GameObject m_MessageAnnouncer;
    [SerializeField] private TextMeshProUGUI m_TextMessage;

    [SerializeField] private TextMeshProUGUI m_TextScene;

    #endregion

    private GameObject m_SceneAnnouncer;
    private List<Message> m_MessagePipeline; //display message pipeline
    private bool m_isShowingPipeline = false; //is currently showing pipeline

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    private void Start () {

        m_SceneAnnouncer = m_TextScene.gameObject;

        m_TextScene.text = GameMaster.Instance.SceneName;

        ActiveMessageAnnouncer(false); //hide message announcer

        ActiveSceneAnnouncer(false);

        m_MessagePipeline = new List<Message>(); //initialize pipeline
    }

    #endregion

    private IEnumerator DisplayScene(string sceneName, float timeToDisplay = 3f)
    {
        ActiveSceneAnnouncer(true);

        m_TextScene.text = sceneName;

        yield return new WaitForSeconds(timeToDisplay);

        ActiveSceneAnnouncer(false);
    }

    private IEnumerator DisplayMessage()
    {
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
            ActiveMessageAnnouncer(false);
        }
    }

    private void ActiveMessageAnnouncer(bool active)
    {
        m_MessageAnnouncer.SetActive(active);
    }

    private void ActiveSceneAnnouncer(bool active)
    {
        if (active)
            StartCoroutine(SceneTextAppearance());
        else
            StartCoroutine(SceneTextDisappearance());
    }

    private IEnumerator SceneTextAppearance()
    {
        var value = 0f;

        m_SceneAnnouncer.SetActive(true);

        while (m_TextScene.color.a < 1)
        {
            m_TextScene.color = m_TextScene.color.ChangeColor(a: value);

            yield return new WaitForSeconds(0.05f);

            value += 0.05f;
        }
    }

    private IEnumerator SceneTextDisappearance()
    {
        var value = 1f;

        while (m_TextScene.color.a > 0)
        {
            m_TextScene.color = m_TextScene.color.ChangeColor(a: value);

            yield return new WaitForSeconds(0.1f);

            value -= 0.05f;
        }
        
        m_SceneAnnouncer.SetActive(false);
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
        StopAllCoroutines();
        StartCoroutine(DisplayScene(sceneName));
    }

    #endregion
}

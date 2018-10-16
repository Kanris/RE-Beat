using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnnouncerManager : MonoBehaviour {

    #region message class

    public class Message
    {
        public enum MessageType { Item, Scene, Task, Message }

        public string message = "Empty message"; //message to display
        public float time = 1.5f; //time to display

        public Color color;
        public MessageType messageType;

        public Message(string message, MessageType messageType, float time = 1.5f)
        {
            this.message = message;
            this.time = time;
            this.messageType = messageType;

            SetColor(this.messageType);
        }

        private void SetColor(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Item:
                    ColorUtility.TryParseHtmlString("#FF0800", out color);
                    break;

                case MessageType.Message:
                    ColorUtility.TryParseHtmlString("#FF7500", out color);
                    break;

                case MessageType.Scene:
                    ColorUtility.TryParseHtmlString("#DE00FF", out color);
                    break;

                case MessageType.Task:
                    ColorUtility.TryParseHtmlString("#FF0069", out color);
                    break;
            }

            color = color.ChangeColor(a: 0.8f);
        }

        public void PlayNotificationSound()
        {
            AudioManager.Instance.Play("Noti" + this.messageType);
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
    
    [SerializeField] private GameObject m_UI;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private Animator m_UIAnimator;

    #endregion
        
    private List<Message> m_MessagePipeline; //display message pipeline
    private bool m_isShowingPipeline = false; //is currently showing pipeline
    private float m_Speed = 2.0f;
    private float m_StartTime;
    private Color m_StartColor;
    private Color m_EndColor;

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    private void Start () {

        StartCoroutine( SetActiveUI(false) );

        m_MessagePipeline = new List<Message>(); //initialize pipeline
    }

    #endregion

    private void Update()
    {
        if (m_isShowingPipeline) //color change animation
        {
            var t = (Time.time - m_StartTime) * m_Speed;
            m_UI.GetComponent<Image>().color = Color.Lerp(m_StartColor, m_EndColor, t);
        }
    }

    private IEnumerator DisplayMessage()
    {
        if (!m_UI.activeSelf)
            yield return SetActiveUI(true);

        var itemToDisplay = m_MessagePipeline[0];

        //color animation
        m_StartColor = m_UI.GetComponent<Image>().color;
        m_EndColor = itemToDisplay.color;
        m_StartTime = Time.time;

        itemToDisplay.PlayNotificationSound();
        m_Text.text = itemToDisplay.message;

        yield return new WaitForSeconds(itemToDisplay.time);

        m_MessagePipeline.RemoveAt(0);

        if (m_MessagePipeline.Count != 0)
        {
            yield return SetActiveUI(false);
            yield return DisplayMessage();
        }
        else
        {
            m_isShowingPipeline = false;
            yield return SetActiveUI(false);
        }
    }

    private IEnumerator SetActiveUI(bool active)
    {
        if (!active)
        {
            m_Text.gameObject.SetActive(false);

            m_UIAnimator.SetTrigger("Disappear");

            yield return new WaitForEndOfFrame();

            yield return new WaitForSeconds(m_UIAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        m_UI.SetActive(active);
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

    #endregion
}

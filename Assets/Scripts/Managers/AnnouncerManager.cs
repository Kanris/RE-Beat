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
        Instance = this;
    }
    #endregion

    #region private fields

    #region serialize fields
    
    [Header("Notification")]
    [SerializeField] private GameObject m_NotificationUI;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private Animator m_UIAnimator;

    [Header("Scrap")]
    [SerializeField] private GameObject m_ScrapUI;
    [SerializeField] private TextMeshProUGUI AmountText; //current coins amount
    [SerializeField] private TextMeshProUGUI AddScrapText; //coins to add

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

        PlayerStats.OnScrapAmountChange += ChangeScrapAmount; //subscribe on coins amount change

        StartCoroutine( SetActiveNotificationUI(false) );

        SetAciveScrapUI(false);

        m_MessagePipeline = new List<Message>(); //initialize pipeline
    }

    #endregion

    private void Update()
    {
        if (m_isShowingPipeline) //color change animation
        {
            var t = (Time.time - m_StartTime) * m_Speed;
            m_NotificationUI.GetComponent<Image>().color = Color.Lerp(m_StartColor, m_EndColor, t);
        }
    }

    private void OnDestroy()
    {
        PlayerStats.OnScrapAmountChange -= ChangeScrapAmount; //subscribe on coins amount change
    }

    private IEnumerator DisplayMessage()
    {
        if (!m_NotificationUI.activeSelf)
            yield return SetActiveNotificationUI(true);

        var itemToDisplay = m_MessagePipeline[0];

        //color animation
        m_StartColor = m_NotificationUI.GetComponent<Image>().color;
        m_EndColor = itemToDisplay.color;
        m_StartTime = Time.time;

        itemToDisplay.PlayNotificationSound();
        m_Text.text = itemToDisplay.message;

        yield return new WaitForSeconds(itemToDisplay.time);

        m_MessagePipeline.RemoveAt(0);

        if (m_MessagePipeline.Count != 0)
        {
            yield return SetActiveNotificationUI(false);
            yield return DisplayMessage();
        }
        else
        {
            m_isShowingPipeline = false;
            yield return SetActiveNotificationUI(false);
        }
    }

    private IEnumerator SetActiveNotificationUI(bool active)
    {
        if (!active)
        {
            m_Text.gameObject.SetActive(false);

            m_UIAnimator.SetTrigger("Disappear");

            yield return new WaitForEndOfFrame();

            yield return new WaitForSeconds(m_UIAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        m_NotificationUI.SetActive(active);
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

    #region scrap

    public void ChangeScrapAmount(int value)
    {
        if (value > 0)
        {
            StartCoroutine(DisplayChangeAmount(value, '+', 1));
        }
        else
        {
            StartCoroutine(DisplayChangeAmount(value, '-', -1, true));
        }
    }

    private IEnumerator DisplayChangeAmount(int value, char sign, int val, bool displayAmount = false)
    {
        var currentCoinsCount = PlayerStats.Scrap - value;

        if (value < 0)
        {
            value *= -1;
            currentCoinsCount = PlayerStats.Scrap + value;
        }

        var addAmount = value;

        SetAciveScrapUI(true);

        AddScrapText.gameObject.SetActive(true);
        AddScrapText.text = sign + value.ToString();
        AmountText.text = currentCoinsCount.ToString();

        yield return new WaitForSeconds(0.5f);

        for (int index = 0; index < value; index++)
        {
            currentCoinsCount += val;
            AmountText.text = currentCoinsCount.ToString();

            addAmount -= 1;
            AddScrapText.text = sign + addAmount.ToString();

            yield return new WaitForSeconds(0.001f);
        }

        yield return new WaitForSeconds(1f);

        AddScrapText.gameObject.SetActive(false);

        if (!displayAmount) SetAciveScrapUI(false);
    }

    public void ShowScrapAmount(bool value)
    {
        SetAciveScrapUI(value);
        AmountText.text = PlayerStats.Scrap.ToString();
        AddScrapText.gameObject.SetActive(false);
    }

    private void SetAciveScrapUI(bool value)
    {
        m_ScrapUI.SetActive(value);
    }

    #endregion
}

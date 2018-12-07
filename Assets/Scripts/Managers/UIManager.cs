using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {

    #region message class

    public class Message
    {
        public enum MessageType { Item, Scene, Task, Message }

        public string message = "Empty message"; //message to display
        public float duration; //time to display

        public Color color;
        public MessageType messageType;

        public Message(string message, MessageType messageType, float duration = 3f)
        {
            this.message = message;
            this.duration = duration;
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
    public static UIManager Instance;

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

    [Header("Upper")]
    [SerializeField] private GameObject m_LifePanel; //life panel
    [SerializeField] private Image m_BulletImage; //bullet display cooldown

    [Header("Scrap")]
    [SerializeField] private TextMeshProUGUI m_AmountText; //current coins amount
    [SerializeField] private TextMeshProUGUI m_AddScrapText; //coins to add

    [Header("Notification")]
    [SerializeField] private GameObject m_NotificationUI; //notification ui
    [SerializeField] private TextMeshProUGUI m_Text; //notification text
    [SerializeField] private Animator m_Animator; //notification animator

    #endregion

    private List<GameObject> m_HealthInPanel = new List<GameObject>(); //
    private int m_CurrentActiveHPIndex = 0;

    //notification fields
    private List<Message> m_MessagePipeline; //display message pipeline
    private bool m_isShowingPipeline = false; //is currently showing pipeline

    #endregion

    #region initialize

    private void Start()
    {
        InitializeHealList();

        PlayerStats.OnScrapAmountChange += ChangeScrapAmount; //subscribe on coins amount change

        m_MessagePipeline = new List<Message>(); //initialize pipeline

        m_AmountText.text = PlayerStats.Scrap.ToString(); //display current scrap amount
    }
    #endregion

    #region notification 

    private IEnumerator DisplayMessage()
    {
        if (!m_NotificationUI.activeSelf) //play appear animation
            yield return SetActiveNotificationUI(true);

        var itemToDisplay = m_MessagePipeline[0]; //get firs item in Queue

        itemToDisplay.PlayNotificationSound(); //play notification sound

        m_NotificationUI.GetComponent<Image>().color = itemToDisplay.color; //change notification color
        m_Text.text = itemToDisplay.message; //change notification text

        yield return new WaitForSeconds(itemToDisplay.duration); //display need amount time

        m_MessagePipeline.RemoveAt(0); //remove displayed item from queue

        if (m_MessagePipeline.Count != 0) //if there is items in queue
        {
            //display next notification
            yield return SetActiveNotificationUI(false);
            yield return DisplayMessage();
        }
        else //if there is not items in queue
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

            m_Animator.SetTrigger("Disappear");

            yield return new WaitForEndOfFrame();

            yield return new WaitForSeconds(m_Animator.GetCurrentAnimatorStateInfo(0).length);
        }

        m_NotificationUI.SetActive(active);
    }


    public void DisplayNotificationMessage(Message message)
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
        StartCoroutine(DisplayChangeAmount(value));
    }

    private IEnumerator DisplayChangeAmount(int value)
    {
        var currentCoinsCount = Convert.ToInt32( m_AmountText.text ); //current scrap amount displayed

        var sign = value > 0 ? '+' : '-'; //draw add amoun sign
        var addValue = value > 0 ? 1 : -1; //add value for loop

        value = Mathf.Abs(value);

        m_AddScrapText.gameObject.SetActive(true); //display add amount text
        m_AddScrapText.text = sign + value.ToString(); //display amount that will be added

        yield return new WaitForSeconds(.5f); //time before start add amount

        //display add animation
        for (; value > 0; value--)
        {
            currentCoinsCount += addValue;
            m_AmountText.text = currentCoinsCount.ToString();

            m_AddScrapText.text = sign + value.ToString();

            yield return new WaitForSeconds(.03f);
        }

        m_AddScrapText.text = sign + value.ToString(); //show zero add value at the end

        yield return new WaitForSeconds(.5f); //wait before hide add text

        m_AddScrapText.gameObject.SetActive(false);
    }

    #endregion

    #region upper panel

    private void InitializeHealList()
    {
        for (var index = 0; index < m_LifePanel.transform.childCount; index++)
        {
            m_HealthInPanel.Add(m_LifePanel.transform.GetChild(index).gameObject);
        }

        m_CurrentActiveHPIndex = m_HealthInPanel.Count - 1;
    }

    #region public methods

    public void AddHealth()
    {
        if (m_CurrentActiveHPIndex <= 5)
        {
            m_HealthInPanel[m_CurrentActiveHPIndex].GetComponent<Animator>().SetBool("Disable", false);

            if (m_CurrentActiveHPIndex < 5)
                m_CurrentActiveHPIndex++;

        }
    }

    public void RemoveHealth()
    {
        if (m_CurrentActiveHPIndex >= 0)
        {
            m_HealthInPanel[m_CurrentActiveHPIndex].GetComponent<Animator>().SetBool("Disable", true);

            if (m_CurrentActiveHPIndex > 0)
                m_CurrentActiveHPIndex--;
        }
    }

    public void ResetState()
    {
        for (var index = 0; index < m_HealthInPanel.Count; index++)
        {
            m_HealthInPanel[index].GetComponent<Animator>().SetBool("Disable", false);
        }

        m_CurrentActiveHPIndex = m_HealthInPanel.Count - 1;
    }

    #region bullet

    public void BulletCooldown(float cooldown)
    {
        StartCoroutine(DisplayBulletCooldown(cooldown));
    }

    private IEnumerator DisplayBulletCooldown(float time)
    {
        m_BulletImage.fillAmount = 0f;

        var tickTime = time * .1f;

        while (m_BulletImage.fillAmount < 1)
        {
            yield return new WaitForSeconds(tickTime);
            m_BulletImage.fillAmount += .1f;
        }
    }

    #endregion

    #endregion

    #endregion
}

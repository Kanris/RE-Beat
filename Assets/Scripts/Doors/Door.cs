using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
public class Door : MonoBehaviour {

    #region enum

    public enum DoorType { Key, SwitchOrButton } //door types

    #endregion

    #region private fields

    #region serialize fields

    [Header("How to open")]
    [SerializeField] private DoorType Type; //current door type

    [Header("Item to open")]
    [SerializeField] private Item KeyName; //key that have to open the door (if door type is key)

    [Header("Announcer message")]
    [SerializeField] private string DisplayMessage; //display message if door is close

    [Header("Effects")]
    [SerializeField] private Audio DoorOpenAudio;
    [SerializeField] private GameObject m_UI; //interaction button ui (needed only if door type is key)

    #endregion

    private Animator m_Animator; //reference to the gameobject animator
    private float m_TimeBetweenShowMessage;

    #endregion

    #region private methods

    private void Start()
    {
        m_Animator = GetComponent<Animator>(); //get gameobject animator

        InitializeInteractionButton(); //initialize door ui
    }

    private void InitializeInteractionButton()
    {
        if (Type == DoorType.Key) //if door type is Key
        {
            m_UI.SetActive(false); //hide door ui
        }
    }

    private void Update()
    {
        if (Type == DoorType.Key) //if door is have to open with key
        {
            if (m_UI.activeSelf & !PauseMenuManager.IsPauseManagerActive()) //if player is near door
            {
                if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player pressed submit button
                {
                    OpenDoorWithKey(); //try to open the door
                }

                if (GameMaster.Instance.IsPlayerDead)
                {
                    m_UI.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & Type == DoorType.Key) //if player is in trigger
        {
            m_UI.SetActive(true); //show door ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & Type == DoorType.Key) //if player is leaving trigger
        {
            m_UI.SetActive(false); //hide door ui
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) //if player is in collision
        {
            var displayMessage = LocalizationManager.Instance.GetItemsLocalizedValue(DisplayMessage);

            if (m_TimeBetweenShowMessage < Time.time)
            {
                m_TimeBetweenShowMessage = Time.time + 2f;
                ShowAnnouncerMessage(displayMessage); //show tip
            }
        }
    }

    private void OpenDoorWithKey()
    {
        if (string.IsNullOrEmpty(KeyName.itemDescription.Name)) //if there is not key name
        {
            Destroy(gameObject); //open the door
            GameMaster.Instance.SaveState<int>(gameObject.name, 0, GameMaster.RecreateType.Object); //save object state
        }
        else if (PlayerStats.PlayerInventory.IsInBag(KeyName.itemDescription.Name)) //if there is key name and player have it in the bag
        {
            if (PlayerStats.PlayerInventory.Remove(KeyName)) //remove key from the inventory
                ShowAnnouncerMessage(LocalizationManager.Instance.GetItemsLocalizedValue(KeyName.itemDescription.Name) + " " + LocalizationManager.Instance.GetItemsLocalizedValue("door_notification_key")); //display announcer message that key was removed from the bag

            GameMaster.Instance.SaveState<int>(gameObject.name, 0, GameMaster.RecreateType.Object); //save object state

            Destroy(gameObject); //open the door
        }
        else if (m_TimeBetweenShowMessage < Time.time)
        {
            m_TimeBetweenShowMessage = Time.time + 2f;//if player havn't needed key
            ShowAnnouncerMessage(LocalizationManager.Instance.GetItemsLocalizedValue(KeyName.itemDescription.Name) + " " + LocalizationManager.Instance.GetItemsLocalizedValue("door_notification")); //display message that key is required
        }
    }

    private void ShowAnnouncerMessage(string messageToDisplay)
    {
        if (!string.IsNullOrEmpty(messageToDisplay))
            UIManager.Instance.DisplayNotificationMessage(messageToDisplay, 
                UIManager.Message.MessageType.Message);
    }

    private void PlayAnimation(string name)
    {
        m_Animator.SetTrigger(name);
    }

    #endregion

    #region public methods

    public void PlayOpenDoorAnimation()
    {
        AudioManager.Instance.Play(DoorOpenAudio);
        PlayAnimation("Open");
    }

    public void PlayCloseDoorAnimation()
    {
        PlayAnimation("Close");
    }

    #endregion
}

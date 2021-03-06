﻿using UnityEngine;

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

    [Header("Additional")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton; //interaction button ui (needed only if door type is key)

    #endregion

    private Animator m_Animator; //reference to the gameobject animator
    private float m_TimeBetweenShowMessage;

    #endregion

    #region private methods

    private void Start()
    {
        m_Animator = GetComponent<Animator>(); //get gameobject animator

        if (m_InteractionUIButton != null)
        {
            m_InteractionUIButton.PressInteractionButton = OpenDoorWithKey;
            m_InteractionUIButton.SetActive(false); //initialize door ui
        }
    }

    private void Update()
    {
        if (Type == DoorType.Key) //if door is have to open with key
        {
            if (m_InteractionUIButton.ActiveSelf()) //if player is near door
            {
                if (GameMaster.Instance.IsPlayerDead)
                {
                    m_InteractionUIButton.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Type == DoorType.Key && !m_InteractionUIButton.ActiveSelf()) //if player is in trigger
        {
            m_InteractionUIButton.SetIsPlayerNear(true);
            m_InteractionUIButton.SetActive(true); //show door ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Type == DoorType.Key) //if player is leaving trigger
        {
            m_InteractionUIButton.SetIsPlayerNear(false);
            m_InteractionUIButton.SetActive(false); //hide door ui
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Type != DoorType.Key) //if player is in collision
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
        if (KeyName != null) 
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
        if (DoorOpenAudio != null)
            AudioManager.Instance.Play(DoorOpenAudio);

        PlayAnimation("Open");
    }

    public void PlayCloseDoorAnimation()
    {
        PlayAnimation("Close");
    }

    #endregion
}

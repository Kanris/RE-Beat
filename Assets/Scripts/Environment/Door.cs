using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Door : MonoBehaviour {

    public enum DoorType { Key, SwitchOrButton }

    [SerializeField] private DoorType Type;
    [SerializeField] private Item KeyName;
    [SerializeField] private string DisplayMessage;

    private GameObject m_InteractionButton;
    public bool m_IsPlayerNearDoor = false;
    private Animator m_Animator;

    private void Start()
    {
        InitializeInteractionButton();

        InitializeAnimator();

        ShowInteractionKey(false);
    }

    private void InitializeInteractionButton()
    {
        if (Type == DoorType.Key)
        {
            var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
            m_InteractionButton = Instantiate(interactionButton, transform);
        }
    }


    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Door.InitializeAnimator: Can't find Animator component on Gamobject");
        }
    }

    private void Update()
    {
        if (m_IsPlayerNearDoor)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                if (Type == DoorType.Key)
                    OpenDoorWithKey();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNearDoor)
        {
            m_IsPlayerNearDoor = true;
            ShowInteractionKey(m_IsPlayerNearDoor);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsPlayerNearDoor)
        {
            m_IsPlayerNearDoor = false;
            ShowInteractionKey(m_IsPlayerNearDoor);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") & !m_IsPlayerNearDoor)
        {
            m_IsPlayerNearDoor = true;
            ShowTip();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") & m_IsPlayerNearDoor)
        {
            m_IsPlayerNearDoor = false;
        }
    }

    private void ShowInteractionKey(bool show)
    {
        if (Type == DoorType.Key)
        {
            if (m_InteractionButton != null)
                m_InteractionButton.gameObject.SetActive(show);
            else
                Debug.LogError("Door.ShowInteractionKey: InteractionButtonImage is not initialized");
        }
    }

    private void OpenDoorWithKey()
    {
        if (string.IsNullOrEmpty(KeyName.Name))
        {
            Destroy(gameObject);
        }
        else if (PlayerStats.PlayerInventory.IsInBag(KeyName.Name))
        {
            if (PlayerStats.PlayerInventory.Remove(KeyName))
                AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message(KeyName.Name + " was removed from inventory"));

            if (GameMaster.Instance != null)
                GameMaster.Instance.SaveBoolState(gameObject.name, true);

            Destroy(gameObject);
        }
        else
        {
            AnnouncerManager.Instance.DisplayAnnouncerMessage(GetMessage());
        }
    }

    private void ShowTip()
    {
        if (!string.IsNullOrEmpty(DisplayMessage))
            AnnouncerManager.Instance.DisplayAnnouncerMessage(
                new AnnouncerManager.Message(DisplayMessage));
        else
            Debug.LogError("Door.ShowTip: Can't display empty tip");
    }

    public void PlayOpenDoorAnimation()
    {
        AudioManager.Instance.Play("DoorOpen");
        PlayAnimation("Open");
    }

    public void PlayCloseDoorAnimation()
    {
        PlayAnimation("Close");
    }

    private void PlayAnimation(string name)
    {
        if (m_Animator != null)
        {
            m_Animator.SetTrigger(name);
        }
    }

    private AnnouncerManager.Message GetMessage()
    {
        return new AnnouncerManager.Message(KeyName.Name + " required");
    }

}

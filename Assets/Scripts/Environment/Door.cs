using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public enum DoorType { Key, SwitchOrButton }

    [SerializeField] private DoorType Type;
    [SerializeField] private Item KeyName;

    private SpriteRenderer m_InteractionButton;
    private bool m_IsPlayerNearDoor = false;
    private Animator m_Animator;

    private void Start()
    {
        InitializeInteractionButton();

        InitializeAnimator();

        ShowInteractionKey(false);
    }

    private void InitializeInteractionButton()
    {
        if (transform.childCount > 0)
        {
            m_InteractionButton = transform.GetChild(0).GetComponent<SpriteRenderer>();

            if (m_InteractionButton == null)
            {
                Debug.LogError("Door.InitializeInteractionButton: There is no Sprite Renderer on Gameobject.");
            }
        }
        else if (Type == DoorType.Key)
        {
            Debug.LogError("Door.InitializeInteractionButton: Can't find SpriteRenderer in child");
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
        if (m_IsPlayerNearDoor & Type == DoorType.Key)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenDoorWithKey();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNearDoor & Type == DoorType.Key)
        {
            m_IsPlayerNearDoor = true;
            ShowInteractionKey(m_IsPlayerNearDoor);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsPlayerNearDoor & Type == DoorType.Key)
        {
            m_IsPlayerNearDoor = false;
            ShowInteractionKey(m_IsPlayerNearDoor);
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
            Destroy(gameObject);
        }
        else
        {
            AnnouncerManager.Instance.DisplayAnnouncerMessage(GetMessage());
        }
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

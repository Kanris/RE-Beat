using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PickUpItem : MonoBehaviour {

    [SerializeField] private Item item;

    private GameObject m_InteractionButton;
    private bool m_IsPlayerNearDoor = false;

    private void Start()
    {
        InitializeInteractionButton();
        ShowInteractionKey(false);
    }

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);
    }

    private void Update()
    {
        if (m_IsPlayerNearDoor)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                AddToTheInventory();
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

    private void ShowInteractionKey(bool show)
    {
        if (m_InteractionButton != null)
            m_InteractionButton.gameObject.SetActive(show);
        else
            Debug.LogError("Door.ShowInteractionKey: InteractionButtonImage is not initialized");
    }

    private void AddToTheInventory()
    {
        PlayerStats.PlayerInventory.Add(item);
        AnnouncerManager.Instance.DisplayAnnouncerMessage(GetAnnouncerMessage());
        Destroy(gameObject);
    }

    private AnnouncerManager.Message GetAnnouncerMessage()
    {
        return new AnnouncerManager.Message(item.Name + " add to inventory");
    }
}

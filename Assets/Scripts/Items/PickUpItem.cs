using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PickUpItem : MonoBehaviour {

    [SerializeField] private Item item;

    private GameObject m_InteractionButton;
    private bool m_IsPlayerNearDoor = false;
    private PlayerStats m_PlayerStats;
    private SpriteRenderer m_SpriteRenderer;

    private void Start()
    {
        InitializeInteractionButton();
        ShowInteractionKey(false);
    }

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);

        //m_SpriteRenderer.sprite.
    }

    private void Update()
    {
        if (m_IsPlayerNearDoor)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                InteractWithItem();
            }
        }
    }

    private void InteractWithItem()
    {            
        switch (item.itemType)
        {
            case Item.ItemType.Item:
                AddToTheInventory();
                break;
            case Item.ItemType.Note:
                ReadNote();
                break;
            case Item.ItemType.Heal:
                if (m_PlayerStats != null)
                    m_PlayerStats.HealPlayer(item.HealAmount);
                Destroy(gameObject);
                break;
        }

        if (GameMaster.Instance != null)
            GameMaster.Instance.SaveState(name, 0, GameMaster.RecreateType.Object);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNearDoor)
        {
            m_IsPlayerNearDoor = true;

            if (item.itemType == Item.ItemType.Heal)
                m_PlayerStats = collision.GetComponent<Player>().playerStats;

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
        PlayerStats.PlayerInventory.Add(item, GetComponent<SpriteRenderer>().sprite.name);
        AnnouncerManager.Instance.DisplayAnnouncerMessage(GetAnnouncerMessage());

        Destroy(gameObject);
    }

    private void ReadNote()
    {
        AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message(item.Description, 4f));
    }

    private AnnouncerManager.Message GetAnnouncerMessage()
    {
        return new AnnouncerManager.Message(item.Name + " add to inventory");
    }
}

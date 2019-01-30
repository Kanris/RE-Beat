using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class PickUpItem : MonoBehaviour {

    #region private fields

    [SerializeField] private Item item; //item description

    [Header("Additional")]
    [SerializeField] private TextMeshProUGUI m_ItemDescriptionText;

    [Header("Effects")]
    [SerializeField] private GameObject m_InteractionButton; //item ui
    [SerializeField] private GameObject m_DescriptionUI;

    private PlayerStats m_PlayerStats; //player stats (for heal item type)
    private bool m_IsPlayerNearItem = false; //is player near item

    #endregion

    #region private methods

    #region initialize

    private void Start()
    { 
        m_InteractionButton.gameObject.SetActive(false); //hide item ui

        m_ItemDescriptionText.text = GetDisplayText();
    }

    private void OnValidate()
    {
        if (item != null)
        {
            GetComponent<SpriteRenderer>().sprite = item.Image;
            transform.name = item.name;
        }
    }

    #endregion

    private void Update()
    {
        if (m_IsPlayerNearItem & InputControlManager.IsCanUseSubmitButton()) //if is player near item
        {
            if (InputControlManager.Instance.IsPickupPressed()) //if player pressed submit button
            {
                InteractWithItem(); //add item
            }
        }
        else if (m_InteractionButton.activeSelf)
        {
            m_InteractionButton.SetActive(false);
        }
    }

    private string GetDisplayText()
    {
        var textToDisplay = string.Empty;

        switch (item.itemDescription.itemType)
        {
            case ItemDescription.ItemType.Heal:
                textToDisplay = "<color=yellow>" + item.itemDescription.Name + "</color> heals you for <color=yellow>"
                            + item.itemDescription.HealAmount + "</color> points";
                break;

            case ItemDescription.ItemType.Item:
                textToDisplay = "<color=yellow>" + item.itemDescription.Name + "</color>";
                break;

            case ItemDescription.ItemType.Note:
                textToDisplay = "<color=yellow>" + 
                            LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Description) 
                                                 + "</color>";
                break;
        }

        return textToDisplay;
    }

    private void InteractWithItem()
    {            
        switch (item.itemDescription.itemType) //base on item type
        {
            case ItemDescription.ItemType.Item: //if it is item
                AddToTheInventory(); //add to the inventory
                break;

            case ItemDescription.ItemType.Note: //it is is note
                ReadNote(); //read note
                break;

            case ItemDescription.ItemType.Heal: //if it is heal
                if (m_PlayerStats != null)
                    m_PlayerStats.HealPlayer(item.itemDescription.HealAmount); //heal player
                break;
        }
        
        GameMaster.Instance.SaveState(name, 0, GameMaster.RecreateType.Object); //save object state
        InputControlManager.Instance.StartGamepadVibration(1f, 0.05f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near item
        {
            m_IsPlayerNearItem = true; //player is near item

            if (item.itemDescription.itemType == ItemDescription.ItemType.Heal) //if item type is heal
                m_PlayerStats = collision.GetComponent<Player>().playerStats; //save reference to the player stats

            m_InteractionButton.gameObject.SetActive(true); // show item ui
            m_DescriptionUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player move away from the item
        {
            m_IsPlayerNearItem = false; //player is not near the door

            if (item.itemDescription.itemType == ItemDescription.ItemType.Heal) //if item type is heal
                m_PlayerStats = null; //remove reference to the player stats

            m_InteractionButton.gameObject.SetActive(false); //hide item ui
            m_DescriptionUI.SetActive(false);
        }
    }

    private void AddToTheInventory()
    {
        PlayerStats.PlayerInventory.Add(item.itemDescription, GetComponent<SpriteRenderer>().sprite.name); //add item to the player's bag

        var itemName = LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Name);
        var itemAddMessage = LocalizationManager.Instance.GetItemsLocalizedValue("add_to_inventory_message");

        UIManager.Instance.DisplayNotificationMessage(itemName + " " + itemAddMessage, 
            UIManager.Message.MessageType.Item); //show announcer message about new item in the bag
    }

    private void ReadNote()
    {
        var noteText = LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Description);
        UIManager.Instance.DisplayNotificationMessage(noteText, UIManager.Message.MessageType.Item, 6f); //show announcer with message in the note
    }

    #endregion
}

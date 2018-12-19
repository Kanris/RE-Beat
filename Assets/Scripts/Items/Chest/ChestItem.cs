using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class ChestItem : MonoBehaviour {

    #region private fields

    [SerializeField] private Item item; //item description

    [SerializeField] private Image m_ItemImage;
    [SerializeField] private TextMeshProUGUI m_ItemText;

    #endregion

    #region public methods

    private void Start()
    {
        if (m_ItemText != null & LocalizationManager.Instance != null)
            m_ItemText.text = LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Name);
    }

    public void AddToTheInventory()
    {
        if (item.itemDescription.itemType == ItemDescription.ItemType.Item)
        {
            PlayerStats.PlayerInventory.Add(item.itemDescription, GetComponent<Image>().sprite.name); //add item to the player inventory

            var itemName = LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Name);
            var itemAddMessage = LocalizationManager.Instance.GetItemsLocalizedValue("add_to_inventory_message");

            UIManager.Instance.DisplayNotificationMessage(itemName + " " + itemAddMessage,
                UIManager.Message.MessageType.Item); //display add message
        }
        else if (item.itemDescription.itemType == ItemDescription.ItemType.Note)
        {
            var noteMessage = LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Description);

            UIManager.Instance.DisplayNotificationMessage(noteMessage,
                UIManager.Message.MessageType.Message, 5); //display add message
        }

        GameMaster.Instance.SaveState(transform.parent.parent.parent.name, gameObject.name, GameMaster.RecreateType.ChestItem); //save chest item state

        Destroy(gameObject); //destroy item
    }

    #endregion

    private void OnValidate()
    {
        if (item != null)
        {
            if (m_ItemImage != null) m_ItemImage.sprite = item.Image;
            transform.name = item.name;

            if (m_ItemText != null & LocalizationManager.Instance != null) 
                m_ItemText.text = LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Name );

            else if (m_ItemText != null)
            {
                m_ItemText.text = item.itemDescription.Name;
            }
        }
    }
}

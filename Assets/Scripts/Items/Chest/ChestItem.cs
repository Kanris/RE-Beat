using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ChestItem : MonoBehaviour {

    #region private fields

    [SerializeField] private Item item; //item description

    private Image m_SelectImage;

    #endregion

    #region public methods

    private void Awake()
    {
        m_SelectImage = transform.GetChild(0).GetComponent<Image>();
        m_SelectImage.gameObject.SetActive(false);
    }

    public void AddToTheInventory()
    {
        PlayerStats.PlayerInventory.Add(item.itemDescription, GetComponent<Image>().sprite.name); //add item to the player inventory

        var itemName = LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Name);
        var itemAddMessage = LocalizationManager.Instance.GetItemsLocalizedValue("add_to_inventory_message");

        UIManager.Instance.DisplayNotificationMessage(
            new UIManager.Message(itemName + " " + itemAddMessage, UIManager.Message.MessageType.Item)); //display add message

        GameMaster.Instance.SaveState(transform.parent.parent.parent.name, gameObject.name, GameMaster.RecreateType.ChestItem); //save chest item state

        Destroy(gameObject); //destroy item
    }

    #endregion

    private void OnValidate()
    {
        if (item != null)
        {
            GetComponent<Image>().sprite = item.Image;
            transform.name = item.name;
        }
    }

    public void MouseHoverEnter()
    {
        m_SelectImage.gameObject.SetActive(true);
    }

    public void MouseHoverExit()
    {
        m_SelectImage.gameObject.SetActive(false);
    }
}

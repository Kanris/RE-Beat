using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ChestItem : MonoBehaviour {

    #region private fields

    [SerializeField] private Item item; //item description

    #endregion

    #region public methods

    public void AddToTheInventory()
    {
        PlayerStats.PlayerInventory.Add(item.itemDescription, GetComponent<Image>().sprite.name); //add item to the player inventory

        var itemName = LocalizationManager.Instance.GetItemsLocalizedValue(item.itemDescription.Name);
        var itemAddMessage = LocalizationManager.Instance.GetItemsLocalizedValue("add_to_inventory_message");

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message(itemName + " " + itemAddMessage + " - I")); //display additing message

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
}

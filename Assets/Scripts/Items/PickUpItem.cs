using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PickUpItem : MonoBehaviour {

    #region private fields

    [SerializeField] private Item item; //item description

    private GameObject m_InteractionButton; //item ui
    private PlayerStats m_PlayerStats; //player stats (for heal item type)
    private bool m_IsPlayerNearDoor = false; //is player near item

    #endregion

    #region private methods

    #region initialize

    private void Start()
    {
        InitializeInteractionButton(); //initialize item ui

        m_InteractionButton.gameObject.SetActive(false); //hide item ui
    }

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);

    }

    #endregion

    private void Update()
    {
        if (m_IsPlayerNearDoor) //if is player near item
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player pressed submit button
            {
                InteractWithItem(); //add item
            }
        }
    }

    private void InteractWithItem()
    {            
        switch (item.itemType) //base on item type
        {
            case Item.ItemType.Item: //if it is item
                AddToTheInventory(); //add to the inventory
                break;

            case Item.ItemType.Note: //it is is note
                ReadNote(); //read note
                break;

            case Item.ItemType.Heal: //if it is heal
                if (m_PlayerStats != null)
                    m_PlayerStats.HealPlayer(item.HealAmount); //heal player

                Destroy(gameObject);
                break;
        }
        
        GameMaster.Instance.SaveState(name, 0, GameMaster.RecreateType.Object); //save object state
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near item
        {
            m_IsPlayerNearDoor = true; //player is near item

            if (item.itemType == Item.ItemType.Heal) //if item type is heal
                m_PlayerStats = collision.GetComponent<Player>().playerStats; //save reference to the player stats

            m_InteractionButton.gameObject.SetActive(true); // show item ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player move away from the item
        {
            m_IsPlayerNearDoor = false; //player is not near the door

            if (item.itemType == Item.ItemType.Heal) //if item type is heal
                m_PlayerStats = null; //remove reference to the player stats

            m_InteractionButton.gameObject.SetActive(false); //hide item ui
        }
    }

    private void AddToTheInventory()
    {
        PlayerStats.PlayerInventory.Add(item, GetComponent<SpriteRenderer>().sprite.name); //add item to the player's bag
        AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message(item.Name + " add to inventory")); //show announcer message about new item in the bag

        Destroy(gameObject); //destroy this item
    }

    private void ReadNote()
    {
        AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message(item.Description, 4f)); //show announcer with message in the note
    }

    #endregion
}

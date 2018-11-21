using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using TMPro;

public class Trader : MonoBehaviour {

    [SerializeField] private GameObject m_InteractionUI; //ui that npc show to player when he get closer
    [SerializeField] private GameObject m_StoreUI; //items that this vendor sells

    [Header("Description UI")]
    [SerializeField] private GameObject m_DescriptionUI; //item description ui
    [SerializeField] private TextMeshProUGUI m_DescriptionNameText; //item name
    [SerializeField] private TextMeshProUGUI m_DescriptionText; //item description
    [SerializeField] private TextMeshProUGUI m_ScrapAmountText; //item cost

    [Header("Notification")]
    [SerializeField] private GameObject m_Notification; //message that vendor show when there is nothing to sell
    [SerializeField] private Audio m_ClickAudio;

    //current selected item info
    private Item m_CurrentSelectedItem; 
    private GameObject m_CurrentSelectedItemGO;

    private PlayerStats m_Player; //notify is player near the vendor

    private void Start()
    {
        StartCoroutine(HideUIWithDelay()); //hide ui with delay so GameMaster can recreate trader inventory state
    }

    private IEnumerator HideUIWithDelay()
    {
        yield return new WaitForSeconds(0.1f);

        HideUI();
    }

    // Update is called once per frame
    void Update () {
		
        if (m_Player != null) //if player is near
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit") & !m_StoreUI.activeSelf) //if player press submit button and store ui isn't open
            {
                m_StoreUI.SetActive(true); //show store ui
                m_InteractionUI.SetActive(false); //hide interaction elements
            }
        }
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near
        {
            if (CheckInventoryAmount()) //if there are items to sell
            {
                m_Player = collision.GetComponent<Player>().playerStats; //indicate that player is near

                collision.GetComponent<Player>().TriggerPlayerBussy(true); //dont allow player to attack
                AnnouncerManager.Instance.ShowScrapAmount(true); //show player's current amount of scraps
                m_InteractionUI.SetActive(true); //show interaction elements
            }
            else //if there is nothing to sell
            {
                collision.GetComponent<Player>().TriggerPlayerBussy(true); //don't allow player to attack
                AnnouncerManager.Instance.ShowScrapAmount(true); //show player's current amount of scraps
                m_Notification.SetActive(true); //show npc message
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leave trader trigger
        {
            m_Player = null; //indicate that player is not near trader

            HideUI(); //hide npc ui

            collision.GetComponent<Player>().TriggerPlayerBussy(false); //allow player to attack
            AnnouncerManager.Instance.ShowScrapAmount(false); //hide scrap amount
        }
    }

    private void HideUI()
    {
        m_InteractionUI.SetActive(false);
        m_DescriptionUI.SetActive(false);
        m_StoreUI.SetActive(false);
        m_Notification.SetActive(false);
    }

    public void ShowItemDescription(GameObject itemToDisplayGO)
    {
        m_CurrentSelectedItem = itemToDisplayGO.GetComponent<TraderItem>().m_TraderItem; //get selected item description
        m_CurrentSelectedItemGO = itemToDisplayGO; //store item gameobject
         
        m_DescriptionUI.SetActive(true); //show descripiton ui

        //set text to display on description ui
        m_DescriptionNameText.text = LocalizationManager.Instance.GetItemsLocalizedValue(m_CurrentSelectedItem.itemDescription.Name);

        m_DescriptionText.text = LocalizationManager.Instance.GetItemsLocalizedValue(m_CurrentSelectedItem.itemDescription.Description);

        m_ScrapAmountText.text = m_CurrentSelectedItem.itemDescription.ScrapAmount.ToString();
    }

    public void BuyItem()
    {
        if (m_CurrentSelectedItem != null) //if item were selected
        {
            if ((PlayerStats.Scrap - m_CurrentSelectedItem.itemDescription.ScrapAmount) >= 0) //if player has enough scrap
            {
                AudioManager.Instance.Play(m_ClickAudio);

                PlayerStats.Scrap = -m_CurrentSelectedItem.itemDescription.ScrapAmount; //change player's scrap amount

                //add item to the inventory if it's not heal potion
                if (m_CurrentSelectedItem.itemDescription.itemType != ItemDescription.ItemType.Heal)
                {
                    //get item info
                    var itemName = LocalizationManager.Instance.GetItemsLocalizedValue(m_CurrentSelectedItem.itemDescription.Name);
                    var inventoryMessage = LocalizationManager.Instance.GetItemsLocalizedValue("add_to_inventory_message");

                    //display that item was added to inventory
                    AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message(
                        itemName + " " + inventoryMessage, AnnouncerManager.Message.MessageType.Item
                    ));

                    //add item to inventory
                    PlayerStats.PlayerInventory.Add(m_CurrentSelectedItem.itemDescription, m_CurrentSelectedItem.Image.name);
                }

                //apply item upgrade to the player
                m_CurrentSelectedItemGO.GetComponent<TraderItem>().ApplyUpgrade(m_Player);

                //move item from store ui (so childCount works properly)
                m_CurrentSelectedItemGO.transform.SetParent(null);
                Destroy(m_CurrentSelectedItemGO);

                //hide desciption ui
                m_DescriptionUI.SetActive(false);

                //if there is nothing to sell
                if (!CheckInventoryAmount())
                {
                    m_StoreUI.SetActive(false); //hide store ui
                    m_Notification.SetActive(true); //show trader message
                }
            }
            else //if player does not have enought scrap
            {
                AnnouncerManager.Instance.DisplayAnnouncerMessage(
                    new AnnouncerManager.Message("I haven't enough money to buy this",
                                                 AnnouncerManager.Message.MessageType.Message));
            }
        }
    }

    private bool CheckInventoryAmount()
    {
        return m_StoreUI.transform.GetChild(0).transform.childCount > 0; //if there are items to sell
    }

}

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

    [Header("Additional")]
    [SerializeField] private bool m_IsGeneralTrader = true;

    //current selected item info
    private Item m_CurrentSelectedItem; 
    private GameObject m_CurrentSelectedItemGO;

    private PlayerStats m_Player; //notify is player near the vendor

    // Update is called once per frame
    void Update () {
		
        if (m_IsGeneralTrader)
        {
            if (m_Player != null) //if player is near
            {
                if (CrossPlatformInputManager.GetButtonDown("Cancel"))
                {
                    if (m_DescriptionUI.activeSelf)
                    {
                        m_DescriptionUI.SetActive(false);
                    }
                    else if (m_StoreUI.activeSelf)
                    {
                        HideUI();
                        m_InteractionUI.SetActive(true);
                    }
                }

                if (CrossPlatformInputManager.GetButtonDown("Submit") & !m_StoreUI.activeSelf) //if player press submit button and store ui isn't open
                {
                    PauseMenuManager.Instance.SetIsCantOpenPauseMenu(true); //don't allow to open pause menu

                    m_StoreUI.SetActive(true); //show store ui
                    m_InteractionUI.SetActive(false); //hide interaction elements
                }
            }
            else if (m_InteractionUI.activeSelf)
            {
                HideUI();
                m_DescriptionUI.SetActive(false);
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
                
                m_InteractionUI.SetActive(true); //show interaction elements
            }
            else //if there is nothing to sell
            {
                collision.GetComponent<Player>().TriggerPlayerBussy(true); //don't allow player to attack
                m_Notification.SetActive(true); //show npc message
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leave trader trigger
        {
            m_Player = null; //indicate that player is not near trader

            collision.GetComponent<Player>().TriggerPlayerBussy(false); //allow player to attack

            HideUI(); //hide npc ui
        }
    }

    public void HideUI()
    {
        if (m_InteractionUI != null) m_InteractionUI.SetActive(false);

        if (m_DescriptionUI != null) m_DescriptionUI.SetActive(false);
        if (m_StoreUI != null) m_StoreUI.SetActive(false);
        if (m_Notification != null) m_Notification.SetActive(false);


        PauseMenuManager.Instance.SetIsCantOpenPauseMenu(false);
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
                if (m_Player.CurrentHealth == m_Player.MaxHealth & 
                    m_CurrentSelectedItem.itemDescription.itemType == ItemDescription.ItemType.Heal)
                {
                    UIManager.Instance.DisplayNotificationMessage("Can't buy repair at max health!", 
                        UIManager.Message.MessageType.Message);
                }
                else
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
                        UIManager.Instance.DisplayNotificationMessage(itemName + " " + inventoryMessage, 
                            UIManager.Message.MessageType.Item);

                        //add item to inventory
                        PlayerStats.PlayerInventory.Add(m_CurrentSelectedItem.itemDescription, m_CurrentSelectedItem.Image.name);
                    }

                    //apply item upgrade to the player
                    m_CurrentSelectedItemGO.GetComponent<TraderItem>().ApplyUpgrade(m_Player);

                    //hide desciption ui
                    m_DescriptionUI.SetActive(false);

                    //move item from store ui (so childCount works properly)
                    if (!m_CurrentSelectedItemGO.GetComponent<TraderItem>().m_IsInfiniteAmount)
                    {
                        m_CurrentSelectedItemGO.transform.SetParent(null);
                        Destroy(m_CurrentSelectedItemGO);

                        //if there is nothing to sell
                        if (!CheckInventoryAmount())
                        {
                            m_StoreUI.SetActive(false); //hide store ui
                            m_Notification.SetActive(true); //show trader message
                        }
                    }
                }         
            }
            else //if player does not have enought scrap
            {
                UIManager.Instance.DisplayNotificationMessage("Payment rejected <br> \"You don't have enough scraps\"",
                                                 UIManager.Message.MessageType.Message);
            }
        }
    }

    public void SetPlayer(PlayerStats player)
    {
        m_Player = player;
    }

    private bool CheckInventoryAmount()
    {
        return m_StoreUI.transform.GetChild(0).transform.childCount > 0; //if there are items to sell
    }

}

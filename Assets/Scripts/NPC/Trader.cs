using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Trader : MonoBehaviour {

    [SerializeField] private GameObject m_InteractionUI; //ui that npc show to player when he get closer
    [SerializeField] private GameObject m_StoreUI; //items that this vendor sells
    [SerializeField] private GameObject m_InventoryUI; //trader's store UI

    [Header("Description UI")]
    [SerializeField] private TextMeshProUGUI m_DescriptionNameText; //item name
    [SerializeField] private TextMeshProUGUI m_DescriptionText; //item description

    [Header("Scroll")]
    [SerializeField] private RectTransform m_Content; //traders store inventory
    [SerializeField] private GameObject m_ArrowUP; //arrow up image
    [SerializeField] private GameObject m_ArrowDown; //arrow down image

    private float m_DefaultYContentPosition; //default y position for trader's inventory

    //current selected item info
    private Item m_CurrentSelectedItem; 
    private GameObject m_CurrentSelectedItemGO;

    private bool m_IsPlayerNear; //indicates is player near
    private bool m_IsBoughtItem; //indicates that player bought item

    private int m_CurrentSelectedItemIndex = 0;

    private void Awake()
    {
        m_DefaultYContentPosition = m_Content.localPosition.y; //get default position y position for trader's inventory
    }

    // Update is called once per frame
    void Update () {

        if (m_IsPlayerNear) //if player is near
        {
            if (InputControlManager.Instance.m_Joystick.GetControl(InControl.InputControlType.Back).WasPressed 
                || InputControlManager.Instance.m_Joystick.Action2) //if back button or attack button pressed
            {
                //if trader's store is open
                if (m_StoreUI.activeSelf)
                {
                    StartCoroutine(HideUI()); //hide npc ui
                    m_InteractionUI.SetActive(true);
                }
            }

            //if there is not selected object in trader's inventory
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                //select first item
                EventSystem.current.SetSelectedGameObject(m_InventoryUI.transform.GetChild(m_CurrentSelectedItemIndex).gameObject);
            }

            //if player can press submit button
            if (InputControlManager.IsCanUseSubmitButton())
            {
                if (InputControlManager.IsUpperButtonsPressed() && !m_StoreUI.activeSelf) //if player press submit button and store ui isn't open
                {
                    PauseMenuManager.Instance.SetIsCantOpenPauseMenu(true); //don't allow to open pause menu

                    m_StoreUI.SetActive(true); //show store ui
                    m_InteractionUI.SetActive(false); //hide interaction elements

                    EventSystem.current.SetSelectedGameObject(null); //remove selected gameobject to select first item in the list
                    EventSystem.current.SetSelectedGameObject(m_InventoryUI.transform.GetChild(m_CurrentSelectedItemIndex).gameObject);
                }
                else if (InputControlManager.Instance.m_Joystick.Action4.WasReleased && m_CurrentSelectedItem != null)
                {
                    m_CurrentSelectedItemGO.GetComponent<TraderItem>().m_BuyingImage.fillAmount = 0f;
                    m_IsBoughtItem = false;
                }
                //submit button pressed to buy item
                else if (InputControlManager.Instance.m_Joystick.Action4.IsPressed && m_CurrentSelectedItem != null)
                {
                    if (m_CurrentSelectedItemGO.GetComponent<TraderItem>().m_BuyingImage.fillAmount >= 1f)
                    {
                        m_CurrentSelectedItemGO.GetComponent<TraderItem>().m_BuyingImage.fillAmount = 0f;

                        BuyItem();

                        m_IsBoughtItem = true;
                    }
                    else if (!m_IsBoughtItem)
                    {
                        InputControlManager.Instance.StartJoystickVibrate(0.5f, 0.01f);
                        m_CurrentSelectedItemGO.GetComponent<TraderItem>().m_BuyingImage.fillAmount += (1f * Time.deltaTime);
                    }
                }
            }
        }
        else if (m_InteractionUI.activeSelf || m_StoreUI.activeSelf)
        {
            StartCoroutine(HideUI()); //hide npc ui
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near
        {
            m_IsPlayerNear = true; //indicates that plyear is near

            m_InteractionUI.SetActive(true); //show interaction elements
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leave trader trigger
        {
            m_IsPlayerNear = false; //indicate that player is not near trader

            StartCoroutine( HideUI() ); //hide npc ui
        }
    }

    //hide all trader's ui elemetnts
    public IEnumerator HideUI()
    {
        m_InteractionUI?.SetActive(false); //hide interaction UI

        m_StoreUI?.SetActive(false); //hide store UI

        //remove item's selection
        m_CurrentSelectedItemIndex = 0;
        EventSystem.current.SetSelectedGameObject(null); //remove selected gameobject to select first item in the list
        EventSystem.current.SetSelectedGameObject(m_InventoryUI.transform.GetChild(m_CurrentSelectedItemIndex).gameObject);

        yield return new WaitForEndOfFrame();

        PauseMenuManager.Instance.SetIsCantOpenPauseMenu(false); //allow to open pause menu
    }

    public void ShowItemDescription(GameObject itemToDisplayGO)
    {
        m_CurrentSelectedItem = itemToDisplayGO.GetComponent<TraderItem>().m_TraderItem; //get selected item description
        m_CurrentSelectedItemGO = itemToDisplayGO; //store item gameobject

        //move rect depend on items count
        var index = itemToDisplayGO.transform.GetSiblingIndex();

        //play arrows animation if arrows are available
        if (m_ArrowUP.activeSelf) 
            ManageArrowsVisibility(index, m_CurrentSelectedItemIndex);

        m_CurrentSelectedItemIndex = index;

        //if selected item index 4 or grater - move rect
        if (index > 3)
            m_Content.localPosition = m_Content.localPosition.
                With(y: m_DefaultYContentPosition + .5f * (index - 3)); //.5f * index more than 3 (.5f * 2)

        else //if selected item index is less than 4 return rect to normal position
            m_Content.localPosition = m_Content.localPosition.
                With(y: m_DefaultYContentPosition);

        //itemToDisplayGO.transform.GetSiblingIndex()

        //set text to display on description ui
        m_DescriptionNameText.text = LocalizationManager.Instance.GetItemsLocalizedValue(m_CurrentSelectedItem.itemDescription.Name);

        m_DescriptionText.text = LocalizationManager.Instance.GetItemsLocalizedValue(m_CurrentSelectedItem.itemDescription.Description);
    }

    #region arrows control

    //manage arraows visibility (base on current selected item index)
    public void ManageArrowsVisibility(int index, int previousIndex)
    {
        var epsilon = .01f; //epsilon to check alpha value

        if (index > 0) //index below first items
        {
            if (index + 1 == m_Content.childCount) //there is more items below current item
            {
                //if there is more than 1 item in list and up button is in hide trigger
                if (m_Content.childCount > 1 && m_ArrowUP.GetComponent<Image>().color.a < epsilon)
                    //show up arrow appear animation
                    m_ArrowUP.GetComponent<Animator>().SetTrigger("Show"); 

                //hide down arrow (because there is no items below current item)
                m_ArrowDown.GetComponent<Animator>().SetTrigger("Hide");
            }
            //selected item is below previous selected
            else if (index > m_CurrentSelectedItemIndex)
            {
                //if up arrow is hidden
                if (m_ArrowUP.GetComponent<Image>().color.a < epsilon)
                    //show up arrow appear animation
                    m_ArrowUP.GetComponent<Animator>().SetTrigger("Show");

                //play arrow move down animation
                m_ArrowDown.GetComponent<Animator>().SetTrigger("Move");
            }
            //selected item is above previous selected
            else
            {
                //if down arrow is hiiden
                if (m_ArrowDown.GetComponent<Image>().color.a < epsilon)
                    //show down arrow appear animation
                    m_ArrowDown.GetComponent<Animator>().SetTrigger("Show");

                //play arrow move up animation
                m_ArrowUP.GetComponent<Animator>().SetTrigger("Move");
            }
        }
        else //index on first items
        {
            //there is only 1 item in list
            if (m_Content.childCount == 1)
            {
                //hide buttons
                m_ArrowUP.SetActive(false);
                m_ArrowDown.SetActive(false);
            }
            else
            {
                if (m_Content.childCount > 1 && m_ArrowDown.GetComponent<Image>().color.a < epsilon)
                    m_ArrowDown.GetComponent<Animator>().SetTrigger("Show");

                m_ArrowUP.GetComponent<Animator>().SetTrigger("Hide");
            }
        }
    }

    //activate/disable arrows base on values
    private void ArrowVisibility(bool upValue, bool downValue)
    {
        m_ArrowUP.SetActive(upValue);
        m_ArrowDown.SetActive(downValue);
    }

    #endregion

    public void BuyItem()
    {
        if (m_CurrentSelectedItem != null) //if item were selected
        {
            //current player on scene
            var player = GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<Player>().playerStats;

            if (player != null)
            {
                if ((PlayerStats.Scrap - m_CurrentSelectedItem.itemDescription.ScrapAmount) >= 0) //if player has enough scrap
                {
                    if (player.CurrentHealth == player.MaxHealth &
                        m_CurrentSelectedItem.itemDescription.itemType == ItemDescription.ItemType.Heal)
                    {
                        UIManager.Instance.DisplayNotificationMessage("Can't buy repair at max health!",
                            UIManager.Message.MessageType.Message);
                    }
                    else
                    {
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
                        m_CurrentSelectedItemGO.GetComponent<TraderItem>().ApplyUpgrade(player);

                        //move item from store ui (so childCount works properly)
                        if (!m_CurrentSelectedItemGO.GetComponent<TraderItem>().m_IsInfiniteAmount)
                        {
                            m_CurrentSelectedItemGO.transform.SetParent(null);
                            Destroy(m_CurrentSelectedItemGO);

                            //get in focuse next trader's item
                            EventSystem.current.SetSelectedGameObject(null);
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
    }

    private bool CheckInventoryAmount()
    {
        return m_StoreUI.transform.GetChild(0).transform.childCount > 0; //if there are items to sell
    }

}

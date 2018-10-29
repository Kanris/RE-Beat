using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using TMPro;

public class Trader : MonoBehaviour {

    [SerializeField] private GameObject m_InteractionUI;
    [SerializeField] private GameObject m_StoreUI;

    [Header("Description UI")]
    [SerializeField] private GameObject m_DescriptionUI;
    [SerializeField] private TextMeshProUGUI m_DescriptionNameText;
    [SerializeField] private TextMeshProUGUI m_DescriptionText;
    [SerializeField] private TextMeshProUGUI m_ScrapAmountText;

    [Header("Notification")]
    [SerializeField] private GameObject m_Notification;

    private Item m_CurrentSelectedItem;
    private GameObject m_CurrentSelectedItemGO;

    private bool m_IsPlayerNear;

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
		
        if (m_IsPlayerNear)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit") & !m_StoreUI.activeSelf)
            {
                m_StoreUI.SetActive(true);
                m_InteractionUI.SetActive(false);
            }
        }
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (CheckInventoryAmount())
            {
                m_IsPlayerNear = true;

                collision.GetComponent<Player>().TriggerPlayerBussy(true);
                AnnouncerManager.Instance.ShowScrapAmount(true);
                m_InteractionUI.SetActive(true);
            }
            else
            {
                collision.GetComponent<Player>().TriggerPlayerBussy(true);
                AnnouncerManager.Instance.ShowScrapAmount(true);
                m_Notification.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = false;

            HideUI();

            collision.GetComponent<Player>().TriggerPlayerBussy(false);
            AnnouncerManager.Instance.ShowScrapAmount(false);
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
        m_CurrentSelectedItem = itemToDisplayGO.GetComponent<TraderItem>().m_TraderItem;
        m_CurrentSelectedItemGO = itemToDisplayGO;

        m_DescriptionUI.SetActive(true);

        m_DescriptionNameText.text = LocalizationManager.Instance.GetItemsLocalizedValue(m_CurrentSelectedItem.itemDescription.Name);

        m_DescriptionText.text = LocalizationManager.Instance.GetItemsLocalizedValue(m_CurrentSelectedItem.itemDescription.Description);

        m_ScrapAmountText.text = m_CurrentSelectedItem.itemDescription.ScrapAmount.ToString();
    }

    public void BuyItem()
    {
        if (m_CurrentSelectedItem != null)
        {
            if ((PlayerStats.Scrap - m_CurrentSelectedItem.itemDescription.ScrapAmount) >= 0)
            {
                PlayerStats.Scrap = -m_CurrentSelectedItem.itemDescription.ScrapAmount;

                var itemName = LocalizationManager.Instance.GetItemsLocalizedValue(m_CurrentSelectedItem.itemDescription.Name);
                var inventoryMessage = LocalizationManager.Instance.GetItemsLocalizedValue("add_to_inventory_message");

                AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message(
                    itemName + " " + inventoryMessage, AnnouncerManager.Message.MessageType.Item
                ));

                PlayerStats.PlayerInventory.Add(m_CurrentSelectedItem.itemDescription, m_CurrentSelectedItem.Image.name);

                m_CurrentSelectedItemGO.GetComponent<TraderItem>().ApplyUpgrade();

                m_CurrentSelectedItemGO.transform.SetParent(null);
                Destroy(m_CurrentSelectedItemGO);

                m_DescriptionUI.SetActive(false);

                if (!CheckInventoryAmount())
                {
                    m_StoreUI.SetActive(false);
                    m_Notification.SetActive(true);
                }
            }
            else
            {
                AnnouncerManager.Instance.DisplayAnnouncerMessage(
                    new AnnouncerManager.Message("I haven't enough money to buy this",
                                                 AnnouncerManager.Message.MessageType.Message));
            }
        }
    }

    private bool CheckInventoryAmount()
    {
        return m_StoreUI.transform.GetChild(0).transform.childCount > 0;
    }

}

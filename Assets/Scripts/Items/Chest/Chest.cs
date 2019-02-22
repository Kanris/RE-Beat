using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class Chest : MonoBehaviour {

    #region private fields

    #region enum

    public enum ChestType { Common, Destroyable } //types of chest
    [Header("Type")]
    public ChestType chestType; //current chest type

    #endregion

    [Header("UI")]
    [SerializeField] private GameObject m_ChestUI; //chest inventory
    [SerializeField] private GameObject m_InventoryUI; //chest inventory

    [Header("Effects")]
    [SerializeField] private GameObject m_ChestContainItems; //particles that indicate is there any items in the chest
    [SerializeField] private Sprite m_OpenChestSprite; //sprite to swap when chest is empty
    [SerializeField] private Audio ChestOpenAudio; //sound that plays when chest is open/close

    [Header("Additional")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton;

    private GameObject m_InstantChestContainItems; //instantiated chest contain items particles
    private Player m_Player; //indicates is player near or not

    private bool m_IsCanBeOpen; //indicates is chest can be open or not (for destroyable chest)
    private WorldObjectStats m_WorldObjectStats;

    #endregion

    #region private methods

    // Use this for initialization
    void Start () {

        m_InstantChestContainItems = Instantiate(m_ChestContainItems, transform); //instantiate chest particles

        m_InteractionUIButton.PressInteractionButton = OpenChest;

        SetActiveInteractionButton(false); //hide interactive ui
        SetActiveInventory(false); //hide inventory ui

        if (chestType == ChestType.Destroyable)
            InitializeWorldObjectStats(); //initialize destroyable chest
    }

    private void InitializeWorldObjectStats()
    {
        m_WorldObjectStats = GetComponent<WorldObjectStats>();

        m_WorldObjectStats.OnHealthZero = SetIsCanBeOpen;
    }

    #region SetActive

    private void SetActiveInteractionButton(bool value)
    {
        m_InteractionUIButton.SetActive(value);
        m_InteractionUIButton.SetIsPlayerNear(value);
    }

    private void SetActiveInventory(bool value)
    {
        if (m_Player != null)
        {
            m_Player.GetComponent<Platformer2DUserControl>().IsCanJump = !value;
            m_Player.TriggerPlayerBussy(value); //allow or dont allow player to attack when chest inventory is open
        }

        Time.timeScale = value ? 0f : 1f; //return time back to normal

        AudioManager.Instance.Play(ChestOpenAudio); //play chest open sound

        m_ChestUI.SetActive(value); //show or hide chest inventory
    }

    #endregion

    private void Update()
    {
        if (m_Player != null) //if player is near the chest
        {
            if (InputControlManager.Instance.IsCanUseSubmitButton())
            {
                if (m_ChestUI.activeSelf)
                {
                    if (m_InventoryUI.transform.childCount == 0 && m_InstantChestContainItems != null) //if there is no child left
                    {
                        ChangeChestSprite();
                        Destroy(m_InstantChestContainItems);
                    }
                    else if (EventSystem.current.currentSelectedGameObject == null && m_InventoryUI.transform.childCount > 0)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                        EventSystem.current.SetSelectedGameObject(m_InventoryUI.transform.GetChild(0).gameObject);
                    }
                }
            }

            if (InputControlManager.Instance.IsBackMenuPressed() || InputControlManager.Instance.IsBackPressed())
            {
                StartCoroutine(CloseChest());
            }
        }
        else if (m_InteractionUIButton.ActiveSelf())
        {
            m_InteractionUIButton.SetActive(false);
        }
    }

    private IEnumerator CloseChest()
    {
        yield return null;
        
        SetActiveInteractionButton(true); //disable chest ui
        if (m_ChestUI.activeSelf) SetActiveInventory(false); //if chest inventory is open - close it

        m_Player = null; //remove player reference

        PauseMenuManager.Instance.SetIsCantOpenPauseMenu(false); //can open pause menu
    }

    private void OpenChest()
    {
        if (!m_ChestUI.activeSelf)
        {
            if (chestType == ChestType.Destroyable & !m_IsCanBeOpen) //if chest is destroyable but still have health
            {
                var chestInfo = LocalizationManager.Instance.GetItemsLocalizedValue("chest_info");
                UIManager.Instance.DisplayNotificationMessage(chestInfo,
                    UIManager.Message.MessageType.Message); //display warning message
            }
            else //if chest can be open
            {
                SetActiveInventory(!m_ChestUI.activeSelf); //show or hide chest inventory
                SetActiveInteractionButton(false);

                if (m_ChestUI.activeSelf)
                {
                    PauseMenuManager.Instance.SetIsCantOpenPauseMenu(true); //don't allow to open pause menu

                    if (m_InventoryUI.transform.childCount > 0)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                        EventSystem.current.SetSelectedGameObject(m_InventoryUI.transform.GetChild(0).gameObject);
                    }
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !m_InteractionUIButton.ActiveSelf() && !m_ChestUI.activeSelf) //if player is near chest
        {
            m_Player = collision.GetComponent<Player>(); //get player reference
            SetActiveInteractionButton(true); //show chest ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && m_InteractionUIButton.ActiveSelf())
        {
            m_Player = null;
            SetActiveInteractionButton(false);
        }
    }

    private void ChangeChestSprite()
    {
        GetComponent<SpriteRenderer>().sprite = m_OpenChestSprite;

        transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f); //change chest position
    }

    #endregion

    #region public methods

    public void RemoveFromChest(string name) //remove item from chest inventory
    {
        var grid = m_InventoryUI.transform; //get inventory grid

        //search item in grid
        for (int index = 0; index < grid.childCount; index++)
        {
            var gridChildren = grid.GetChild(index);

            if (gridChildren.name == name) //if gridchild name is equal to the search item
            {
                gridChildren.SetParent(null); //change grid children parrent
                Destroy(gridChildren.gameObject); //remove object from the scene

                if (grid.childCount == 0) //if there is no child left
                {
                    ChangeChestSprite(); //chest is empty
                    SetIsCanBeOpen(); //indicate that chest can be open
                    Destroy(m_InstantChestContainItems); //remove particles that show that chest has items in it
                }

                break;
            }
        }
    }

    public void SetIsCanBeOpen()
    {
        m_IsCanBeOpen = true;
    }

    #endregion
}

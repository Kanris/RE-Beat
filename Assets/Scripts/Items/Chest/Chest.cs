using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class Chest : MonoBehaviour {

    #region private fields

    [SerializeField] private GameObject m_Inventory; //chest inventory

    #region enum

    public enum ChestType { Common, Destroyable }
    public ChestType chestType; //current chest type

    #endregion


    [Header("Stats for destroyable")]
    [SerializeField, Range(0, 10)] private int Health = 0; //chest health (for destroyable chest)

    [Header("UI")]
    [SerializeField] private GameObject m_InstantInteractionButton; //chest ui

    [Header("Effects")]
    [SerializeField] private GameObject m_HitEffect;
    [SerializeField] private GameObject m_ChestContainItems;
    [SerializeField] private Sprite m_OpenChestSprite;
    [SerializeField] private Audio ChestOpenAudio;

    private GameObject m_InstantChestContainItems;
    private Player m_Player; //player
    private Animator m_Animator; //chest animator

    #endregion

    #region private methods

    // Use this for initialization
    void Start () {

        m_Animator = GetComponent<Animator>();

        m_InstantChestContainItems = Instantiate(m_ChestContainItems, transform);

        SetActiveInteractionButton(false);

        SetActiveInventory(false);
    }

    #region SetActive

    private void SetActiveInteractionButton(bool value)
    {
        m_InstantInteractionButton.SetActive(value);
    }

    private void SetActiveInventory(bool value)
    {
        if (m_Player != null) m_Player.TriggerPlayerBussy(value); //allow or dont allow player to attack when chest inventory is open

        AudioManager.Instance.Play(ChestOpenAudio); //play chest open sound

        m_Inventory.SetActive(value); //show or hide chest inventory
    }

    #endregion

    private void Update()
    {
        if (m_Player != null) //if player is near the chest
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player pressed submit button
            {
                OpenChest(); //try to open the chest
            }

            if (m_Inventory.activeSelf)
            {
                if (m_Inventory.transform.GetChild(0).childCount == 0 && m_InstantChestContainItems != null) //if there is no child left
                {
                    ChangeChestSprite();
                    Destroy(m_InstantChestContainItems);
                }

                if (CrossPlatformInputManager.GetButtonDown("Cancel"))
                {
                    StartCoroutine(CloseChest());
                }
            }
        }
        else if (m_InstantInteractionButton.activeSelf)
        {
            m_InstantInteractionButton.SetActive(false);
        }
    }

    private IEnumerator CloseChest()
    {
        yield return null;

        SetActiveInteractionButton(true); //disable chest ui
        if (m_Inventory.activeSelf) SetActiveInventory(false); //if chest inventory is open - close it

        PauseMenuManager.Instance.SetIsCantOpenPauseMenu(false); //can open pause menu
    }

    private void OpenChest()
    {
        if (chestType == ChestType.Destroyable & Health != 0) //if chest is destroyable but still have health
        {
            var chestInfo = LocalizationManager.Instance.GetItemsLocalizedValue("chest_info");
            UIManager.Instance.DisplayNotificationMessage(chestInfo, 
                UIManager.Message.MessageType.Message); //display warning message
        }
        else //if chest can be open
        {
            SetActiveInventory(!m_Inventory.activeSelf); //show or hide chest inventory

            if (m_Inventory.activeSelf)
            {
                PauseMenuManager.Instance.SetIsCantOpenPauseMenu(true); //don't allow to open pause menu
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near chest
        {
            m_Player = collision.GetComponent<Player>(); //get player reference
            SetActiveInteractionButton(true); //show chest ui
        }

        if (chestType == ChestType.Destroyable) //if chest is destroyable
        {
            if (collision.CompareTag("PlayerAttackRange") & Health > 0) //if player is attacking chest and it health is greater than zero
            {
                ShowHitParticles(collision.transform.parent.transform.localScale.x); //show hit particles
                DamageChest(); //damage chest
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is leave chest
        {
            SetActiveInteractionButton(false); //disable chest ui
            if (m_Inventory.activeSelf) SetActiveInventory(false); //if chest inventory is open - close it

            m_Player = null; //remove player reference
        }
    }

    private void ShowHitParticles(float playerLook)
    {
        var hitParticlesInstantiate = Instantiate(m_HitEffect);
        hitParticlesInstantiate.transform.position = transform.position;

        if (playerLook == 1) //where player look
            hitParticlesInstantiate.transform.rotation = 
                new Quaternion(hitParticlesInstantiate.transform.rotation.x, hitParticlesInstantiate.transform.rotation.y * -1, hitParticlesInstantiate.transform.rotation.z, hitParticlesInstantiate.transform.rotation.w);

        Destroy(hitParticlesInstantiate, 1.5f); //destroy particles after 1.5s
    }

    private void DamageChest()
    {
        Health -= 1; //remove 1 health from the chest hp

        if (Health == 0)
        {
            ChangeChestSprite();
        }
        else
        {
            m_Animator.SetTrigger("TakeDamage"); //play take damage animation
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
        var grid = m_Inventory.transform.GetChild(0); //get inventory grid

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
                    Health = 0;
                    Destroy(m_InstantChestContainItems);
                }

                break;
            }
        }
    }

    #endregion
}

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

    [Header("Effects")]
    [SerializeField] private Sprite m_OpenChestSprite;
    [SerializeField] private Audio ChestOpenAudio;

    private GameObject m_InteractionButton; //chest ui
    private Player m_Player; //player
    private Animator m_Animator; //chest animator
    private bool isChestEmpty; //indicate is chest empty

    #endregion

    #region private methods

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start () {

        InitializeInteractionButton();

        ActiveInteractionButton(false);

        ActiveInventory(false);
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI");
        m_InteractionButton = Instantiate(interactionButton, transform) as GameObject;
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
        }
    }

    private void OpenChest()
    {
        if (chestType == ChestType.Destroyable & Health != 0) //if chest is destroyable but still have health
        {
            var chestInfo = LocalizationManager.Instance.GetItemsLocalizedValue("chest_info");
            AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message(chestInfo)); //display warning message
        }
        else //if chest can be open
        {
            ActiveInventory(!m_Inventory.activeSelf); //show or hide chest inventory

            if (!isChestEmpty) //is chest is empty
                IsChestEmpty(); //change chest sprite
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near chest
        {
            m_Player = collision.GetComponent<Player>(); //get player reference
            ActiveInteractionButton(true); //show chest ui
        }

        if (chestType == ChestType.Destroyable) //if chest is destroyable
        {
            if (collision.CompareTag("PlayerAttackRange") & Health > 0) //if player is attacking chest and it health is greater than zero
            {
                StopAllCoroutines();
                ShowHitParticles(collision.transform.parent.transform.localScale.x); //show hit particles
                StartCoroutine(DamageChest()); //damage chest
            }
        }
    }

    private void ShowHitParticles(float playerLook)
    {
        var hitParticles = Resources.Load("Effects/ChestHit") as GameObject;
        var hitParticlesInstantiate = Instantiate(hitParticles);
        hitParticlesInstantiate.transform.position = transform.position;

        if (playerLook == 1) //where player look
            hitParticlesInstantiate.transform.rotation = 
                new Quaternion(hitParticlesInstantiate.transform.rotation.x, hitParticlesInstantiate.transform.rotation.y * -1, hitParticlesInstantiate.transform.rotation.z, hitParticlesInstantiate.transform.rotation.w);

        Destroy(hitParticlesInstantiate, 1.5f); //destroy particles after 1.5s
    }

    private IEnumerator DamageChest()
    {
        Health -= 1; //remove 1 health from the chest hp

        m_Animator.SetBool("IsTakeDamage", true); //play take damage animation

        yield return new WaitForSeconds(0.2f);

        m_Animator.SetBool("IsTakeDamage", false); //stop playing take damage animation

        if (Health <= 0) //if health is less or equal zero
        {
            OpenChest(); //open chest
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is leave chest
        {
            ActiveInteractionButton(false); //disable chest ui
            if (m_Inventory.activeSelf) ActiveInventory(false); //if chest inventory is open - close it

            m_Player = null; //remove player reference

            if (!isChestEmpty) //if chest is empty
                IsChestEmpty(); //change chest sprite
        }
    }

    private void ChangeChestSprite()
    {
        isChestEmpty = true; //chest is empty

        GetComponent<SpriteRenderer>().sprite = m_OpenChestSprite;

        transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f); //change chest position
    }

    #region Active

    private void ActiveInteractionButton(bool active)
    {
        m_InteractionButton.SetActive(active);
    }

    private void ActiveInventory(bool active)
    {
        if (m_Player != null) m_Player.TriggerPlayerBussy(active); //allow or dont allow player to attack when chest inventory is open

        AudioManager.Instance.Play(ChestOpenAudio); //play chest open sound

        m_Inventory.SetActive(active); //show or hide chest inventory
    }

    #endregion

    #endregion

    #region public methods

    public void IsChestEmpty() //check is chest empty
    {
        if (m_Inventory.transform.GetChild(0).childCount == 0 & !isChestEmpty) //if chest inventory doesn't have childs
        {
            ChangeChestSprite(); //chest have to be empty
        }
    }

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
                    ChangeChestSprite(); //chest is empty

                break;
            }
        }
    }

    #endregion
}

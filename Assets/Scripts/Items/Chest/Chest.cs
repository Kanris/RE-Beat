using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Chest : MonoBehaviour {

    public enum ChestType { Common, Destroyable }

    public ChestType chestType;
    public int Health = 0; 

    private GameObject m_InteractionButton;
    private Player m_Player;
    private GameObject m_Inventory;
    private Animator m_Animator;
    private bool isChestEmpty;

	// Use this for initialization
	void Start () {

        InitializeInteractionButton();

        InitializeInventory();

        InitializeAnimator();

        ActiveInteractionButton(false);

        ActiveInventory(false);
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI");
        m_InteractionButton = Instantiate(interactionButton, transform) as GameObject;
    }

    private void InitializeInventory()
    {
        if (transform.childCount > 0)
        {
            m_Inventory = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("Chest.InitializeInventory: Chest has no grid on it");
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Chest.InitializeAnimator: Can't find animator on gameObject");
        }
    }

    #endregion

    private void Update()
    {
        if (m_Player != null)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                OpenChest();
            }
        }
    }

    private void OpenChest()
    {
        if (chestType == ChestType.Destroyable & Health != 0)
        {
            AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message("This chest is too rusty, can't open it."));
        }
        else
        {
            ActiveInventory(!m_Inventory.activeSelf);

            if (!isChestEmpty)
                IsChestEmpty();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_Player = collision.GetComponent<Player>();
            ActiveInteractionButton(true);
        }

        if (collision.CompareTag("PlayerAttackRange") & Health > 0)
        {
            StopAllCoroutines();
            StartCoroutine( DamageChest(PlayerStats.DamageAmount) );
        }
    }

    private IEnumerator DamageChest(int amount)
    {
        Health -= amount;

        m_Animator.SetBool("IsTakeDamage", true);

        yield return new WaitForSeconds(0.2f);

        m_Animator.SetBool("IsTakeDamage", false);

        if (Health <= 0)
        {
            OpenChest();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ActiveInteractionButton(false);
            ActiveInventory(false);

            m_Player = null;

            if (!isChestEmpty)
                IsChestEmpty();
        }
    }

    public void IsChestEmpty()
    {
        if (m_Inventory.transform.GetChild(0).childCount == 0 & !isChestEmpty)
        {
            ChangeChestSprite();
        }
    }

    public void RemoveFromChest(string name)
    {
        var grid = m_Inventory.transform.GetChild(0);

        for (int index = 0; index < grid.childCount; index++)
        {
            var gridChildren = grid.GetChild(index);
            if (gridChildren.name == name)
            {
                gridChildren.SetParent(null);
                Destroy(gridChildren.gameObject);

                if (grid.childCount == 0)
                    ChangeChestSprite();

                break;
            }
        }
    }

    private void ChangeChestSprite()
    {
        Health = 0;

        isChestEmpty = true;

        m_Animator.SetTrigger("Open");
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f);

        /*var openChestSprite = Resources.LoadAll<Sprite>("Sprites/Props")[9];

        GetComponent<SpriteRenderer>().sprite = openChestSprite;*/
    }

    #region Active

    private void ActiveInteractionButton(bool active)
    {
        m_InteractionButton.SetActive(active);
    }

    private void ActiveInventory(bool active)
    {
        if (m_Player != null) m_Player.TriggerPlayerBussy(active);

        m_Inventory.SetActive(active);
    }

    #endregion
}

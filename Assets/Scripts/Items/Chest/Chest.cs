using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Chest : MonoBehaviour {

    private GameObject m_InteractionButton;
    private Player m_Player;
    private GameObject m_Inventory;

	// Use this for initialization
	void Start () {

        InitializeInteractionButton();

        InitializeInventory();

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

    #endregion

    private void Update()
    {
        if (m_Player != null)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                m_Player.isPlayerBusy = !m_Inventory.activeSelf;
                ActiveInventory(!m_Inventory.activeSelf);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_Player = collision.GetComponent<Player>();
            ActiveInteractionButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_Player = null;
            ActiveInteractionButton(false);
            ActiveInventory(false);
        }
    }

    #region Active

    private void ActiveInteractionButton(bool active)
    {
        m_InteractionButton.SetActive(active);
    }

    private void ActiveInventory(bool active)
    {
        m_Inventory.SetActive(active);
    }

    #endregion
}

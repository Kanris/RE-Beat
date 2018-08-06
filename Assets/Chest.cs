using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Chest : MonoBehaviour {

    private GameObject m_InteractionButton;
    private bool m_IsPlayerNear;
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
        if (m_IsPlayerNear)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                ActiveInventory(!m_Inventory.activeSelf);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            m_IsPlayerNear = true;
            ActiveInteractionButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsPlayerNear)
        {
            m_IsPlayerNear = false;
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

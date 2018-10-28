using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Trader : MonoBehaviour {

    [SerializeField] private GameObject m_InteractionUI;
    [SerializeField] private GameObject m_StoreUI;
    [SerializeField] private GameObject m_DescriptionUI;

    private bool m_IsPlayerNear;
	
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
            m_IsPlayerNear = true;

            collision.GetComponent<Player>().TriggerPlayerBussy(true);
            AnnouncerManager.Instance.ShowScrapAmount(true);
            m_InteractionUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = false;
            m_InteractionUI.SetActive(false);
            m_DescriptionUI.SetActive(false);
            m_StoreUI.SetActive(false);

            collision.GetComponent<Player>().TriggerPlayerBussy(false);
            AnnouncerManager.Instance.ShowScrapAmount(false);
        }
    }

    public void ShowItemDescription()
    {
        m_DescriptionUI.SetActive(true);
    }

    public void BuyItem(int coins)
    {
        PlayerStats.Scrap = -coins;
    }

}

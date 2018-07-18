using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableBackground : MonoBehaviour {

    private bool m_PlayerInCave;
    private GameObject m_Background;

    private void Start()
    {
        InitializeBackground();
    }

    private void InitializeBackground()
    {
        m_Background = GameObject.FindWithTag("BackgroundImage");

        if (m_Background == null)
        {
            Debug.LogError("DisableBackground: Can't find gameObject with BackgroundImage tag");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_PlayerInCave)
        {
            m_PlayerInCave = true;
            ManageBackground();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_PlayerInCave)
        {
            m_PlayerInCave = false;
            ManageBackground();
        }
    }


    private void ManageBackground()
    {
        if (m_Background != null)
            m_Background.SetActive(!m_PlayerInCave);
        else
            Debug.LogError("DisableBackground.ManageBackground: m_Background is not initialized");
    }
}

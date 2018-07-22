using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class DisableBackground : MonoBehaviour {

    private bool m_PlayerInCave;
    private GameObject m_Background;
    private TilemapRenderer m_TilemapRenderer;

    private void Start()
    {
        InitializeBackground();

        InitializeTilemapRenderer();
    }

    private void InitializeBackground()
    {
        m_Background = GameObject.FindWithTag("BackgroundImage");

        if (m_Background == null)
        {
            Debug.LogError("DisableBackground: Can't find gameObject with BackgroundImage tag");
        }
    }

    private void InitializeTilemapRenderer()
    {
        m_TilemapRenderer = GetComponent<TilemapRenderer>();

        if (m_TilemapRenderer == null)
        {
            Debug.LogError("DisableBackground.InitializeTilemapRenderer: Can't find component - TilemapRenderer");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_PlayerInCave)
        {
            m_PlayerInCave = true;
            ManageBackground();
            m_TilemapRenderer.sortingOrder = -10;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_PlayerInCave)
        {
            m_PlayerInCave = false;
            ManageBackground();
            m_TilemapRenderer.sortingOrder = 10;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class DisableBackground : MonoBehaviour {

    private bool m_PlayerInCave;
    private GameObject m_Background;
    private TilemapRenderer m_TilemapRenderer;

    private Tilemap m_MistTilemap;
    private Animator m_Animator;
    private bool m_IsFading;

    private void Start()
    {
        InitializeBackground();

        InitializeTilemapRenderer();

        InitializeMist();
    }

    private void Update()
    {
        if (m_PlayerInCave & GameMaster.Instance.isPlayerDead)
        {
            StartCoroutine(PlayerLeaveCave(true));
        }
    }

    private void InitializeMist()
    {
        var mistGameObject = GameObject.FindWithTag("Mist");

        if (mistGameObject != null)
        {
            m_MistTilemap = mistGameObject.GetComponent<Tilemap>();
            m_Animator = mistGameObject.GetComponent<Animator>();
        }
        else
            Debug.LogError("DisableBackground.InitializeMist: Can't find Gameobject with tag - Mist");
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
            PlayerEnterCave();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_PlayerInCave)
        {
            StartCoroutine(PlayerLeaveCave(false));
        }
    }

    private void PlayerEnterCave()
    {
        StartCoroutine(FadeToClear());
        m_PlayerInCave = true;
        ManageBackground();
    }

    private IEnumerator PlayerLeaveCave(bool isNeedWaiting)
    {
        m_PlayerInCave = false;
        
        if (isNeedWaiting)
            yield return new WaitForSeconds(1.5f);
        else
            yield return null;

        yield return FadeToBlack();
        ManageBackground();
    }

    private void ChangeBackgroundOrder()
    {
        m_TilemapRenderer.sortingOrder = -m_TilemapRenderer.sortingOrder;
    }

    private void ManageBackground()
    {
        if (m_Background != null)
            m_Background.SetActive(!m_PlayerInCave);
        else
            Debug.LogError("DisableBackground.ManageBackground: m_Background is not initialized");
    }

    private IEnumerator FadeToClear()
    {
        yield return FadeTo("FadeOut");
    }

    private IEnumerator FadeToBlack()
    {
        yield return FadeTo("FadeIn");
    }

    private IEnumerator FadeTo(string trigger)
    {
        m_IsFading = true;
        m_Animator.SetTrigger(trigger);

        while (m_IsFading)
            yield return null;
    }

    public void AnimationComplete()
    {
        m_IsFading = false;
    }
}

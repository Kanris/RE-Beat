using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class DisableBackground : MonoBehaviour {

    private bool m_PlayerInCave;
    private Animator m_BackgroundAnimator;
    private TilemapRenderer m_TilemapRenderer;

    private Tilemap m_MistTilemap;
    private Animator m_MistAnimator;
    private bool m_IsFading;

    private void Start()
    {
        InitializeTilemapRenderer();

        InitializeMist();

        InitializeBackground();
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
            m_MistAnimator = mistGameObject.GetComponent<Animator>();
        }
        else
            Debug.LogError("DisableBackground.InitializeMist: Can't find Gameobject with tag - Mist");
    }

    private void InitializeBackground()
    {
        var m_Background = GameObject.FindWithTag("BackgroundImage");

        if (m_Background == null)
        {
            Debug.LogError("DisableBackground: Can't find gameObject with BackgroundImage tag");
        }
        else
        {
            m_BackgroundAnimator = m_Background.GetComponent<Animator>();
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

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_PlayerInCave)
        {
            yield return PlayerLeaveCave(false);
        }
    }

    private void PlayerEnterCave()
    {
        StartCoroutine(FadeToClear());
        m_PlayerInCave = true;
    }

    private IEnumerator PlayerLeaveCave(bool isNeedWaiting)
    {
        m_PlayerInCave = false;
        
        if (isNeedWaiting)
            yield return new WaitForSeconds(1.5f);
        else
            yield return null;

        yield return FadeToBlack();
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

        if (m_MistAnimator != null) m_MistAnimator.SetTrigger(trigger);

        if (m_BackgroundAnimator != null) m_BackgroundAnimator.SetTrigger(trigger);

        while (m_IsFading)
            yield return null;
    }

    public void AnimationComplete()
    {
        m_IsFading = false;
    }
}

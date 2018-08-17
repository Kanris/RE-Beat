using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class DisableBackground : MonoBehaviour {

    [SerializeField] private Animator BackgroundAnimator;
    [SerializeField] private Animator MistAnimator;
    [SerializeField] private bool isLight;
    [SerializeField] private GameObject Light;

    private bool m_PlayerInCave;
    private Tilemap m_MistTilemap;
    private bool m_IsFading;

    private void Start()
    {
        InitializeBackground();
        ChangeLightState(false);
    }

    private void Update()
    {
        if (m_PlayerInCave & GameMaster.Instance.isPlayerDead)
        {
            StartCoroutine(PlayerLeaveCave(true));
        }
    }

    #region Initialize

    private void InitializeBackground()
    {
        var m_Background = GameObject.FindWithTag("BackgroundImage");

        if (m_Background != null)
        {
            BackgroundAnimator = m_Background.GetComponent<Animator>();
        }
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_PlayerInCave)
        {
            PlayerEnterCave();
            ChangeLightState(true);
        }

        if (collision.CompareTag("Player") | collision.CompareTag("Enemy") | collision.CompareTag("Item"))
        {
            ChangeObjectMaterial(collision, true);
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_PlayerInCave)
        {
            yield return PlayerLeaveCave(false);
            ChangeLightState(false);
        }

        if (collision.CompareTag("Player") | collision.CompareTag("Enemy") | collision.CompareTag("Item"))
        {
            ChangeObjectMaterial(collision, false);
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

        if (MistAnimator != null) MistAnimator.SetTrigger(trigger);

        if (BackgroundAnimator != null) BackgroundAnimator.SetTrigger(trigger);

        while (m_IsFading)
            yield return null;
    }

    private void ChangeObjectMaterial(Collider2D collision, bool isEnter)
    {
        if (isLight)
        {
            var objectMaterialChange = collision.GetComponent<MaterialChange>();

            if (objectMaterialChange != null)
                objectMaterialChange.Change(isEnter);
        }

    }

    public void AnimationComplete()
    {
        m_IsFading = false;
    }

    private void ChangeLightState(bool isPlayerNear)
    {
        if (isLight)
        {
            if (Light != null)
            {
                Light.SetActive(isPlayerNear);
            }
        }
    }
}

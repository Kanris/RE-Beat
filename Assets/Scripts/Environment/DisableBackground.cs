using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class DisableBackground : MonoBehaviour {

    [SerializeField] private Animator BackgroundAnimator;
    [SerializeField] private Animator MistAnimator;

    private GameObject[] LightsInCave;
    private Tilemap m_MistTilemap;
    private bool m_PlayerInCave;
    private bool m_IsFading;

    private void Start()
    {
        InitializeBackground();
        InitializeLight();

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

    private void InitializeLight()
    {
        LightsInCave = GameObject.FindGameObjectsWithTag("CaveLight");
    }

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
            //collis
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
        if (LightsInCave.Length != 0)
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
        if (LightsInCave.Length != 0)
        {
            foreach (var light in LightsInCave)
            {
                if (light != null)
                    light.SetActive(isPlayerNear);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFaderManager : MonoBehaviour {

    #region Singleton
    public static ScreenFaderManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
    #endregion

    private Animator m_Animator;
    private GameObject m_ChildUIGameObject;
    private bool m_IsFading = false;

    private void Start()
    {
        InitializeUI();

        InitializeAnimator();

        SetActiveUI(false);
    }

    private void InitializeUI()
    {
        var childTransform = transform.GetChild(0);

        if (childTransform != null)
        {
            m_ChildUIGameObject = childTransform.gameObject;
        }
        else
        {
            Debug.LogError("ScreenFaderManager.InitializeUI: Can't find child object");
        }
    }

    private void InitializeAnimator()
    {
        m_Animator = m_ChildUIGameObject.GetComponentInChildren<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("ScreenFaderManager.InitializeAnimator: Can't find animator in child objects");
        }
    }

    private void SetActiveUI(bool active)
    {
        if (m_ChildUIGameObject != null)
            m_ChildUIGameObject.SetActive(active);
    }

    public IEnumerator FadeToClear()
    {
        yield return FadeTo("FadeOut");
    }

    public IEnumerator FadeToBlack()
    {
        yield return FadeTo("FadeIn");
    }

    private IEnumerator FadeTo(string trigger)
    {
        m_IsFading = true;
        SetActiveUI(m_IsFading);
        m_ChildUIGameObject.transform.SetSiblingIndex(999);
        m_Animator.SetTrigger(trigger);

        while (m_IsFading)
            yield return null;

        m_ChildUIGameObject.transform.SetSiblingIndex(-999);
        //SetActiveUI(m_IsFading);
    }

    public void AnimationComplete()
    {
        m_IsFading = false;
    }
}

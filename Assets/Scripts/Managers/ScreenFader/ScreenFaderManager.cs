using System.Collections;
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

    #region private fields

    #region serialize fields

    [SerializeField] private Animator m_BackgroundAnimator;
    [SerializeField] private GameObject m_UI;

    #endregion

    private bool m_IsFading = false;

    #endregion

    #region private methods

    private void Start()
    {
        m_UI.SetActive(false); //hide ui
    }

    private IEnumerator FadeTo(string trigger)
    {
        m_IsFading = true; //notify that fading starts

        m_UI.SetActive(m_IsFading); //show fade ui
        m_BackgroundAnimator.SetTrigger(trigger); //start animation

        while (m_IsFading)
            yield return null;

        if (trigger == "FadeOut")
            m_UI.SetActive(false);
    }

    #endregion

    #region public methods

    public void AnimationComplete()
    {
        m_IsFading = false; //notify that animation complete
    }

    public IEnumerator FadeToClear()
    {
        yield return FadeTo("FadeOut"); //start fade out animation
    }

    public IEnumerator FadeToBlack()
    {
        yield return FadeTo("FadeIn"); //start fade in animaiton
    }

    #endregion
}

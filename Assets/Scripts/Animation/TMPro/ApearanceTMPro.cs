using System.Collections;
using UnityEngine;
using TMPro;

public class ApearanceTMPro : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI m_TextToAnimate;

    [Header("Time")]
    [SerializeField] private float increment = .05f;

    [Header("Conditions")]
    [SerializeField] private bool m_PlayOnEnable = false;

    public void TextAppearance()
    {
        m_TextToAnimate.color = m_TextToAnimate.color.ChangeColor(a: 0f);

        if (!m_TextToAnimate.gameObject.activeSelf)
            m_TextToAnimate.gameObject.SetActive(true);

        StartCoroutine(AnimateAppearanceText());
    }

    private IEnumerator AnimateAppearanceText()
    {
        var value = 0f;

        while (m_TextToAnimate.color.a < 1)
        {
            m_TextToAnimate.color = m_TextToAnimate.color.ChangeColor(a: value);

            yield return new WaitForSecondsRealtime(increment);

            value += increment;
        }
    }

    private void OnEnable()
    {
        if (m_PlayOnEnable)
        {
            TextAppearance();
        }
    }
}

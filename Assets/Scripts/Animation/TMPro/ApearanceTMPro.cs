using System.Collections;
using UnityEngine;
using TMPro;

public class ApearanceTMPro : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI m_TextToAnimate;

    public void TextAppearance()
    {
        if (!m_TextToAnimate.gameObject.activeSelf)
            m_TextToAnimate.gameObject.SetActive(true);

        StartCoroutine(AnimateAppearanceText());
    }

    private IEnumerator AnimateAppearanceText()
    {
        var value = 0f;

        const float increment = 0.02f;

        while (m_TextToAnimate.color.a < 1)
        {
            m_TextToAnimate.color = m_TextToAnimate.color.ChangeColor(a: value);

            yield return new WaitForSeconds(increment);

            value += increment;
        }
    }
}

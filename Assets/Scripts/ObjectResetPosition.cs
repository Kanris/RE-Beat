using UnityEngine;
using System.Collections;
using TMPro;

public class ObjectResetPosition : MonoBehaviour {

    [SerializeField] private MagneticBox m_ObjectToReset;

    [Header("UI")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton;

    [Header("Timer")]
    [SerializeField] private GameObject m_TimerUI;
    [SerializeField] private TextMeshProUGUI m_TimerText;

    private bool m_IsCanReset = true;

    private void Start()
    {
        SetActiveUI(false);
        m_TimerUI.SetActive(false);

        m_InteractionUIButton.PressInteractionButton = ActivateStation;
        m_ObjectToReset.OnBoxDestroy += ChangeBoxToObserve;
    }

    private void ActivateStation()
    {
        if (m_IsCanReset)
        {
            m_IsCanReset = false;
            m_ObjectToReset.ResetPosition();

            StartCoroutine(ShowTimer());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_InteractionUIButton.SetIsPlayerNear(true);
            SetActiveUI(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_InteractionUIButton.SetIsPlayerNear(false);
            SetActiveUI(false);
        }
    }

    private void SetActiveUI(bool value)
    {
        m_InteractionUIButton.SetActive(value);
    }

    private IEnumerator ShowTimer()
    {
        var value = 5;

        m_TimerUI.SetActive(true);

        while (value > 0)
        {
            m_TimerText.text = "0:0" + value;
            yield return new WaitForSeconds(1f);
            value--;
        }

        m_TimerUI.SetActive(false);
        m_IsCanReset = true;
    }

    private void ChangeBoxToObserve(MagneticBox newBox)
    {
        m_ObjectToReset = newBox;
        m_ObjectToReset.OnBoxDestroy += ChangeBoxToObserve;
    }
}

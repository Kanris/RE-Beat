using UnityEngine;
using System.Collections;
using TMPro;

public class ObjectResetPosition : MonoBehaviour {

    [SerializeField] private MagneticBox m_ObjectToReset;

    [Header("UI")]
    [SerializeField] private GameObject m_InteractionUI;
    [SerializeField] private GameObject m_TimerUI;
    [SerializeField] private TextMeshProUGUI m_TimerText;

    private bool m_IsCanReset = true;
    private bool m_IsPlayerNear = false;

    private void Start()
    {
        SetActiveUI(false);

        m_TimerUI.SetActive(false);

        m_ObjectToReset.OnBoxDestroy += ChangeBoxToObserve;
    }

    // Update is called once per frame
    void Update () {
		
        if (m_IsPlayerNear)
        {
            if (m_IsCanReset)
            {
                if (GameMaster.Instance.m_Joystick.LeftStickY > .9f || GameMaster.Instance.m_Joystick.DPadUp.WasPressed)
                {
                    m_IsCanReset = false;
                    m_ObjectToReset.ResetPosition();

                    StartCoroutine(ShowTimer());
                }
            }
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = true;
            SetActiveUI(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = false;
            SetActiveUI(false);
        }
    }

    private void SetActiveUI(bool value)
    {
        m_InteractionUI.SetActive(value);
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

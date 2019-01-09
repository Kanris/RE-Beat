using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class DistruptionSignal : MonoBehaviour
{
    [SerializeField] private PlayerInTrigger m_PlayerInTrigger;

    private Camera2DFollow m_Camera;
    private bool m_IsPlayerNear;

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = Camera.main.GetComponent<Camera2DFollow>();
        m_PlayerInTrigger.OnPlayerInTrigger += ChangeIsPlayerNear;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsPlayerNear)
        {
            if (GameMaster.Instance.IsPlayerDead)
            {
                m_IsPlayerNear = false;
                m_Camera.StopLowHealthEffect();
            }
        }
    }

    private void OnDestroy()
    {
        Debug.LogError("Is player Near> " + m_IsPlayerNear);

        if (m_IsPlayerNear)
        {
            m_Camera.StopLowHealthEffect();

            UIManager.Instance.DisplayNotificationMessage
                ("Distruption signal is disappear", UIManager.Message.MessageType.Message);
        }
    }
    
    private void ChangeIsPlayerNear(bool value, Transform target)
    {
        StopAllCoroutines();

        Debug.LogError(value);

        m_IsPlayerNear = value;

        if (m_IsPlayerNear)
        {
            StartCoroutine(PlayerInTrigger());
        }
        else
        {
            StartCoroutine(PlayerLeaveTrigger());
        }
    }

    private IEnumerator PlayerInTrigger()
    {
        yield return new WaitForSeconds(1f);

        UIManager.Instance.DisplayNotificationMessage
            ("Distruption signal is nearby", UIManager.Message.MessageType.Message);

        m_Camera.PlayLowHealthEffect();
    }

    private IEnumerator PlayerLeaveTrigger()
    {
        yield return new WaitForSeconds(1f);

        m_Camera.StopLowHealthEffect();
    }
}

using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

public class DistruptionSignal : MonoBehaviour
{
    [SerializeField] private PlayerInTrigger m_PlayerInTrigger; //indicates that player in disruption zone or not

    private Camera2DFollow m_Camera; //camera to activate/deactivate disruption effect
    private bool m_IsPlayerNear; //is player in zone or not (to diactivate disruption effect)

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
        if (m_IsPlayerNear)
        {
            m_Camera.StopLowHealthEffect();
            ChangePlayerStats(true);

            UIManager.Instance.DisplayNotificationMessage
                ("Distruption signal is disappear", UIManager.Message.MessageType.Message);
        }
    }
    
    private void ChangeIsPlayerNear(bool value, Transform target)
    {
        StopAllCoroutines();

        m_IsPlayerNear = value;
        ChangePlayerStats(!m_IsPlayerNear);

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

    private void ChangePlayerStats(bool value)
    {
        if (!GameMaster.Instance.IsPlayerDead)
            GameMaster.Instance.m_Player?.transform.GetChild(0).GetComponent<Player>().playerStats.SetChipsAvailability(value);
    }
}

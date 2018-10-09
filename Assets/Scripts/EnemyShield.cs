using UnityEngine;

public class EnemyShield : MonoBehaviour {

    public delegate void VoidBoolDelegate(bool value);
    public event VoidBoolDelegate OnShieldDestroy;

    public DebuffPanel.DebuffTypes m_DebuffType;

    [Header("Durations")]
    [SerializeField, Range(1f, 5f)] private float m_DebuffDuration = 5f;
    [SerializeField, Range(1f, 5f)] private float m_ShieldDuration = 2f;

    private bool m_IsQuitting; //is application closing

    // Use this for initialization
    private void Start () {

        GetComponent<SpriteRenderer>().color = GetShieldColor();

        SubscribeToEvents();

        Destroy(gameObject, m_ShieldDuration);
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //is player return to the start screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //is player move to the next scene

    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting)
        {
            if (OnShieldDestroy != null)
            {
                OnShieldDestroy(false);
            }
        }
    }

    private Color GetShieldColor()
    {
        var shieldColor = new Color(255, 255, 255, 0.5f);

        switch (m_DebuffType)
        {
            case DebuffPanel.DebuffTypes.Cold:
                shieldColor = new Color(0, 255, 227, 0.5f);
                break;

            case DebuffPanel.DebuffTypes.Defense:
                shieldColor = new Color(255, 0, 183, 0.5f);
                break;

            case DebuffPanel.DebuffTypes.Fire:
                shieldColor = new Color(253, 2, 2, 0.5f);
                break;
        }
        
        return shieldColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttackRange"))
        {
            collision.transform.parent.GetComponent<Player>()
                .playerStats.DebuffPlayer(m_DebuffType, m_DebuffDuration);
        }
        else if (collision.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }
}

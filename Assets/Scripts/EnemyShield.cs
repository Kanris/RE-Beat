using UnityEngine;

public class EnemyShield : MonoBehaviour {

    public DebuffPanel.DebuffTypes m_DebuffType;

    [Header("Durations")]
    [SerializeField, Range(1f, 5f)] private float m_DebuffDuration = 5f;
    [SerializeField, Range(1f, 5f)] private float m_ShieldDuration = 2f;

	// Use this for initialization
	private void Start () {

        Destroy(gameObject, m_ShieldDuration);

        GetComponent<SpriteRenderer>().color = GetShieldColor();
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
            collision.transform.parent.GetComponent<Player>().playerStats.DebuffPlayer(m_DebuffType, m_DebuffDuration);
        }
    }
}

using UnityEngine;

public class ReachablePoint : MonoBehaviour {

    [SerializeField] private Transform m_NearestTunnel; //tunnel where companion will be spawned

    private SpriteRenderer m_SpriteRenderer; //reachable point sprite
    private bool m_IsActive; //is this reachable point is actvie

    private void Start()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>(); //get gameobject's spriterenderer

        GetComponent<Animator>()?.SetFloat("Speed", Random.Range(.8f, 1.2f)); //change animator speed
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!GameMaster.Instance.IsPlayerDead) //if player is not dead
        {
            if (collision.CompareTag("Player") && !m_IsActive) //if player triggered reacheble point
            {
                //set reachable point transform
                GameMaster.Instance.SetReachablePoint(transform, m_NearestTunnel);

                //activate point effect
                SetActivatePoint(true);

                //display to the player that revive with companion is available
                UIManager.Instance.SetReviveAvailable(true);
                UIManager.Instance
                    .DisplayNotificationMessage("Companion can now revive you", UIManager.Message.MessageType.Message);
            }
        }
    }

    //change point color to indicate is it active or not
    public void SetActivatePoint(bool value)
    {
        var color = Color.white;

        if (value)
        {
            ColorUtility.TryParseHtmlString("#FF56BC", out color);
        }

        m_IsActive = value;
        m_SpriteRenderer.color = color;
    }

}

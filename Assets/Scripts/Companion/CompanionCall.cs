using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

public class CompanionCall : MonoBehaviour {

    [Header("Spawn")]
    [SerializeField] private GameObject m_Player; //player's prefab
    [SerializeField] private GameObject m_Companion; //companion's prefab

    [Header("Additional")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton; //button interaction ui

    private SpriteRenderer m_StationImage;
    private bool m_IsPlayer; //is player triggered
    private GameObject m_WhoTriggered; //gameobject that triggered
    private bool m_IsChanging; //is spawn new character

	// Use this for initialization
	void Start () {

        m_InteractionUIButton.PressInteractionButton = ActivateStation;
        m_InteractionUIButton.SetActive(false); //hide interaction button

        m_IsPlayer = true;

        m_StationImage = GetComponent<SpriteRenderer>();
    }

    private void ActivateStation()
    {
        if (m_InteractionUIButton.ActiveSelf())
        {
            if (!m_IsChanging)
            {
                StartCoroutine(ChangeCharacter()); //change character
            }
        }
    }

    private IEnumerator ChangeCharacter()
    {
        m_IsChanging = true; //character is changing

        yield return Camera.main.GetComponent<Camera2DFollow>().PlayReviveEffect(); //play change effect

        Destroy(m_WhoTriggered); //destroy object that triggers station

        var whoToSpawn = m_Player; //spawn player

        if (m_IsPlayer) //if player triggered station
        {
            whoToSpawn = m_Companion; //spawn companion
            m_StationImage.color = Color.magenta;
        }
        else
        {
            m_StationImage.color = new Color(.549f, .980f, .984f);
        }

        GameMaster.Instance.m_Player = Instantiate(whoToSpawn, transform.position, transform.rotation); //instantiate gameobject

        GameMaster.Instance.IsPlayerDead = m_IsPlayer;
        m_IsChanging = false; //character was change
        m_IsPlayer = !m_IsPlayer;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player near station
        {
            m_InteractionUIButton.SetActive(true); //show interaction button

            m_WhoTriggered = collision.transform.parent.gameObject;//save companion gameobject

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leave station trigger
        {
            m_InteractionUIButton.SetActive(false); //hide interaction button
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

public class CompanionCall : MonoBehaviour {

    [SerializeField] private GameObject m_InteractionUI; //button interaction ui

    [Header("Spawn")]
    [SerializeField] private GameObject m_Player; //player's prefab
    [SerializeField] private GameObject m_Companion; //companion's prefab

    private SpriteRenderer m_StationImage;
    private bool m_IsPlayer; //is player triggered
    private GameObject m_WhoTriggered; //gameobject that triggered
    private bool m_IsChanging; //is spawn new character

	// Use this for initialization
	void Start () {

        m_InteractionUI.SetActive(false); //hide interaction button

        m_IsPlayer = true;

        m_StationImage = GetComponent<SpriteRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
		
        if (m_InteractionUI.activeSelf) //if player is near
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit") & !m_IsChanging) //if submit button pressed and spawn is not in progress
            {
                StartCoroutine( ChangeCharacter() ); //change character
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

        Instantiate(whoToSpawn, transform.position, transform.rotation); //instantiate gameobject

        m_IsChanging = false; //character was change
        m_IsPlayer = !m_IsPlayer;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player near station
        {
            m_InteractionUI.SetActive(true); //show interaction button

            m_WhoTriggered = collision.transform.parent.gameObject;//save companion gameobject

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leave station trigger
        {
            m_InteractionUI.SetActive(false); //hide interaction button
        }
    }
}

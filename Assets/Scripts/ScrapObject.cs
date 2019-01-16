using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapObject : MonoBehaviour {

    [SerializeField] private Transform m_Target; //target to movoe this scrap object

    [Header("Effects")]
    [SerializeField] private GameObject m_HitParticles; //particles that will be created when scrapobject got to the target
    [SerializeField] private Audio m_GotToPlayer; //audio that will player when scrapobject got to the targe

    private int m_ScrapAmount = 0; //amount of scraps to add

	// Update is called once per frame
	void Update () {
		
        if (m_Target != null) //if there is target
        {
            //move scrapobject to the target
            transform.position = Vector3.MoveTowards(transform.position, m_Target.position, 10f * Time.deltaTime);
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if scrapobject hit player
        {
            PlayerStats.Scrap = m_ScrapAmount; //add scraps to the player

            AudioManager.Instance.Play(m_GotToPlayer); //play sound that scraps add to the player

            Destroy( Instantiate(m_HitParticles, m_Target.position, Quaternion.identity), 1f ); //create hit particles

            Destroy(gameObject); //destroy this scrap object
        }
    }

    //initialize scrap object
    public void SetTarget(Transform target, int scrap)
    {
        m_ScrapAmount = scrap; //set amount of scrap
        m_Target = target; //set target
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapObject : MonoBehaviour {

    [SerializeField] private Transform m_Target;

    private int m_ScrapAmount = 0;

	// Update is called once per frame
	void Update () {
		
        if (m_Target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_Target.position, 10f * Time.deltaTime);
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats.Scrap = m_ScrapAmount;
            Destroy(gameObject);
        }
    }

    public void SetTarget(Transform target, int scrap)
    {
        m_ScrapAmount = scrap;
        m_Target = target;
    }
}

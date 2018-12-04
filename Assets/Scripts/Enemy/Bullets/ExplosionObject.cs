using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionObject : MonoBehaviour {

    [SerializeField] private LayerMask m_LayerMask;
    [SerializeField] private Transform m_Target;
    [SerializeField, Range(1f, 10f)] private float m_TimeBeforeDetonation = 10f;

    [Header("Effects")]
    [SerializeField] private GameObject m_DetonateParticles;

    // Use this for initialization
    void Start () {

        StartCoroutine(ExplosionSequence());

    }
	
	// Update is called once per frame
	void Update () {
		
        if (m_Target != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, m_Target.position, 2f * Time.deltaTime);
        }
	}

    private IEnumerator ExplosionSequence()
    {
        var currentTimer = 0f;

        while (currentTimer < m_TimeBeforeDetonation)
        {
            yield return new WaitForSeconds(1f);
            transform.localScale = transform.localScale.Add(x: 0.2f, y: 0.2f);
            currentTimer++;
        }

        Detonate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((m_LayerMask.value & 1 << collision.gameObject.layer) != 0) //if hit player - detonate
        {
            Detonate();
        }
        else if (collision.gameObject.layer == 14) //if hit ground - detonate
        {
            Detonate();
        }
    }

    private void Detonate()
    {
        var hit2D = Physics2D.OverlapCircle(transform.position, 1.4f, m_LayerMask); // player in range

        if (hit2D != null)
        {
            //set hit direction
            var fromWhereHit = hit2D.transform.position - transform.position;
            fromWhereHit.Normalize();

            hit2D.GetComponent<Player>().m_EnemyHitDirection = fromWhereHit.x > 0f ? 1 : -1;

            //player takes damage
            hit2D.GetComponent<Player>().playerStats.TakeDamage(1);
        }

        Destroy(
            Instantiate(m_DetonateParticles, transform.position, Quaternion.identity), 2f);

        Destroy(gameObject);
    }

    public void SetTarget(Transform target)
    {
        m_Target = target;
    }
}

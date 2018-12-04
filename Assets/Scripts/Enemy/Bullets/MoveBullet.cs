using UnityEngine;

public class MoveBullet : MonoBehaviour {

    [SerializeField] private LayerMask m_LayerMask;
    [Range(1, 10)] public int DamageAmount = 1;

	// Use this for initialization
	void Start () {

        Destroy(gameObject, 1f);

	}
	
	// Update is called once per frame
	void Update () {

        transform.Translate(Vector3.right * Time.deltaTime * 20f);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((m_LayerMask & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer))
        {
            var divider = 1;

            if (collision.transform.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(DamageAmount, divider);
            }
            else if (collision.transform.CompareTag("Enemy"))
            {
                if (collision.gameObject.GetComponent<EnemyStatsGO>() != null)
                {
                    collision.gameObject.GetComponent<EnemyStatsGO>().TakeDamage(null, 0, DamageAmount);
                }
                else
                    Destroy(gameObject);
            }

            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

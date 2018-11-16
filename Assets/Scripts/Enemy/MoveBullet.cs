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
            Stats statsToTakeDamage = null;
            var divider = 1;

            if (collision.transform.CompareTag("Player"))
            {
                statsToTakeDamage = collision.gameObject.GetComponent<Player>().playerStats;
            }
            else if (collision.transform.CompareTag("Enemy"))
            {
                if (collision.gameObject.GetComponent<EnemyStatsGO>() != null)
                {
                    statsToTakeDamage = collision.gameObject.GetComponent<EnemyStatsGO>().EnemyStats;
                    divider = 0;
                }
                else
                    Destroy(gameObject);
            }

            Damage(statsToTakeDamage, divider);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Damage(Stats statsToTakeDamage, int divider)
    {
        if (statsToTakeDamage != null)
            statsToTakeDamage.TakeDamage(DamageAmount, divider);

        Destroy(gameObject);
    }
}

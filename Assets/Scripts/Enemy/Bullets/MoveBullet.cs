using UnityEngine;

public class MoveBullet : MonoBehaviour {

    [SerializeField] private LayerMask m_LayerMask;
    [Range(1, 10)] public int DamageAmount = 1;

    [Header("Effects")]
    [SerializeField] private GameObject m_BulletHitPrefab;

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
            if (collision.transform.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(DamageAmount, 1);
            }
            else if (collision.transform.CompareTag("Enemy"))
            {
                if (collision.gameObject.GetComponent<EnemyStatsGO>() != null)
                {
                    collision.gameObject.GetComponent<EnemyStatsGO>().TakeDamage(null, 0, DamageAmount);
                }
            }

            CreateBulletHitEffect(collision);
            Destroy(gameObject);
        }
        else //hit ground
        {
            CreateBulletHitEffect(collision);
            Destroy(gameObject);
        }
    }

    private void CreateBulletHitEffect(Collision2D collision)
    {
        var bulletHitEffect = Instantiate(m_BulletHitPrefab);
        bulletHitEffect.transform.position = collision.contacts[0].point;

        Destroy(bulletHitEffect, 1f);
    }
}

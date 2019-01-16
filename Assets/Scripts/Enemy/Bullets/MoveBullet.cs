using UnityEngine;

public class MoveBullet : MonoBehaviour {

    [Header("Attack stats")]
    [SerializeField] private LayerMask m_LayerMask; //what bullet can hit
    [Range(1, 10)] public int DamageAmount = 1; //damage amount that bullet can deal

    [Header("Effects")]
    [SerializeField] private GameObject m_BulletHitPrefab; //destroying prefab particles

	// Use this for initialization
	void Start () {

        Destroy(gameObject, 1f); //destroy bullet after 1 sec

	}
	
	// Update is called once per frame
	void Update () {

        transform.Translate(Vector3.right * Time.deltaTime * 20f); //move bullet

    }

    //when bullet hit collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if collision is in layer mask that bullet can hit
        if (((m_LayerMask & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer))
        {
            DamageObject(collision.transform); //damage hited collision

            CreateBulletHitEffect(collision); //create destroying bullet particles
            Destroy(gameObject); //destroy bullet
        }
        else //hit ground
        {
            CreateBulletHitEffect(collision); //create destroying bullet particles
            Destroy(gameObject); //destroy bullet
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if trigger is in layer mask that bullet can hit
        if (((m_LayerMask & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer))
        {
            DamageObject(collision.transform); //damage hited collision
        }
    }

    private void DamageObject(Transform hitTransform)
    {
        if (hitTransform.CompareTag("Player")) //if hit player
        {
            //damage player
            hitTransform.GetComponent<Player>().playerStats.TakeDamage(DamageAmount, 1);
        }
        else if (hitTransform.CompareTag("Enemy")) //if hit enemy
        {
            //damage enemy
            hitTransform.GetComponent<EnemyStatsGO>()?.TakeDamage(null, 0, DamageAmount);
        }
        else if (hitTransform.CompareTag("WorldObject")) //if hit world object
        {
            //hit world object
            hitTransform.GetComponent<WorldObjectStats>().TakeDamage(true);
        }
    }

    //create destroying bullet particles
    private void CreateBulletHitEffect(Collision2D collision)
    {
        var bulletHitEffect = Instantiate(m_BulletHitPrefab); //create destroying bullet particles
        bulletHitEffect.transform.position = collision.contacts[0].point; //get collision hit point and move there bullet destroying particles

        Destroy(bulletHitEffect, 1f); //destroy bullet destroying particles after 1 sec
    }
}

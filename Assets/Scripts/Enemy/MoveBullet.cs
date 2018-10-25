using UnityEngine;

public class MoveBullet : MonoBehaviour {

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
        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);
            Destroy(gameObject);
        }
    }
}

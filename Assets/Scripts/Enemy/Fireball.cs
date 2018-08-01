using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour {

    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float DestroyTime;
    [HideInInspector] public int DamageAmount = 1;

    private Animator m_Animator;
    private bool isDestroying = false;

	// Use this for initialization
	void Start () {

        InitializeDirection();

        InitializeAnimator();

        DestroyTime = Time.time + 4f;
    }

    private void InitializeDirection()
    {
        if (Direction == Vector3.zero)
            Direction = Vector3.right;

        if (Direction == -Vector3.right)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("Fireball.InitializeAnimator: Can't find component - Animator on Gameobject");
        }
    }

    private void Update()
    {
        if (Time.time >= DestroyTime & !isDestroying)
            StartCoroutine(DestroyFireball());
    }

    // Update is called once per frame
    void FixedUpdate() {

        if (!isDestroying)
            transform.position += Direction * Time.fixedDeltaTime * 2.5f;

    }

    private IEnumerator OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") & !isDestroying)
        {
            DamagePlayer(collision);
        }

       if (!collision.gameObject.CompareTag("Enemy"))
            yield return DestroyFireball();
    }

    private void DamagePlayer(Collision2D collision)
    {
        collision.gameObject.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);
    }

    private IEnumerator DestroyFireball()
    {
        isDestroying = true;

        m_Animator.SetBool("isCollide", isDestroying);

        yield return new WaitForSeconds(0.25f);

        Destroy(gameObject);
    }
}

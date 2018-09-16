using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DestroyingWall : MonoBehaviour {
    
    [SerializeField] private int Health = 3;

    private Animator m_Animator;

	// Use this for initialization
	void Start () {

        m_Animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttackRange") & Health > 0)
        {
            StartCoroutine(WallHit(1));

            if (Health == 0)
            {
                GameMaster.Instance.SaveState(transform.name, 0, GameMaster.RecreateType.Object);
                Destroy(gameObject);
            }
            else
            {
                SpawnParticles(collision.transform.parent.transform.localScale.x);
            }
        }
    }

    private void SpawnParticles(float playerLook)
    {
        var hitParticle = Resources.Load("Effects/ChestHit") as GameObject;
        var hitParticlesInstantiate = Instantiate(hitParticle);
        hitParticlesInstantiate.transform.position = transform.position;

        if (playerLook == 1)
            hitParticlesInstantiate.transform.rotation =
                new Quaternion(hitParticlesInstantiate.transform.rotation.x, hitParticlesInstantiate.transform.rotation.y * -1, hitParticlesInstantiate.transform.rotation.z, hitParticlesInstantiate.transform.rotation.w);

        Destroy(hitParticlesInstantiate, 1.5f);
    }

    private IEnumerator WallHit(int amount)
    {
        Health -= amount;

        m_Animator.SetBool("Hit", true);

        yield return new WaitForSeconds(0.2f);

        m_Animator.SetBool("Hit", false);
    }
}

using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Animator))]
public class DestroyingWall : MonoBehaviour {

    #region private fields

    [SerializeField, Range(1, 5)] private int Health = 3; //wall health

    [Header("Effects")]
    [SerializeField] private GameObject m_HitEffect;

    private Animator m_Animator; //destroying wall animator

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        m_Animator = GetComponent<Animator>(); //reference to the wall animator

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttackRange") & Health > 0) //if player attack wall and wall is not destroyed
        {
            WallHit(); //hit wall

            if (Health == 0) //if wall health is 0
            {
                GameMaster.Instance.SaveState(transform.name, 0, GameMaster.RecreateType.Object); //save wall state
                Destroy(gameObject); //destroy wall
            }
            else //if wall health is greater than 0
            {
                ShowHitParticles(collision.transform.parent.transform.localScale.x); //show hit particles
            }
        }
    }

    private void ShowHitParticles(float playerLook)
    {
        var hitParticlesInstantiate = Instantiate(m_HitEffect);
        hitParticlesInstantiate.transform.position = transform.position;

        if (playerLook == 1)
            hitParticlesInstantiate.transform.rotation =
                new Quaternion(hitParticlesInstantiate.transform.rotation.x, hitParticlesInstantiate.transform.rotation.y * -1, hitParticlesInstantiate.transform.rotation.z, hitParticlesInstantiate.transform.rotation.w);

        Destroy(hitParticlesInstantiate, 1.5f); //destroy particles after 1.5s
    }

    private void WallHit()
    {
        Health -= 1; //remove 1 health from current wall health

        Camera.main.GetComponent<Camera2DFollow>().Shake(.05f, .2f);

        StartCoroutine(HitAnimation()); //play hit animation
    }

    private IEnumerator HitAnimation()
    {
        m_Animator.SetBool("Hit", true); //play hit animation

        yield return new WaitForSeconds(0.2f);

        m_Animator.SetBool("Hit", false); //stop playing hit animation
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(BoxCollider2D))]
public class MoveGrass : MonoBehaviour {

    #region private fields

    private Animator m_Animator;
    private int m_Health = 1;

    [SerializeField] private GameObject m_GrassParticles;

    #endregion

    #region private methods

    #region Initialize

    // Use this for initialization
    void Start () {

        m_Animator = GetComponent<Animator>();

    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Health > 0) //if player in grass
        {
            m_Animator.SetTrigger("Move"); //play move grass animation
        }

        if (collision.CompareTag("PlayerAttackRange") & m_Health > 0) //if player attack grass
        {
            m_Health--;
            DestroyGrass(); //show destroy grass animation
        }
    }

    private void DestroyGrass()
    {
        ShowDestroyParticles(); //show destroy particles
        m_Animator.SetTrigger("Hit"); //play grass hit animation
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.3f); //move grass a little below
    }

    private void ShowDestroyParticles()
    {
        var resourceDestroyParticlesInstantiate = Instantiate(m_GrassParticles);
        resourceDestroyParticlesInstantiate.transform.position = transform.position;

        Destroy(resourceDestroyParticlesInstantiate, 5f);
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(BoxCollider2D))]
public class MoveGrass : MonoBehaviour {

    private Animator m_Animator;

    #region Initialize

    // Use this for initialization
    void Start () {

        InitializeAnimator();

    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayMoveAnimation();
        }

        if (collision.CompareTag("PlayerAttackRange") )
        {
            DestroyGrass();
        }
    }

    private void PlayMoveAnimation()
    {
        m_Animator.SetTrigger("Move");
    }

    private void DestroyGrass()
    {
        ShowDestroyParticles();
        m_Animator.SetTrigger("Hit");
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.3f);
    }

    private void ShowDestroyParticles()
    {
        var resourceDestroyParticles = Resources.Load("Effects/Destroy grass") as GameObject;

        var resourceDestroyParticlesInstantiate = Instantiate(resourceDestroyParticles);
        resourceDestroyParticlesInstantiate.transform.position = transform.position;

        Destroy(resourceDestroyParticlesInstantiate, 5f);
    }

}

using UnityEngine;

[RequireComponent(typeof(Animator), typeof(BoxCollider2D))]
public class MoveGrass : MonoBehaviour {

    #region private fields

    [SerializeField] private GameObject m_GrassParticles; //grass destroy particles

    private Animator m_Animator; //grass animator
    private WorldObjectStats m_WorldObjectStats;
    private bool m_IsDestroyed; //is grass is destroyed

    #endregion

    #region private methods

    #region Initialize

    // Use this for initialization
    void Start () {

        m_Animator = GetComponent<Animator>(); //get grass animator
        InitializeWorldObjectStats(); //initialize world object stats
    }

    private void InitializeWorldObjectStats()
    {
        m_WorldObjectStats = GetComponent<WorldObjectStats>(); //get world object stats component from gameobject
        m_WorldObjectStats.OnHealthZero = DestroyGrass; //method that will be invoke when grass is destroyed
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsDestroyed) //if player in grass
        {
            m_Animator.SetTrigger("Move"); //play move grass animation
        }
    }

    private void DestroyGrass()
    {
        ShowDestroyParticles(); //show destroy particles
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

using UnityEngine;

[RequireComponent(typeof(Animator), typeof(BoxCollider2D))]
public class MoveGrass : MonoBehaviour {

    #region private fields

    [SerializeField] private GameObject m_GrassParticles; //grass destroy particles

    private Animator m_Animator; //grass animator
    private WorldObjectStats m_WorldObjectStats; //world object stats

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

    //when player enter to the grass zone
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Player") || collision.CompareTag("Enemy") || collision.CompareTag("Companion")) && !m_IsDestroyed) //if player in grass
        {
            if (collision.gameObject.GetComponent<Animator>() != null) //if player dash near grass
            {
                if (collision.gameObject.GetComponent<Animator>().GetBool("Dash"))
                    m_WorldObjectStats.TakeDamage(); //destroy grass
            }

            m_Animator.SetTrigger("Move"); //play move grass animation
        }
    }

    //when player stayed in the grass zone 
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !m_IsDestroyed) //if player in grass
        {
            if (collision.gameObject.GetComponent<Animator>().GetBool("Dash")) //if player dash near grass
                m_WorldObjectStats.TakeDamage(); //destroy grass
        }
    }

    private void DestroyGrass()
    {
        SetIsDestroyed(true); //indicates that grass is destroyed

        ShowDestroyParticles(); //show destroy particles
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.3f); //move grass a little below
    }

    private void ShowDestroyParticles()
    {
        var resourceDestroyParticlesInstantiate = Instantiate(m_GrassParticles); //create destroy particles on grass
        resourceDestroyParticlesInstantiate.transform.position = transform.position; //place destroy particles on grass position

        Destroy(resourceDestroyParticlesInstantiate, 5f); //destroy grass after 5 seconds
    }

    private void SetIsDestroyed(bool value)
    {
        m_IsDestroyed = value;
    }

    #endregion
}

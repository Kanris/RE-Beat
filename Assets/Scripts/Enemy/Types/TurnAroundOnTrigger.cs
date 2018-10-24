using UnityEngine;

public class TurnAroundOnTrigger : MonoBehaviour {

    #region serialize fields

    [SerializeField] private EnemyMovement m_EnemyMovement; //to turn around

    #endregion

    #region private methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_EnemyMovement.TurnAround(); //turn around if trigger behind the enemy
        }
    }

    #endregion
}

using UnityEngine;

public class StringTrigger : MonoBehaviour {

    #region private fields

    private Rigidbody2D m_Box; //box rigidbody
    private bool m_IsQuitting; //if application is closing

    #endregion

    #region private methods

    #region initialize

    private void Start()
    {
        ChangeIsQuitting(false); //aplication is not closing

        SubscribeToEvents(); //subscribe to the events
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //if player return to the start screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //if player is moving to the next scene
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("GravitySprite")) //if box is in the string trigger
        {
            m_Box = collision.GetComponent<Rigidbody2D>(); //get box collider

            if (m_Box != null)
            {
                m_Box.constraints = RigidbodyConstraints2D.FreezeAll; //dont allow box to move
            }
        }
        else if (collision.CompareTag("PlayerAttackRange")) //if player hit string
        {
            GameMaster.Instance.SaveState<int>(gameObject.name, 0, GameMaster.RecreateType.Object); //save string state
            Destroy(gameObject); //remove string
        }
    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true); //application is closing
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting) //if application is not closing
        {
            if (m_Box != null) //if m_box contains reference to the box rigidbody
            {
                m_Box.transform.SetParent(transform.parent); //change box parent

                m_Box.GetComponent<BoxCollider2D>().enabled = true;

                m_Box.constraints = RigidbodyConstraints2D.FreezePositionX //allow box to move on y position (fall because of gravity)
                    | RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    #endregion
}

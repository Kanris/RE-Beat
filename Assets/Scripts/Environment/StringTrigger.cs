using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringTrigger : MonoBehaviour {

    private Rigidbody2D m_Box;
    private bool m_IsQuitting;

    private void Start()
    {
        ChangeIsQuitting(false);

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("GravitySprite"))
        {
            m_Box = collision.GetComponent<Rigidbody2D>();

            if (m_Box != null)
            {
                m_Box.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        else if (collision.CompareTag("PlayerAttackRange"))
        {
            GameMaster.Instance.SaveState<int>(gameObject.name, 0, GameMaster.RecreateType.Object);
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting)
        {
            if (m_Box != null)
            {
                m_Box.transform.SetParent(transform.parent);

                m_Box.GetComponent<BoxCollider2D>().enabled = true;

                m_Box.constraints = RigidbodyConstraints2D.FreezePositionX
                    | RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }
}

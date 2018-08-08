using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringTrigger : MonoBehaviour {

    private Rigidbody2D m_Box;

    public static bool isQuitting;

    private void Start()
    {
        isQuitting = false;
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
            GameMaster.Instance.SaveBoolState(gameObject.name);
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (!isQuitting)
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
}

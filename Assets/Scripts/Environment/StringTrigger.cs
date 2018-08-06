using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringTrigger : MonoBehaviour {

    private Rigidbody2D m_Box;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("GravitySprite"))
        {
            m_Box = collision.GetComponent<Rigidbody2D>();

            if (m_Box != null)
            {
                m_Box.gravityScale = 0f;
                m_Box.velocity = Vector2.zero;
                m_Box.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        else if (collision.CompareTag("PlayerAttackRange"))
        {
            if (m_Box != null) m_Box.gravityScale = 3f;

            m_Box.constraints = RigidbodyConstraints2D.FreezePositionX 
                | RigidbodyConstraints2D.FreezeRotation;

            Destroy(gameObject);
        }
    }
}

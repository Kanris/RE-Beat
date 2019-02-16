using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [SerializeField, Range(10f, 200f)] private float JumpHeight = 60f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (Mathf.Abs(collision.contacts[0].normal.x) < 0.3f)
            {
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, JumpHeight);
            }
        }
    }
}

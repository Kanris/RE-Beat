using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEnemyUI : MonoBehaviour {

    [SerializeField] private Rigidbody2D m_Rigidbody2D;

    // Update is called once per frame
    void Update () {
        if (m_Rigidbody2D != null)
            transform.position = new Vector3(m_Rigidbody2D.position.x, transform.position.y);
	}
}

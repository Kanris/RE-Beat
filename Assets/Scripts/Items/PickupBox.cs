﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PickupBox : MonoBehaviour {

    private GameObject m_InteractionButton;
    private Vector3 m_SpawnPosition;
    private Quaternion m_SpawnRotation;
    private Transform m_Player;
    
    private bool m_IsBoxUp;

    public static bool isQuitting = false;

    public float YRestrictions = -10f;

    [SerializeField] private BoxCollider2D m_BoxCollider;

    private float updateTime = 4f;

	// Use this for initialization
	void Start () {

        InitializeInteractionButton();

        ActiveInteractionButton(false);

        m_SpawnPosition = transform.position;
        m_SpawnRotation = transform.rotation;

        isQuitting = false;
    }

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);
    }
    
    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
         if (!isQuitting)
         {
             var newBox = Resources.Load("Items/Box") as GameObject;
             Instantiate(newBox, m_SpawnPosition, m_SpawnRotation);
         }
     }

    // Update is called once per frame
    void Update () {
		
        if (m_Player != null)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                Transform parrentTransform = null;

                if (!m_IsBoxUp)
                {
                    if (m_Player != null)
                    {
                        parrentTransform = m_Player;
                    }
                }

                AttachToParent(parrentTransform);
            }
        }
        else if (m_IsBoxUp)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                AttachToParent(null);
            }
        }

        if (transform.position.y < YRestrictions)
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player == null)
        {
            ActiveInteractionButton(true);
            m_Player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player != null)
        {
            ActiveInteractionButton(false);
            m_Player = null;
        }
    }

    private void ActiveInteractionButton(bool isActive)
    {
        if (m_InteractionButton != null)
        {
            m_InteractionButton.SetActive(isActive);
        }
    }

    private void AttachToParent(Transform parrent)
    {
        transform.SetParent(parrent);

        if (parrent != null)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            ChangeBoxProperty(true, 15);   
        }
        else
        {
            ChangeBoxProperty(false, 0);
        }
    }

    private void ChangeBoxProperty(bool isActive, int layerId)
    {
        m_IsBoxUp = isActive;
        m_BoxCollider.enabled = !isActive;

        if (isActive)
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        else
        {
            GameMaster.Instance.SaveState(gameObject.name, transform.position, GameMaster.RecreateType.Position);
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        transform.gameObject.layer = layerId;
        ActiveInteractionButton(!isActive);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBox : MonoBehaviour {

    private GameObject m_InteractionButton;
    private bool m_IsPlayerNear;
    private bool m_IsBoxUp;
    private Transform m_Player;

	// Use this for initialization
	void Start () {

        InitializeInteractionButton();

        ActiveInteractionButton(false);
    }

    private void InitializeInteractionButton()
    {
        var interactionButtonTransform = transform.GetChild(0);

        if (interactionButtonTransform != null)
        {
            m_InteractionButton = interactionButtonTransform.gameObject;
        }
        else
        {
            Debug.LogError("PickupBox.InitializeInteractionButton: There is no child gameobject");
        }
    }
	
	// Update is called once per frame
	void Update () {
		
        if (m_IsPlayerNear)
        {
            if (Input.GetKeyDown( KeyCode.E ))
            {
                Transform parrentTransform = null;
                if (!m_IsBoxUp)
                {
                    if (m_Player != null)
                    {
                        parrentTransform = m_Player;
                        m_IsBoxUp = true;
                    }
                    else
                        Debug.LogError("PickupBox.Update: player transform is null");
                }

                AttachToParent(parrentTransform);
            }
        }
        else if (m_IsBoxUp)
        {
            if (Input.GetKeyDown( KeyCode.E ))
            {
                m_IsBoxUp = false;
                AttachToParent(null);
            }
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            m_IsPlayerNear = true;
            ActiveInteractionButton(true);
            m_Player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsPlayerNear)
        {
            m_IsPlayerNear = false;
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
            transform.localPosition = new Vector3(0.5f, 0.6f);
            transform.gameObject.layer = 15;
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            transform.gameObject.layer = 0;
        }
    }
}

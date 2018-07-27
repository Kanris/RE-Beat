using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitch : MonoBehaviour {

    [SerializeField] private GameObject DoorToOpen;

    private GameObject m_InteractionButton;
    private bool m_IsPlayerNearSwitch = false;
    private Animator m_Animator;

    // Use this for initialization
    void Start () {

        InitializeAnimator();

        InitializeInteractionButton();

        ShowInteractionKey(false);

    }

    private void InitializeInteractionButton()
    {
        var childTransform = transform.GetChild(0);

        if (childTransform == null)
        {
            Debug.LogError("PickUpItem.InitializeInteractionButton: Can't find SpriteRenderer in child");
        }
        else
            m_InteractionButton = childTransform.gameObject;

    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("DoorSwitch.InitializeAnimator: Can't find Animator component on gameobject");
        }
    }

    // Update is called once per frame
    void Update () {
		
        if (m_IsPlayerNearSwitch)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                m_IsPlayerNearSwitch = false;
                ShowInteractionKey(false);

                StartCoroutine(OpenTheDoor());
            }
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNearSwitch)
        {
            m_IsPlayerNearSwitch = true;
            ShowInteractionKey(m_IsPlayerNearSwitch);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsPlayerNearSwitch)
        {
            m_IsPlayerNearSwitch = false;
            ShowInteractionKey(m_IsPlayerNearSwitch);
        }
    }

    private void ShowInteractionKey(bool show)
    {
        if (m_InteractionButton != null)
            m_InteractionButton.gameObject.SetActive(show);
        else
            Debug.LogError("Door.ShowInteractionKey: InteractionButtonImage is not initialized");
    }

    private IEnumerator OpenTheDoor()
    {
        if (DoorToOpen != null)
        {
            ShowInteractionKey(false);

            m_Animator.SetTrigger("Triggering");

            DoorToOpen.GetComponent<Door>().PlayOpenDoorAnimation();

            yield return new WaitForSeconds(1f);

            Destroy(DoorToOpen);

            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("DoorSwitch.OpenTheDoor: Door to open is not assigned.");
        }

    }
}

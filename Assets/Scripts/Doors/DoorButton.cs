using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : MonoBehaviour {

    [SerializeField] private GameObject DoorToOpen;

    private Animator m_Animator;
    public bool m_IsDoorOpen;
    public bool m_IsPlayerNear;
    public bool m_IsItemNear;

    private void Start()
    {
        InitializeAnimator();
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("DoorButton.InitializeAnimator: Can't fint Animator component on Gameobject");
        }
    }

    private void Update()
    {
        if (m_IsPlayerNear)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                m_IsDoorOpen = !m_IsDoorOpen;

                if (m_IsDoorOpen & m_IsItemNear)
                {
                    TriggerAnimation("Pressed");
                    StartCoroutine(OpenDoor(m_IsDoorOpen));
                }
                else
                {
                    TriggerAnimation("Unpressed");
                    StartCoroutine(OpenDoor(m_IsDoorOpen));
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
            m_IsItemNear = true;

        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
            m_IsItemNear = false;

        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = false;
        }
    }

    private IEnumerator OpenDoor(bool open)
    {
        if (DoorToOpen != null)
        {
            if (open)
            {
                DoorToOpen.GetComponent<Door>().PlayOpenDoorAnimation();

                yield return new WaitForSeconds(0.5f);
            }

            DoorToOpen.SetActive(!open);
        }
        else
        {
            Debug.LogError("DoorButton.OpenDoor: Door to open is not assigned!");
        }
    }

    private void TriggerAnimation(string animation)
    {
        if (m_Animator != null)
        {
            m_Animator.SetTrigger(animation);
        }
    }
}

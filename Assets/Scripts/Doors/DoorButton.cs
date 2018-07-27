using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : MonoBehaviour {

    [SerializeField] private GameObject DoorToOpen;

    private Animator m_Animator;
    private bool m_IsDoorOpen;

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

    private void OnTriggerEnter2D(Collider2D collision)
    { 
        if (collision.CompareTag("Item") & !m_IsDoorOpen)
        {
            TriggerAnimation("Pressed");
            m_IsDoorOpen = true;
            StartCoroutine(OpenDoor(m_IsDoorOpen));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item") & m_IsDoorOpen)
        {
            m_IsDoorOpen = false;
            TriggerAnimation("Unpressed");
            StartCoroutine( OpenDoor(m_IsDoorOpen) );
        }
    }

    private IEnumerator OpenDoor(bool open)
    {
        if (DoorToOpen != null)
        {
            if (open)
            {
                DoorToOpen.GetComponent<Door>().PlayOpenDoorAnimation();

                yield return new WaitForSeconds(2f);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : MonoBehaviour {

    [SerializeField] private GameObject DoorToOpen;

    private Animator m_Animator;
    public bool m_IsDoorOpen;

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
            OpenDoor(m_IsDoorOpen);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item") & m_IsDoorOpen)
        {
            m_IsDoorOpen = false;
            TriggerAnimation("Unpressed");
            OpenDoor(m_IsDoorOpen);
        }
    }

    private void OpenDoor(bool open)
    {
        DoorToOpen.SetActive(!open);
    }

    private void TriggerAnimation(string animation)
    {
        if (m_Animator != null)
        {
            m_Animator.SetTrigger(animation);
        }
    }
}

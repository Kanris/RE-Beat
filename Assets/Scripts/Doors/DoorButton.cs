using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : MonoBehaviour {

    [SerializeField] private GameObject DoorToOpen;

    private Animator m_Animator;

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

    private IEnumerator OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Item"))
            yield return OpenDoor(true);
    }

    private IEnumerator OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Item"))
            yield return OpenDoor(false);
    }

    private IEnumerator OpenDoor(bool open)
    {
        if (DoorToOpen != null)
        {
            if (open)
            {
                DoorToOpen.GetComponent<Door>().PlayOpenDoorAnimation();

                TriggerAnimation("Pressed");

                yield return new WaitForSeconds(0.5f);
            }
            else
                TriggerAnimation("Unpressed");

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

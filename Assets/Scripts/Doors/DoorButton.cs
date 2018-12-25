using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorButton : MonoBehaviour {

    #region private fields

    [SerializeField] private Door DoorToOpen; //door to open when button is pressed
    private Animator m_Animator; //button animator

    #endregion

    #region private methods

    private void Start()
    {
        m_Animator = GetComponent<Animator>(); //get button animator
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Item")) //if item is on the button
            OpenDoor(true); //open attached door
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Item")) //if item is no longer on the button
            OpenDoor(false); //close attached door
    }

    private void OpenDoor(bool open)
    {
        if (DoorToOpen != null) //if attached door is not equal to null
        {
            if (open) //if need to open the door
            {
                DoorToOpen.PlayOpenDoorAnimation(); //play open door animation

                m_Animator.SetTrigger("Pressed"); //set button to pressed animation 

                GameMaster.Instance.StartJoystickVibrate(5, .1f);
            }
            else //if need to close the door
            {
                GameMaster.Instance.StartJoystickVibrate(5, .1f);
                m_Animator.SetTrigger("Unpressed"); //set button to unpressed animation
            }

            DoorToOpen.gameObject.SetActive(!open); //active or disable attached door
        }
        else
        {
            Debug.LogError("DoorButton.OpenDoor: Door to open is not assigned!");
        }
    }

    #endregion
}

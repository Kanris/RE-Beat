using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorButton : MonoBehaviour {

    #region private fields

    [SerializeField] private Door DoorToOpen; //door to open when button is pressed

    private Animator m_Animator; //button animator

    #endregion

    private void Start()
    {
        m_Animator = GetComponent<Animator>(); //get button animator
    }

    #region box detection

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionHandler(collision, true);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionHandler(collision, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Item")) //if item is on the button
        {
            OpenDoor(false); //open attached door
        }
    }

    private void CollisionHandler(Collision2D collision, bool value)
    {
        if (collision.transform.CompareTag("Item") && DoorToOpen.gameObject.activeSelf) //if item is on the button
        {
            if ( Mathf.Abs(collision.contacts[0].normal.x) < 0.3f )
                OpenDoor(value); //open attached door
        }
    }

    #endregion

    private void OpenDoor(bool value)
    {
        if (DoorToOpen != null) //if attached door is not equal to null
        {
            if (value) //if need to open the door
            {
                DoorToOpen.PlayOpenDoorAnimation(); //play open door animation
            }

            m_Animator.SetBool("Pressed", value); //set button to pressed animation 

            InputControlManager.Instance.StartGamepadVibration(5, .1f); //vibrate gamepad

            DoorToOpen.gameObject.SetActive(!value); //active or disable attached door
        }
        else
        {
            Debug.LogError("DoorButton.OpenDoor: Door to open is not assigned!");
        }
    }
}

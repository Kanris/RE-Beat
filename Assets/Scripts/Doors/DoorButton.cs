using UnityEngine;
using System.Collections;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Animator))]
public class DoorButton : MonoBehaviour {

    #region private fields

    [SerializeField] private Door DoorToOpen; //door to open when button is pressed

    [Header("Camera")]
    [SerializeField] Transform m_DoorToShow; //door that camera will show
    [SerializeField, Range(1f, 6f)] private float m_ShowDuration = 2f; //how much time opened door will be shown

    private Animator m_Animator; //button animator

    private Camera2DFollow m_Camera; //main camera
    private bool m_IsShowOncamera = true; //indicates is player saw opened door


    #endregion

    private void Start()
    {
        m_Camera = Camera.main.GetComponent<Camera2DFollow>(); //get main camera on scene
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
        if (collision.transform.CompareTag("Item") && DoorToOpen != null) //if item is on the button
        {
            if (DoorToOpen.gameObject.activeSelf && Mathf.Abs(collision.contacts[0].normal.x) < 0.3f )
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

            StartCoroutine(ShowOpenedDoor()); //show opened door with camera

            m_Animator.SetBool("Pressed", value); //set button to pressed animation 

            InputControlManager.Instance.StartGamepadVibration(5, .1f); //vibrate gamepad

            DoorToOpen.gameObject.SetActive(!value); //active or disable attached door
        }
        else
        {
            Debug.LogError("DoorButton.OpenDoor: Door to open is not assigned!");
        }
    }

    private IEnumerator ShowOpenedDoor()
    {
        if (m_DoorToShow != null && m_IsShowOncamera)
        {
            m_IsShowOncamera = false;
            Time.timeScale = 0f;

            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<PlatformerCharacter2D>().enabled = false; //do not allow player to move
            StartCoroutine( m_Camera.SetTarget(m_DoorToShow, 1f) );
            yield return new WaitForSecondsRealtime(m_ShowDuration);

            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<PlatformerCharacter2D>().enabled = true; //return player's control
            yield return m_Camera.SetTarget(GameMaster.Instance.m_Player.transform.GetChild(0).transform, 1f);
            Time.timeScale = 1f;

            GameMaster.Instance.SaveState(transform.name, 0, GameMaster.RecreateType.Camera);
        }
    }

    private void OnValidate()
    {
        transform.name = transform.parent.name;
    }

    public void SetCameraState(bool value)
    {
        m_IsShowOncamera = value;
    }
}

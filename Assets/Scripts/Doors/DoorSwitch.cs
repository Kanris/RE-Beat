using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Animator))]
public class DoorSwitch : MonoBehaviour {

    #region events

    public delegate void VoidDelegate();
    public event VoidDelegate OnSwitchPressed;

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField] private Door m_DoorToOpen; //door to open when switch is pressed

    [Header("Camera")]
    [SerializeField] private Transform m_ShowWithCam; //when door is open camera will show this position
    [SerializeField, Range(1f, 6f)] private float m_CamShowTime = 2f; //time before return control to player

    [Header("Additional")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton;

    #endregion

    private Camera2DFollow m_Camera; //to show changes
    private bool m_IsOpening; //is door opening
    private bool m_IsQuitting; //if game is closing

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        m_InteractionUIButton.PressInteractionButton = OpenTheDoor;
        m_InteractionUIButton.SetActive(false); //initialize switch ui

        ChangeIsQuitting(false); //notify that application is not closing

        m_Camera = Camera.main.GetComponent<Camera2DFollow>();

        SubscribeToEvents(); //subscribe to events
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //if player is moving to the main screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //if is player moving to another scene
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !m_InteractionUIButton.ActiveSelf() && !m_IsOpening) //if player is near the switch
        {
            m_InteractionUIButton.SetIsPlayerNear(true);
            m_InteractionUIButton.SetActive(true); //show switch ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leaving the switch
        {
            m_InteractionUIButton.SetIsPlayerNear(false);
            m_InteractionUIButton.SetActive(false); //hide switch ui
        }
    }

    //open door when interaction button pressed
    private void OpenTheDoor()
    {
        if (m_InteractionUIButton.ActiveSelf()) //if switch ui is active
        {
            StartCoroutine(PerfromOpenDoor());
        }
    }

    private IEnumerator PerfromOpenDoor()
    {
        m_InteractionUIButton.SetActive(false); //hide switch ui

        if (m_DoorToOpen != null) //if door to open is attached
        {
            m_IsOpening = true;

            GameMaster.Instance.SaveState(gameObject.name, 0, GameMaster.RecreateType.Object); //save switch state
            GameMaster.Instance.SaveState(m_DoorToOpen.name, 0, GameMaster.RecreateType.Object); //save door state
                        
            m_DoorToOpen.gameObject.SetActive(false); //hide door from scene

            yield return ShowChangesWithCam(); //show changes

            Destroy(m_DoorToOpen.gameObject); //destroy door gameobject
            Destroy(gameObject); //destroy switch
        }
        else
        {
            Debug.LogError("DoorSwitch.OpenTheDoor: Door to open is not assigned.");
        }
    }

    private IEnumerator ShowChangesWithCam()
    {
        //show changes to player with camera (if m_ShowWithCam attached to the script)
        if (m_ShowWithCam != null)
        {
            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<PlatformerCharacter2D>().enabled = false; //do not allow player to move
            StartCoroutine(m_Camera.SetTarget(m_ShowWithCam, 1f)); //show door that open
            yield return new WaitForSeconds(m_CamShowTime);

            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<PlatformerCharacter2D>().enabled = true; //return player's control
            yield return m_Camera.SetTarget(GameMaster.Instance.m_Player.transform.GetChild(0), 1f); //show door that open
        }
    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true); //if application is closing
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting) //if application is not closing
        {
            OnSwitchPressed(); //notify event
        }
    }

    //change quit state
    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    #endregion
}

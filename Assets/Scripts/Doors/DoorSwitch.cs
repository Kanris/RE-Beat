using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorSwitch : MonoBehaviour {

    #region events

    public delegate void VoidDelegate();
    public event VoidDelegate OnSwitchPressed;

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField] private Door DoorToOpen; //door to open when switch is pressed

    [Header("Additional")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton;

    #endregion

    private Animator m_Animator; //switch animator
    private bool m_IsQuitting; //if game is closing

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        m_Animator = GetComponent<Animator>(); //get animator component

        m_InteractionUIButton.PressInteractionButton = OpenTheDoor;
        m_InteractionUIButton.SetActive(false); //initialize switch ui

        ChangeIsQuitting(false); //notify that application is not closing

        SubscribeToEvents(); //subscribe to events
    }

    #region Initialize

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //if player is moving to the main screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //if is player moving to another scene
    }


    #endregion

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !m_InteractionUIButton.ActiveSelf()) //if player is near the switch
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

    private void OpenTheDoor()
    {
        if (m_InteractionUIButton.ActiveSelf()) //if switch ui is active
        {
            m_InteractionUIButton.SetActive(false); //hide switch ui

            if (DoorToOpen != null) //if door to open is attached
            {
                m_Animator.SetTrigger("Triggering"); //play switch trigerring animation

                DoorToOpen.PlayOpenDoorAnimation(); //open the door

                GameMaster.Instance.SaveState(gameObject.name, 0, GameMaster.RecreateType.Object); //save switch state
                GameMaster.Instance.SaveState(DoorToOpen.name, 0, GameMaster.RecreateType.Object); //save door state

                Destroy(DoorToOpen.gameObject); //destroy door gameobject
                Destroy(gameObject); //destroy switch
            }
            else
            {
                Debug.LogError("DoorSwitch.OpenTheDoor: Door to open is not assigned.");
            }
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

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    #endregion
}

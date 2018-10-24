using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
public class DoorSwitch : MonoBehaviour {

    #region events

    public delegate void VoidDelegate();
    public event VoidDelegate OnSwitchPressed;

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField] private Door DoorToOpen; //door to open when switch is pressed

    [Header("Effects")]
    [SerializeField] private GameObject m_InteractionUI;

    #endregion

    private GameObject m_UI; //switch ui
    private Animator m_Animator; //switch animator
    private bool m_IsQuitting; //if game is closing

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        m_Animator = GetComponent<Animator>(); //get animator component

        InitializeInteractionButton(); //initialize switch ui

        ChangeIsQuitting(false); //notify that application is not closing

        SubscribeToEvents(); //subscribe to events
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        m_UI = Instantiate(m_InteractionUI, transform);

        m_UI.SetActive(false); //hide ui
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //if player is moving to the main screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //if is player moving to another scene
    }


    #endregion

    // Update is called once per frame
    private void Update () {
		
        if (m_UI.activeSelf) //if switch ui is active
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player pressed submit button
            {
                m_UI.SetActive(false); //hide switch ui

                StartCoroutine(OpenTheDoor()); //open attached door
            }
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near the switch
        {
            m_UI.SetActive(true); //show switch ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player leaving the switch
        {
            m_UI.SetActive(false); //hide switch ui
        }
    }

    private IEnumerator OpenTheDoor()
    {
        if (DoorToOpen != null) //if door to open is attached
        {
            m_Animator.SetTrigger("Triggering"); //play switch trigerring animation

            DoorToOpen.PlayOpenDoorAnimation(); //open the door

            yield return new WaitForSeconds(1f); //wait 1s

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class DoorSwitch : MonoBehaviour {

    public delegate void VoidDelegate();
    public event VoidDelegate OnSwitchPressed;

    [SerializeField] private GameObject DoorToOpen;

    private GameObject m_InteractionButton;
    private bool m_IsPlayerNearSwitch = false;
    private bool m_IsQuitting;
    private Animator m_Animator;

    // Use this for initialization
    void Start () {

        InitializeAnimator();

        InitializeInteractionButton();

        ShowInteractionKey(false);

        ChangeIsQuitting(false);

        SubscribeToEvents();

    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);
    }


    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("DoorSwitch.InitializeAnimator: Can't find Animator component on gameobject");
        }
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }


    #endregion

    // Update is called once per frame
    void Update () {
		
        if (m_IsPlayerNearSwitch)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                m_IsPlayerNearSwitch = false;
                ShowInteractionKey(false);

                StartCoroutine(OpenTheDoor());
            }
        }

	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNearSwitch)
        {
            m_IsPlayerNearSwitch = true;
            ShowInteractionKey(m_IsPlayerNearSwitch);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_IsPlayerNearSwitch)
        {
            m_IsPlayerNearSwitch = false;
            ShowInteractionKey(m_IsPlayerNearSwitch);
        }
    }

    private void ShowInteractionKey(bool show)
    {
        if (m_InteractionButton != null)
            m_InteractionButton.gameObject.SetActive(show);
        else
            Debug.LogError("Door.ShowInteractionKey: InteractionButtonImage is not initialized");
    }


    private IEnumerator OpenTheDoor()
    {
        if (DoorToOpen != null)
        {
            ShowInteractionKey(false);

            m_Animator.SetTrigger("Triggering");

            DoorToOpen.GetComponent<Door>().PlayOpenDoorAnimation();

            yield return new WaitForSeconds(1f);

            if (GameMaster.Instance != null)
            {
                GameMaster.Instance.SaveState(gameObject.name, 0, GameMaster.RecreateType.Object);
                GameMaster.Instance.SaveState(DoorToOpen.name, 0, GameMaster.RecreateType.Object);
            }

            Destroy(DoorToOpen);

            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("DoorSwitch.OpenTheDoor: Door to open is not assigned.");
        }

    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting)
        {
            OnSwitchPressed();
        }
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }
}

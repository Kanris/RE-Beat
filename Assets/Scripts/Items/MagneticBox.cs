using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class MagneticBox : MonoBehaviour {

    public string NeededItem = "Magnetic Arm";

    private bool m_IsBoxPickedUp;
    private bool m_IsQuitting;
    private Animator m_Animator;
    private Transform m_Player;
    private Vector2 m_RespawnPosition;
    private Quaternion m_RespawnRotation;
    private GameObject m_InteractionButton;

	// Use this for initialization
	void Start () {

        InitializeInteractionButton();

        InitializeRespawnValues();

        InitializeAnimator();

        ChangeIsQuitting(false);

        ActiveInteractionButton(false);

        SubscribeToEvents();
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);
    }

    private void InitializeRespawnValues()
    {
        m_RespawnPosition = transform.position;
        m_RespawnRotation = transform.rotation;
    }

    private void InitializeAnimator()
    {
        m_Animator = GetComponent<Animator>();

        if (m_Animator == null)
        {
            Debug.LogError("MagneticBox.InitializeAnimator: can't find animator on gameObject");
        }
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        if (MoveToNextScene.Instance != null)
            MoveToNextScene.Instance.IsMoveToNextScene += ChangeIsQuitting;
    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting)
        {
            var objectToRespawn = Resources.Load("Items/MagneticBox");
            Instantiate(objectToRespawn, m_RespawnPosition, m_RespawnRotation);
        }
    }
    #endregion

    private void Update()
    {
        if (m_Player != null)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                if (IsPlayerHaveItem())
                {
                    PickUpBox(true);
                }
                else
                {
                    AnnouncerManager.Instance.DisplayAnnouncerMessage(
                        new AnnouncerManager.Message(NeededItem + " - required to pickup this box."));
                }
            }
        }
        else if (m_IsBoxPickedUp)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                PickUpBox(false);
            }
        }
    }

    private void PickUpBox(bool value)
    {
        m_IsBoxPickedUp = value;

        if (value)
        {
            transform.SetParent(m_Player);
            transform.localPosition = new Vector2(0.5f, 0.5f);
            transform.gameObject.layer = 15;
            SetAnimation("Active");
        }
        else
        {
            transform.SetParent(null);
            transform.gameObject.layer = 0;
            SetAnimation("Inactive");
            GameMaster.Instance.SaveState(transform.name, transform.position, GameMaster.RecreateType.Position);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player == null)
        {
            m_Player = collision.transform;
            ActiveInteractionButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player != null)
        {
            m_Player = null;
            ActiveInteractionButton(false);
        }
    }

    private bool IsPlayerHaveItem()
    {
        return PlayerStats.PlayerInventory.IsInBag(NeededItem);
    }

    private void ActiveInteractionButton(bool isActive)
    {
        if (m_InteractionButton != null)
        {
            m_InteractionButton.SetActive(isActive);
        }
    }

    private void SetAnimation(string name)
    {
        m_Animator.SetTrigger(name);
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }
}

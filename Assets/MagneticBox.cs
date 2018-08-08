using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class MagneticBox : MonoBehaviour {

    public static bool isQuitting = false;

    public string NeededItem = "Magnetic Arm";

    private bool m_IsBoxPickedUp;
    private Transform m_Player;
    private GameObject m_InteractionButton;
    private Vector2 m_RespawnPosition;
    private Quaternion m_RespawnRotation;

	// Use this for initialization
	void Start () {

        InitializeInteractionButton();

        InitializeRespawnValues();

        SetActiveInteractionButton(false);

        isQuitting = false;
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI");
        m_InteractionButton = Instantiate(interactionButton, transform) as GameObject;
    }

    private void InitializeRespawnValues()
    {
        m_RespawnPosition = transform.position;
        m_RespawnRotation = transform.rotation;
    }

    #endregion

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (!isQuitting)
        {
            var objectToRespawn = Resources.Load("Items/MagneticBox");
            Instantiate(objectToRespawn, m_RespawnPosition, m_RespawnRotation);
        }
    }

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
        if (value)
        {
            transform.SetParent(m_Player);
            transform.localPosition = new Vector2(0.5f, 0.5f);
            transform.gameObject.layer = 15;
        }
        else
        {
            transform.SetParent(null);
            transform.gameObject.layer = 0;
        }

        m_IsBoxPickedUp = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player == null)
        {
            ChangeProperties(true, collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player != null)
        {
            ChangeProperties(false, collision);
        }
    }

    private void ChangeProperties(bool value, Collider2D collision)
    {
        SetActiveInteractionButton(value);

        if (value)
            m_Player = collision.transform;
        else
            m_Player = null;
    }

    private bool IsPlayerHaveItem()
    {
        return PlayerStats.PlayerInventory.IsInBag(NeededItem);
    }

    private void SetActiveInteractionButton(bool value)
    {
        m_InteractionButton.SetActive(value);
    }
}

﻿using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
public class MagneticBox : MonoBehaviour {

    #region public fields

    [Header("Item")]
    public Item NeededItem; //required item to pickup item

    #endregion

    #region private fields

    #region serialize fields

    [Header("Effects")]
    [SerializeField] private GameObject DeathParticles; //destroying particles
    [SerializeField] private Audio DestroySound; //destroying sound

    #endregion

    private bool m_IsBoxPickedUp; //is box in player hands
    private bool m_IsQuitting; //is application closing
    private Animator m_Animator; //box animator
    private Transform m_Player; //player reference
    private Vector2 m_RespawnPosition; //box respawn point
    private GameObject m_InteractionButton; //box ui

    #endregion

    #region private fields

    #region Initialize
    // Use this for initialization
    private void Start () {

        InitializeInteractionButton(); //initialize box ui

        m_RespawnPosition = transform.position; //initialize respawn point

        m_Animator = GetComponent<Animator>(); //box animator

        ChangeIsQuitting(false); //game is not closing

        m_InteractionButton.SetActive(false); //hide box ui

        SubscribeToEvents();

        GameMaster.Instance.SaveState(transform.name, new ObjectPosition(transform.position), GameMaster.RecreateType.Position); //save box position
    }

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //is player return to the start screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //is player move to the next scene

    }
    
    #endregion

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true); //application is closing
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting) //if application is not closing
        {
            ShowDeathParticles(); //show destroying particles
            PlayDestroySound(); //play destroying sound

            //respawn new box
            var objectToRespawn = Resources.Load("Items/MagneticBox") as GameObject;
            var instantiatedBox = Instantiate(objectToRespawn);
            instantiatedBox.transform.position = m_RespawnPosition;
        }
    }

    private void ShowDeathParticles()
    {
        if (DeathParticles != null) //if there is death particles
        {
            if (transform != null) //if box still reachable
            {
                //show deathParticles
                var deathParticles = GameMaster.Instantiate(DeathParticles, transform.position, transform.rotation);
                GameMaster.Destroy(deathParticles, 1f);
            }
        }
    }

    private void PlayDestroySound()
    {
        if (!string.IsNullOrEmpty(DestroySound))
        {
            AudioManager.Instance.Play(DestroySound);
        }
    }

    private void Update()
    {
        if (m_Player != null) //if player near the box
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player pressed submit button
            {
                if (PlayerStats.PlayerInventory.IsInBag(NeededItem.itemDescription.Name)) //if player have needed item
                {
                    PickUpBox(true); //pick up box
                }
                else //if player haven't needed item
                {
                    AnnouncerManager.Instance.DisplayAnnouncerMessage(
                        new AnnouncerManager.Message(LocalizationManager.Instance.GetItemsLocalizedValue (
                            NeededItem.itemDescription.Name) + " - required to pickup this box.")); //display warning message
                }
            }
        }
        else if (m_IsBoxPickedUp) //if box is picked up
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player pressed submit button
            {
                PickUpBox(false); //put the box
            }
        }
    }

    private void PickUpBox(bool value)
    {
        m_IsBoxPickedUp = value; //change box value

        if (value) //if box is picked up
        {
            transform.SetParent(m_Player); //attach box to the player
            transform.localPosition = new Vector2(0.5f, 0.5f); //put box in fron of the player
            transform.gameObject.layer = 0; //change layer so player animation will play correctly
            m_Animator.SetTrigger("Active"); //play active animation
        }
        else //if need to put box down
        {
            transform.SetParent(null); //detach from parent
            transform.gameObject.layer = 14; //chage layer so player can play ground animation
            m_Animator.SetTrigger("Inactive"); //play inactive animation
            GameMaster.Instance.SaveState(transform.name, new ObjectPosition(transform.position), GameMaster.RecreateType.Position); //save box position
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & m_Player == null) //if player near box
        {
            m_Player = collision.transform; //save player 
            m_InteractionButton.SetActive(true); //show box ui
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & m_Player != null) //if player leave box
        {
            m_Player = null; //remove player reference
            m_InteractionButton.SetActive(false); //hide box ui
        }
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    #endregion
}

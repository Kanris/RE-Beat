using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class MagneticBox : MonoBehaviour {

    #region public fields

    public delegate void VoidDelegate(MagneticBox newBox);
    public event VoidDelegate OnBoxDestroy;

    [Header("Item")]
    public Item NeededItem; //required item to pickup item

    #endregion

    #region private fields

    #region serialize fields

    [Header("Effects")]
    [SerializeField] private GameObject DeathParticles; //destroying particles
    [SerializeField] private Audio DestroySound; //destroying sound
    [SerializeField] private GameObject m_InteractionButton; //box ui

    [Header("Additional")]
    [SerializeField] private Transform m_CeilingCheck;
    [SerializeField] private LayerMask m_WhatIsGround;

    #endregion

    private bool m_IsBoxPickedUp; //is box in player hands
    private bool m_IsQuitting; //is application closing

    private bool m_IsCantRelease; //can't release magnetic box

    private Animator m_Animator; //box animator
    private Vector2 m_RespawnPosition; //where respawn box, after this will be destroyed

    #endregion

    #region private methods

    #region Initialize

    // Use this for initialization
    private void Start () {

        m_RespawnPosition = transform.position; //initialize respawn point

        m_Animator = GetComponent<Animator>(); //get current box animator

        ChangeIsQuitting(false); //indicates that game is not closing is not closing

        m_InteractionButton.SetActive(false); //hide box ui

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //is player return to the start screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //is player move to the next scene
    }

    #endregion

    //set is player quit game
    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true); //application is closing
    }

    //create new box if it was destroyed (except when game is closing)
    private void OnDestroy()
    {
        if (!m_IsQuitting) //if application is not closing
        {
            ShowDeathParticles(); //show destroying particles
            PlayDestroySound(); //play destroying sound

            //respawn new box
            var objectToRespawn = Resources.Load("Items/MagneticBox") as GameObject;
            var instantiatedBox = Instantiate(objectToRespawn, m_RespawnPosition, Quaternion.identity);

            instantiatedBox.name = transform.name; //set current object name to the new box

            OnBoxDestroy?.Invoke(instantiatedBox.GetComponent<MagneticBox>()); //add new box to the station script
        }
    }

    //show death particles effect when box is destroyed
    private void ShowDeathParticles()
    {
        if (DeathParticles != null) //if there is death particles
        {
            if (transform != null) //if box still reachable
            {
                GameMaster.Destroy(GameMaster.Instantiate(DeathParticles, transform.position, transform.rotation), 1f);
            }
        }
    }

    //play destroy sound
    private void PlayDestroySound()
    {
        if (!string.IsNullOrEmpty(DestroySound)) //if destroy sound attached to the script
        {
            AudioManager.Instance.Play(DestroySound); //play destroy sound
        }
    }

    private void Update()
    {
        //check is player can release magnetic box right now
        if (m_IsBoxPickedUp)
        {
            //is there ground above box
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, .2f, m_WhatIsGround))
            {
                m_IsCantRelease = true;
            }
            //there is no ground above box
            else
            {
                m_IsCantRelease = false;
            }
        }

        if (m_InteractionButton.activeSelf) //if player near the box
        {
            if (InputControlManager.Instance.m_Joystick.Action4.WasPressed && InputControlManager.IsCanUseSubmitButton()) //if player pressed submit button
            {
                if (PlayerStats.PlayerInventory.IsInBag(NeededItem.itemDescription.Name)) //if player have needed item
                {
                    StartCoroutine( PickUpBox(true) ); //pick up box
                }
                else //if player haven't needed item
                {
                    UIManager.Instance.DisplayNotificationMessage(LocalizationManager.Instance.GetItemsLocalizedValue(
                            NeededItem.itemDescription.Name) + " - required to pickup this box.", 
                            UIManager.Message.MessageType.Message); //display warning message
                }
            }
        }
        else if (m_IsBoxPickedUp && InputControlManager.IsCanUseSubmitButton()) //if box is picked up
        {
            if (!m_IsCantRelease)
            {
                if (InputControlManager.Instance.m_Joystick.Action4.WasPressed || InputControlManager.IsAttackButtonsPressed()) //if player pressed submit button
                {
                    StartCoroutine(PickUpBox(false)); //put the box
                }
            }
        }

        //if box is in player's hand but still showing interaction button
        if (m_IsBoxPickedUp && m_InteractionButton.activeSelf)
        {
            m_InteractionButton.SetActive(false); //hide interacion button
        }
    }

    private IEnumerator PickUpBox(bool value)
    {
        m_IsBoxPickedUp = value; //change box value

        //place box above player
        if (!value)
            transform.localPosition = new Vector2(0, .6f);

        transform.SetParent(value ? GameMaster.Instance.m_Player.transform.GetChild(0) : null); //attach or detach box to the player

        if (value) //if box is picked up
            transform.localPosition = new Vector2(0f, 0.3f); //put box in above of the player
        else //if need to put box down
            GameMaster.Instance.SaveState(transform.name, new ObjectPosition(transform.position), GameMaster.RecreateType.Position); //save box position

        transform.gameObject.layer = value ? 0 : 12; //chage layer so player can play ground animation
        m_Animator.SetBool("Picked UP", value); //play inactive animation

        yield return new WaitForEndOfFrame();

        if (!GameMaster.Instance.IsPlayerDead)
            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<Player>().TriggerPlayerBussy(value); //trigger player bussy

        InputControlManager.Instance.StartJoystickVibrate(1f, 0.05f); //vibrate gamepad
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") && !m_IsBoxPickedUp) //if player near box
        {
            m_InteractionButton.SetActive(true); //show box ui
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player leave box
        {
            m_InteractionButton.SetActive(false); //hide box ui
        }
    }

    //return box to the default position
    public void ResetPosition()
    {
        ShowDeathParticles(); //show destroying particles
        PlayDestroySound(); //play destroying sound

        transform.position = m_RespawnPosition; //return box to the default position
    }

    #endregion
}

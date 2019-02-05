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

    [Header("Additional")]
    [SerializeField] private InteractionUIButton m_InteractionUIButton;
    [SerializeField] private Transform m_CeilingCheck;
    [SerializeField] private LayerMask m_WhatIsGround;

    #endregion

    private bool m_IsBoxUp; //is box in player hands
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

        m_InteractionUIButton.PressInteractionButton = OnPickUpPress;
        m_InteractionUIButton.SetActive(false); //hide box ui

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
         if (m_IsBoxUp && !m_InteractionUIButton.ActiveSelf()) //if box is picked up
        {
            //if box is in player's hands and interaction button thought it's not
            if (!m_InteractionUIButton.m_IsPlayerNear)
                //set is player near true
                m_InteractionUIButton.SetIsPlayerNear(true);

            if (InputControlManager.Instance.IsAttackButtonsPressed() && InputControlManager.Instance.IsCanUseSubmitButton()) //if player pressed attack button
            {
                OnPickUpPress();
            }
        }

        //if box is in player's hand but still showing interaction button
        if (m_IsBoxUp && m_InteractionUIButton.ActiveSelf())
        {
            m_InteractionUIButton.SetActive(false); //hide interacion button
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_CeilingCheck.position, new Vector3(.45f, .45f));
    }

    private void CheckGroundAbove()
    {
        if (m_IsBoxUp)
        {
            if (!m_InteractionUIButton.m_IsPlayerNear)
                m_InteractionUIButton.SetIsPlayerNear(true);

            //is there ground above box
            if (Physics2D.OverlapBox(m_CeilingCheck.position, new Vector2(.45f, .45f), 0, m_WhatIsGround))
            {
                m_IsCantRelease = true; //player can't release box
                UIManager.Instance.DisplayNotificationMessage("There is no space above box!", UIManager.Message.MessageType.Message, 3f);
            }
            //there is no ground above box
            else
            {
                m_IsCantRelease = false;
            }
        }
        else
        {
            m_IsCantRelease = false;
        }
    }

    private void OnPickUpPress()
    {   
        if (m_InteractionUIButton.ActiveSelf() || m_IsBoxUp)
        {
            if (PlayerStats.PlayerInventory.IsInBag(NeededItem.itemDescription.Name)) //if player have needed item
            {
                CheckGroundAbove(); //check is there is ground above the box (only if box is picked up)

                if (!m_IsCantRelease)
                {
                    StartCoroutine(AttachToParent()); //attach box to the player
                }
            }
            else //if player haven't needed item
            {
                UIManager.Instance.DisplayNotificationMessage(LocalizationManager.Instance.GetItemsLocalizedValue(
                        NeededItem.itemDescription.Name) + " - required to pickup this box.",
                        UIManager.Message.MessageType.Message); //display warning message
            }
        }
    }

    private IEnumerator AttachToParent()
    {
        var value = !m_IsBoxUp;
        
        GetComponent<BoxCollider2D>().enabled = m_IsBoxUp; //disable box collider

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

        m_IsBoxUp = value; //change box value

        yield return new WaitForEndOfFrame();

        if (!GameMaster.Instance.IsPlayerDead)
            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<Player>().TriggerPlayerBussy(value); //trigger player bussy

        InputControlManager.Instance.StartGamepadVibration(1f, 0.05f); //vibrate gamepad
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") && !m_IsBoxUp && !m_InteractionUIButton.ActiveSelf()) //if player near box
        {
            m_InteractionUIButton.SetActive(true); //show box ui
            m_InteractionUIButton.SetIsPlayerNear(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) //if player leave box
        {
            m_InteractionUIButton.SetActive(false); //hide box ui
            m_InteractionUIButton.SetIsPlayerNear(false);
        }
    }

    //return box to the default position
    public void ResetPosition()
    {
        Destroy(gameObject);
    }

    #endregion
}

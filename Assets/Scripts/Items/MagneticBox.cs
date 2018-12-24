using UnityEngine;

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

    #endregion

    private bool m_IsBoxPickedUp; //is box in player hands
    private bool m_IsQuitting; //is application closing
    private Animator m_Animator; //box animator
    private Transform m_Player; //player reference
    private Vector2 m_RespawnPosition; //box respawn point

    private Animator m_PlayerAnimator;
    private float m_PreviousYPosition = 0f;
    private float m_CheckPositionTime;

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    private void Start () {

        m_RespawnPosition = transform.position; //initialize respawn point

        m_Animator = GetComponent<Animator>(); //box animator

        ChangeIsQuitting(false); //game is not closing

        m_InteractionButton.SetActive(false); //hide box ui

        SubscribeToEvents();

        /*GameMaster.Instance.SaveState(transform.name, 
            new ObjectPosition(transform.position), GameMaster.RecreateType.Position); //save box position*/
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

            if (OnBoxDestroy != null)
                OnBoxDestroy(instantiatedBox.GetComponent<MagneticBox>());
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
            if (GameMaster.Instance.m_Joystick.Action4.WasPressed & MouseControlManager.IsCanUseSubmitButton()) //if player pressed submit button
            {
                if (PlayerStats.PlayerInventory.IsInBag(NeededItem.itemDescription.Name)) //if player have needed item
                {
                    PickUpBox(true); //pick up box
                }
                else //if player haven't needed item
                {
                    UIManager.Instance.DisplayNotificationMessage(LocalizationManager.Instance.GetItemsLocalizedValue(
                            NeededItem.itemDescription.Name) + " - required to pickup this box.", 
                            UIManager.Message.MessageType.Message); //display warning message
                }
            }
        }
        else if (m_IsBoxPickedUp & MouseControlManager.IsCanUseSubmitButton()) //if box is picked up
        {
            if (GameMaster.Instance.m_Joystick.Action4.WasPressed) //if player pressed submit button
            {
                PickUpBox(false); //put the box
            }
            else if (!m_PlayerAnimator.GetBool("Ground")) //if player is in air
            {
                if (m_CheckPositionTime < Time.time) //if it's time to check
                {
                    if (m_PreviousYPosition == transform.parent.position.y) //player stuck in jump with box
                    {
                        PickUpBox(false); //release the box
                    }
                    else
                    {
                        m_CheckPositionTime = Time.time + 0.05f; //next check timer
                        m_PreviousYPosition = transform.parent.position.y; //save previous air position
                    }
                }
            }
        }
    }

    private void PickUpBox(bool value)
    {
        m_IsBoxPickedUp = value; //change box value

        if (value) //if box is picked up
        {
            transform.SetParent(m_Player); //attach box to the player
            transform.localPosition = new Vector2(0.5f, 0f); //put box in fron of the player
            transform.gameObject.layer = 0; //change layer so player animation will play correctly
            m_Animator.SetTrigger("Active"); //play active animation

            m_PlayerAnimator = gameObject.transform.parent.GetComponent<Animator>(); //get player animator
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

            if (!m_IsBoxPickedUp) m_InteractionButton.SetActive(true); //show box ui
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

    public void ResetPosition()
    {
        ShowDeathParticles(); //show destroying particles
        PlayDestroySound(); //play destroying sound

        transform.position = m_RespawnPosition;
    }

    #endregion
}

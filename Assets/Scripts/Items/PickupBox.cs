using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PickupBox : MonoBehaviour {

    #region public fields

    [SerializeField, Range(100f, -100f)] private float YRestrictions = -10f; //y fall restrictions

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField] private GameObject DeathParticle; //box destroying particles
    [SerializeField] private Audio DestroySound; //box destroying sound

    #endregion

    private BoxCollider2D m_BoxCollider; //box collider
    private GameObject m_InteractionButton; //box ui
    private Vector3 m_SpawnPosition; //box spawn position
    private Transform m_Player; //player reference
    
    private bool m_IsBoxUp; //is box in player's hand
    private bool m_IsQuitting; //is application closing

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        InitializeInteractionButton(); //initialize box ui

        m_BoxCollider = GetComponent<BoxCollider2D>(); //get box collider

        m_InteractionButton.SetActive(false); //hide box ui

        m_SpawnPosition = transform.position; //initialize respawn position

        ChangeIsQuitting(false); //application is not closing

        SubscribeToEvents();

        GameMaster.Instance.SaveState(gameObject.name, new ObjectPosition(transform.position), GameMaster.RecreateType.Position); //save object state
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //if player is open start screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //is player is move to the next sceen
    }

    #endregion

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true); //application is closing
    }

    private void OnDestroy()
    {
         if (!m_IsQuitting) //is application is not closing
         {
            ShowDeathParticles(); //display destroying particles
            PlayDestroySound(); //play destroy sound

            //respawn new box
            var newBox = Resources.Load("Items/Box") as GameObject;
            var instantiateNewBox = Instantiate(newBox);
            instantiateNewBox.transform.position = m_SpawnPosition;
         }
     }

    private void ShowDeathParticles()
    {
        if (DeathParticle != null) //if death particles has reference
        {
            if (transform != null) //if box is not detroyed
            {
                //spawn particles
                var deathParticles = GameMaster.Instantiate(DeathParticle, transform.position, transform.rotation);
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

    // Update is called once per frame
    void Update () {
		
        if (m_Player != null) //if player is near
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit")) //if player pressed submit button
            {
                var parent = m_Player;

                if (m_IsBoxUp)
                {
                    parent = null;
                }

                AttachToParent(parent); //attach box to the player
            }
        }

        if (!m_IsQuitting) //if application is not closing
        {
            if (transform.position.y < YRestrictions) //if box is out from y restrictions
            {
                Destroy(gameObject); //destroy box to respawn new
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player == null) //if player is near box
        {
            m_InteractionButton.SetActive(true); //show box ui
            m_Player = collision.transform; //get player reference
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player != null) //if player move away from the box
        {
            m_InteractionButton.SetActive(false); //hide box ui
            m_Player = null; //remove player reference
        }
    }

    private void AttachToParent(Transform parrent)
    {
        transform.SetParent(parrent); //attach box to the parrent

        if (parrent != null) //if parent there is parent
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero; //stop box velocity
            transform.localPosition = transform.localPosition.With(x: 0.5f, y: 0.5f);
            ChangeBoxProperty(true, 0);   
        }
        else
        {
            ChangeBoxProperty(false, 14);
        }
    }

    private void ChangeBoxProperty(bool isActive, int layerId)
    {
        m_IsBoxUp = isActive; //is box picked up
        m_BoxCollider.enabled = !isActive; //remove or enable collider on the box

        if (isActive)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            GameMaster.Instance.SaveState(gameObject.name, new ObjectPosition(transform.position), GameMaster.RecreateType.Position);
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        transform.gameObject.layer = layerId; //change layer so player can play walk or jump animation
        m_InteractionButton.SetActive(!isActive); //show or hide box ui
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    #endregion

}

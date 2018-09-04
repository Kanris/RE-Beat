using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PickupBox : MonoBehaviour {

    private GameObject m_InteractionButton;
    private Vector3 m_SpawnPosition;
    private Quaternion m_SpawnRotation;
    private Transform m_Player;
    
    private bool m_IsBoxUp;
    private bool m_IsQuitting = false;

    public float YRestrictions = -10f;

    [SerializeField] private BoxCollider2D m_BoxCollider;
    [SerializeField] private GameObject DeathParticle;
    [SerializeField] private string DestroySound;

    // Use this for initialization
    void Start () {

        InitializeInteractionButton();

        ActiveInteractionButton(false);

        m_SpawnPosition = transform.position;
        m_SpawnRotation = transform.rotation;

        ChangeIsQuitting(false);

        SubscribeToEvents();

        GameMaster.Instance.SaveState(gameObject.name, new ObjectPosition(transform.position), GameMaster.RecreateType.Position);
    }

    #region Initialize

    private void InitializeInteractionButton()
    {
        var interactionButton = Resources.Load("UI/InteractionUI") as GameObject;
        m_InteractionButton = Instantiate(interactionButton, transform);
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }

    #endregion

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
         if (!m_IsQuitting)
         {
            ShowDeathParticles();
            PlayDestroySound();

            var newBox = Resources.Load("Items/Box") as GameObject;
            Instantiate(newBox, m_SpawnPosition, m_SpawnRotation);
         }
     }

    private void ShowDeathParticles()
    {
        if (DeathParticle != null)
        {
            if (transform != null)
            {
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
		
        if (m_Player != null)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                Transform parrentTransform = null;

                if (!m_IsBoxUp)
                {
                    if (m_Player != null)
                    {
                        parrentTransform = m_Player;
                    }
                }

                AttachToParent(parrentTransform);
            }
        }
        else if (m_IsBoxUp)
        {
            if (CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                AttachToParent(null);
            }
        }

        if (!m_IsQuitting)
        {
            if (transform.position.y < YRestrictions)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player == null)
        {
            ActiveInteractionButton(true);
            m_Player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & m_Player != null)
        {
            ActiveInteractionButton(false);
            m_Player = null;
        }
    }

    private void ActiveInteractionButton(bool isActive)
    {
        if (m_InteractionButton != null)
        {
            m_InteractionButton.SetActive(isActive);
        }
    }

    private void AttachToParent(Transform parrent)
    {
        transform.SetParent(parrent);

        if (parrent != null)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            ChangeBoxProperty(true, 0);   
        }
        else
        {
            ChangeBoxProperty(false, 14);
        }
    }

    private void ChangeBoxProperty(bool isActive, int layerId)
    {
        m_IsBoxUp = isActive;
        m_BoxCollider.enabled = !isActive;

        if (isActive)
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        else
        {
            GameMaster.Instance.SaveState(gameObject.name, new ObjectPosition(transform.position), GameMaster.RecreateType.Position);
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        transform.gameObject.layer = layerId;
        ActiveInteractionButton(!isActive);
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }
}

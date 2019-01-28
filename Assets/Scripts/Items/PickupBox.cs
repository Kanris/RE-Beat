﻿using UnityEngine;
using System.Collections;

public class PickupBox : MonoBehaviour {

    #region serialize fields

    [Header("Effects")]
    [SerializeField] private GameObject DeathParticle; //box destroying particles
    [SerializeField] private Audio DestroySound; //box destroying sound
    [SerializeField] private GameObject m_InteractionUI; //interaction button ui on box

    #endregion

    [Header("Destroy conditions")]
    [SerializeField, Range(100f, -100f)] private float YRestrictions = -10f; //y fall restrictions

    [Header("Additional")]
    [SerializeField] private Transform m_CeilingCheck;
    [SerializeField] private LayerMask m_WhatIsGround;

    #region private fields

    private Vector3 m_SpawnPosition; //box spawn position
    private BoxCollider2D m_BoxCollider;

    [SerializeField] private bool m_IsBoxUp; //is box in player's hand
    private bool m_IsQuitting; //is application closing

    private bool m_IsCantRelease; //can't release box

    #endregion

    #region private methods

    #region Initialize
    // Use this for initialization
    private void Start () {

        m_SpawnPosition = transform.position; //initialize respawn position

        m_BoxCollider = GetComponent<BoxCollider2D>(); //get box collider;

        SubscribeToEvents(); 

        SetIsQuitting(false); //application is not closing

        m_InteractionUI.SetActive(false); //hide box ui
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += SetIsQuitting; //if player is open start screen
        MoveToNextScene.IsMoveToNextScene += SetIsQuitting; //is player is move to the next sceen
    }

    #endregion

    private void OnDestroy()
    {
         if (!m_IsQuitting) //is application is not closing
         {
            ShowDeathParticles(); //display destroying particles
            PlayDestroySound(); //play destroy sound

            //respawn new box
            var newBox = Resources.Load("Items/Box") as GameObject;
            Instantiate(newBox, m_SpawnPosition, Quaternion.identity).name = transform.name;
         }
     }

    // Update is called once per frame
    void Update () {

        //check is player can release box right now
        if (m_IsBoxUp)
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

        #region pickup handler

        if (m_InteractionUI.activeSelf && !m_IsBoxUp) //if player is near and box is not in player's hand
        {
            if (InputControlManager.Instance.IsPickupPressed() && InputControlManager.IsCanUseSubmitButton()) //if player pressed submit button
            {
                StartCoroutine( AttachToParent(true) ); //pickup box
            }

        }
        else if (m_IsBoxUp && !m_InteractionUI.activeSelf) //if player is holding box
        {
            if ((InputControlManager.Instance.IsPickupPressed() || InputControlManager.IsAttackButtonsPressed())
                    && InputControlManager.IsCanUseSubmitButton()) //if player pressed submit or attack button
            {
                if (!m_IsCantRelease)
                    StartCoroutine( AttachToParent(false) ); //attach box to the player
                else
                    //show error message
                    UIManager.Instance.DisplayNotificationMessage("There is no space above box!", UIManager.Message.MessageType.Message, 3f);
            }
        }

        //if player is holding box, but interaction button is active
        if (m_IsBoxUp && m_InteractionUI.activeSelf)
        {
            //hide interaction button
            m_InteractionUI.SetActive(false);
        }

        #endregion

        if (!m_IsQuitting) //if application is not closing
        {
            if (transform.position.y < YRestrictions) //if box is out from y restrictions
            {
                Destroy(gameObject); //destroy box to respawn new
            }
        }
    }

    #region ontrigger

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player is near box
        {
            m_InteractionUI.SetActive(true); //show box ui
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //if player move away from the box
        {
            m_InteractionUI.SetActive(false); //hide box ui
        }
    }

    #endregion

    private IEnumerator AttachToParent(bool value)
    { 
        transform.SetParent(value ? GameMaster.Instance.m_Player.transform.GetChild(0) : null); //attach box to the parrent

        if (value) //if parent there is parent
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero; //stop box velocity

            transform.rotation = Quaternion.identity; //return rotation to default values
            transform.localPosition = transform.localPosition.With(x: 0f, y: 0.4f);
        }

        ChangeBoxProperty(value, value ? 0 : 12); //change box propert (change rigidbody, layer etc)

        yield return new WaitForEndOfFrame(); //wait before give player control

        if (!GameMaster.Instance.IsPlayerDead)
            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<Player>().TriggerPlayerBussy(value); //trigger player bussy

        InputControlManager.Instance.StartGamepadVibration(1f, 0.05f);
    }

    private void ChangeBoxProperty(bool value, int layerId)
    {
        m_IsBoxUp = value; //is box picked up
        GetComponent<BoxCollider2D>().enabled = !value;

        if (m_IsBoxUp)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

            GetComponent<Rigidbody2D>().AddForce(new Vector2(100f * GameMaster.Instance.m_Player.transform.GetChild(0).localScale.x, 200f));
            //save current box position
            GameMaster.Instance.SaveState(gameObject.name, new ObjectPosition(transform.position), GameMaster.RecreateType.Position);
        }


        transform.gameObject.layer = layerId; //change layer so player can play walk or jump animation
        m_InteractionUI.SetActive(!m_IsBoxUp); //show or hide box ui
    }

    private float? GetForceScale()
    {
        return GameMaster.Instance.m_Player?.transform.GetChild(0).localScale.x;
    }

    //change quitting value
    private void SetIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    private void OnApplicationQuit()
    {
        SetIsQuitting(true); //application is closing
    }

    #region effects

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

    #endregion

    #endregion

}

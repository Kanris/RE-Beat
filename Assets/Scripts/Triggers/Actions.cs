using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Actions : MonoBehaviour {

    public enum PlayerAction { OnSubmit, OnDestroy }
    public PlayerAction playerAction;

    public enum ActionType { Destroy, Show }
    public ActionType actionType;

    public GameObject ActionObject;

    private bool m_IsQuitting;
    private bool m_IsPlayerNear;


    #region Initialize
    private void Start()
    {
        ChangeIsQuitting(false);

        SubscribeToEvents();
    }



    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }

    #endregion

    private void Update()
    {
        if (playerAction == PlayerAction.OnSubmit)
        {
            if (m_IsPlayerNear)
            {
                if (CrossPlatformInputManager.GetButtonDown("Submit"))
                {
                    ChangeObjectState();
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting & playerAction == PlayerAction.OnDestroy)
        {
            ChangeObjectState();
        }
    }

    #region OnTrigger

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            m_IsPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            m_IsPlayerNear = false;
        }
    }

    #endregion

    private void ChangeObjectState()
    {
        if (ActionObject != null)
        {
            if (actionType == ActionType.Destroy)
            {
                GameMaster.Instance.SaveState(ActionObject.name, 0, GameMaster.RecreateType.Object);
                Destroy(ActionObject);
            }

            else if (actionType == ActionType.Show)
                ActionObject.SetActive(true);
        }
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }
}

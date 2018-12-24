using UnityEngine;

public class Actions : MonoBehaviour {

    #region public fields

    public enum PlayerAction { OnSubmit, OnDestroy }
    public PlayerAction playerAction;

    public enum ActionType { Destroy, Show }
    public ActionType actionType;

    public GameObject ActionObject;

    #endregion

    #region private fields

    private bool m_IsQuitting; //is application closing
    private bool m_IsPlayerNear; //is player near

    #endregion

    #region Initialize
    private void Start()
    {
        ChangeIsQuitting(false);

        SubscribeToEvents();

        if (actionType == ActionType.Show) ActionObject.SetActive(false);
    }



    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }

    #endregion

    #region private methods

    private void Update()
    {
        //trigger on submit
        if (playerAction == PlayerAction.OnSubmit) 
        {
            if (m_IsPlayerNear)
            {
                if (GameMaster.Instance.m_Joystick.Action4.WasPressed)
                {
                    ChangeObjectState();
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        //application is closing
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting & playerAction == PlayerAction.OnDestroy) //on destroy
        {
            ChangeObjectState();
        }
    }

    #region OnTrigger

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_IsPlayerNear = false;
        }
    }

    #endregion

    private void ChangeObjectState()
    {
        if (ActionObject != null)
        {
            //destroy
            if (actionType == ActionType.Destroy) 
            {
                GameMaster.Instance.SaveState(ActionObject.name, 0, GameMaster.RecreateType.Object);
                Destroy(ActionObject);
            }
            //show
            else if (actionType == ActionType.Show) 
                ActionObject.SetActive(true);
        }
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    #endregion
}

using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class TaskManager : MonoBehaviour {

    #region enum

    public enum TaskTriggerType { OnTrigger, OnCollision, OnSubmit, OnDestroy }
    [Header("React condition")]
    [SerializeField] private TaskTriggerType taskTriggerType; //task trigger type

    public enum TaskType { Giver, Updater, Finisher }
    [Header("Task update type")]
    [SerializeField] private TaskType taskType; //task type

    #endregion

    #region private fields

    #region serialize fields

    [Header("Task description")]
    [SerializeField] private string Name; //task name
    [SerializeField] private string key; //task description

    [Header("Destroy conditions")]
    [SerializeField] private bool DestroyEntireObject; //if need to destroy entier object

    #endregion

    private bool m_IsPlayerNear; //is player is near
    private bool m_IsQuitting; //is application is closing

    #endregion

    #region private methods

    #region Initialize

    private void Start()
    {
        ChangeIsQuitting(false); //aplication is not closing

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //player moving to the start screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //player moving to the next scene
    }

    #endregion

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true); //aplication is closing
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting & taskTriggerType == TaskTriggerType.OnDestroy) //if application is not closing and task manager type is destroy
        {
            ChangeTaskStatus(); //change task status
        }
    }

    private void Update()
    {
        if (m_IsPlayerNear) //if player is near
        {
            if (taskTriggerType == TaskTriggerType.OnSubmit) //if task type - on submit
            {
                if ( CrossPlatformInputManager.GetButtonDown("Submit") ) //if player press submit button
                {
                    ChangeTaskStatus();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear) //if player in triger
        { 
            m_IsPlayerNear = true; //player in trigger
            if (taskTriggerType == TaskTriggerType.OnTrigger) //if task have to update status on trigger
            {
                ChangeTaskStatus();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsPlayerNear) //if player in collision
        {
            m_IsPlayerNear = true; //player in collision
            if (taskTriggerType == TaskTriggerType.OnCollision) //if task have to update status on collision
            {
                ChangeTaskStatus();
            }
        }
    }

    private void ChangeTaskStatus()
    {
        if (CheckTask()) //if there is task name
        {
            var isSuccess = false; //is task update success
            
            switch (taskType)
            {
                case TaskType.Giver: //give task to player
                    isSuccess = InfoManager.Instance.AddTask(Name, Name, key);
                    break;

                case TaskType.Updater: //update task
                    isSuccess = InfoManager.Instance.UpdateTask(Name, key);
                    break;

                case TaskType.Finisher: //finish task
                    isSuccess = InfoManager.Instance.CompleteTask(Name, key);
                    break;
            }

            if (isSuccess) //if task give/update/finish was success
            {
                AudioManager.Instance.Play("Task"); //play task update sound
                GameMaster.Instance.SaveState(name, 0, GameMaster.RecreateType.Task); //save task state
                DestroyObject(); //destroy if needed
            }
        }
    }
    
    private bool CheckTask()
    {
        if (string.IsNullOrEmpty(Name))
        {
            Debug.LogError("TaskGiver.CheckTask: Name - empty");
            return false;
        }

        return true;
    }

    public void DestroyObject()
    {
        if (DestroyEntireObject) //if need to destroy whole gameobject
            Destroy(gameObject); 

        else //if need to remove this script
            Destroy(this);
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    #endregion

}

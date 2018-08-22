using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class TaskManager : MonoBehaviour {

    public enum TaskTriggerType { OnTrigger, OnCollision, OnSubmit, OnDestroy  }
    public enum TaskType { Giver, Updater, Finisher }

    [SerializeField] private TaskType taskType;
    [SerializeField] private TaskTriggerType taskTriggerType;
    [SerializeField] private bool DestroyEntireObject;
    [SerializeField] private string Name;
    [SerializeField, TextArea(2, 20)] private string TaskText;

    private bool m_IsPlayerNear;
    private bool m_IsQuitting;

    private void Start()
    {
        ChangeIsQuitting(false);

        SubscribeToEvents();
    }

    #region Initialize

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
        if (!m_IsQuitting & taskTriggerType == TaskTriggerType.OnDestroy)
        {
            ChangeTaskStatus();
        }
    }

    private void Update()
    {
        if (m_IsPlayerNear)
        {
            if (taskTriggerType == TaskTriggerType.OnSubmit)
            {
                if ( CrossPlatformInputManager.GetButtonDown("Submit") )
                {
                    ChangeTaskStatus();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            m_IsPlayerNear = true;
            if (taskTriggerType == TaskTriggerType.OnTrigger)
            {
                ChangeTaskStatus();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsPlayerNear)
        {
            m_IsPlayerNear = true;
            if (taskTriggerType == TaskTriggerType.OnCollision)
            {
                ChangeTaskStatus();
            }
        }
    }

    private void ChangeTaskStatus()
    {
        if (CheckTask())
        {
            var isSuccess = false;

            switch (taskType)
            {
                case TaskType.Giver:
                    isSuccess = JournalManager.Instance.AddTask(Name, TaskText);
                    break;

                case TaskType.Updater:
                    isSuccess = JournalManager.Instance.UpdateTask(Name, TaskText);
                    break;

                case TaskType.Finisher:
                    isSuccess = JournalManager.Instance.CompleteTask(Name, TaskText);
                    break;
            }

            if (isSuccess)
            {
                AudioManager.Instance.Play("Task");
                GameMaster.Instance.SaveState(name, 0, GameMaster.RecreateType.Task);
                DestroyObject();
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
        if (DestroyEntireObject)
            Destroy(gameObject);
        else
            Destroy(this);
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }
}

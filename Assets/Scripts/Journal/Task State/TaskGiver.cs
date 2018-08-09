using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGiver : MonoBehaviour {

    [SerializeField] private string Name;
    [SerializeField, TextArea(2, 20)] private string TaskText;

    private bool m_IsPlayerNear;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            GiveTask();
        }
    }

    private void GiveTask()
    {
        if (CheckTask())
        {
            if (JournalManager.Instance.AddTask(Name, TaskText))
            {
                m_IsPlayerNear = false;
                //TODO: Save task state
                Destroy(gameObject);
            }
        }
    }

    private bool CheckTask()
    {
        if (string.IsNullOrEmpty(Name) | string.IsNullOrEmpty(TaskText))
        {
            Debug.LogError("TaskGiver.CheckTask: Name and/or Task text - empty");
            return false;
        }

        return true;
    }
}

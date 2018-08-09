using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskFinisher : MonoBehaviour {

    [SerializeField] private string Name;

    private bool m_IsPlayerNear;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") & !m_IsPlayerNear)
        {
            TaskComplete();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsPlayerNear)
        {
            TaskComplete();
        }
    }

    public void TaskComplete()
    {
        if (CheckTask())
        {
            if (JournalManager.Instance.CompleteTask(Name))
            {
                m_IsPlayerNear = true;
                Destroy(this);
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

}

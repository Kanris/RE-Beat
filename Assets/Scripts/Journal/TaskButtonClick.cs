using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskButtonClick : MonoBehaviour {

	public void DisplayTaskText()
    {
        JournalManager.Instance.DisplayTaskText(transform.name);
    }
}

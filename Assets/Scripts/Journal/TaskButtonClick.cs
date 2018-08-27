using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskButtonClick : MonoBehaviour {

	public void DisplayTaskText()
    {
        PlayClickSound();
        JournalManager.Instance.DisplayTaskText(transform.name);
    }

    #region Sound

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("UI-Click");
        }
        else
        {
            Debug.LogError("StartScreenManager.PlayClickSound: Audiomanager.Instance is equal to null");
        }
    }

    #endregion
}

using UnityEngine;

public class TaskButtonClick : MonoBehaviour {

    #region public methods

    public void DisplayTaskText() //if task button was pressed
    {
        PlayClickSound(); //play click sound
        InfoManager.Instance.DisplayTaskText(transform.name); //show clicked task description
    }

    #endregion

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

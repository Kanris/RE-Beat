using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseControlManager : MonoBehaviour {

    private GameObject lastselect;

    #region singleton

    public static MouseControlManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    #endregion

    // Update is called once per frame
    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(lastselect);
        }
        else
        {
            lastselect = EventSystem.current.currentSelectedGameObject;
        }
    }

    public static bool IsCanUseSubmitButton()
    {
        if (!PauseMenuManager.IsPauseOpen)
        {
            if (!InfoManager.IsJournalOpen)
            {
                return true;
            }
        }

        return false;
    }

}

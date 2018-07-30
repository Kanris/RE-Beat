using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenManager : MonoBehaviour {

    private void Awake()
    {
        #region Initialize Managers
        Initialize("Managers/EventSystem");

        Initialize("Managers/AudioManager");

        Initialize("Managers/LoadSceneManager");

        Initialize("Managers/ScreenFaderManager");
        #endregion
    }

    void Initialize(string name)
    {
        var gameObjectToInstantiate = Resources.Load(name);
        Instantiate(gameObjectToInstantiate);
    }

    public void LoadScene(string name)
    {
        LoadSceneManager.Instance.Load(name);
    }

    public void LoadGame()
    {

    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

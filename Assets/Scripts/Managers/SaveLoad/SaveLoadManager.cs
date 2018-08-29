using UnityEngine;

public class SaveLoadManager : MonoBehaviour {

    #region Singleton

    public static SaveLoadManager Instance;

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

    #region private fields

    private GameObject SaveImage;

    #endregion

    #region Initialize

    private void Start()
    {
        InitializeSaveImage();
    }

    private void InitializeSaveImage()
    {

    }

    #endregion

    #region Public methods

    public void LoadGameData()
    {
        SaveLoadMaster.LoadPlayerData();
        SaveLoadMaster.LoadGeneralData();
    }

    public void LoadScene()
    {
        var sceneToLoad = SaveLoadMaster.GetLoadScene();

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("SaveLoadManager.LoadScene: scene name is empty");
        }
    }

    public void SaveGameData()
    {
        SaveLoadMaster.SaveGeneralData();
        SaveLoadMaster.SavePlayerData();
    }

    #endregion

    #region Private methods

    private void LoadScene(string sceneName)
    {
        if (LoadSceneManager.Instance != null)
            LoadSceneManager.Instance.Load(sceneName);
        else
            Debug.LogError("SaveLoadManager.LoadScene: Can't load scene because LoadSceneManager.Instance is null");
    }

    #endregion

}

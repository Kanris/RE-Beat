using System.Collections;
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

    [SerializeField] private GameObject m_SaveImage; //save image

    #endregion

    #region Initialize

    private void Start()
    {
        ActiveSaveImage(false); //hide save fields
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

    private void ActiveSaveImage(bool value)
    {
        m_SaveImage.SetActive(value); //active or hide image
    }

    #endregion

    #region public methods

    public bool IsLoadGameDataAvailable()
    {
        return SaveLoadMaster.IsSaveDataAvailable();
    }

    public void LoadGameData()
    {
        SaveLoadMaster.LoadPlayerData(); //load player data
        SaveLoadMaster.LoadGeneralData(); //load general data
    }

    public void LoadScene()
    {
        var sceneToLoad = SaveLoadMaster.GetSceneToLoad(); //get scene to load

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("SaveLoadManager.LoadScene: scene name is empty");
        }
    }

    public void SaveGameData(Audio saveAudio)
    {
        StartCoroutine(SaveGame(saveAudio)); //save game data
    }

    private IEnumerator SaveGame(Audio saveAudio)
    {
        ActiveSaveImage(true); //show save image
        AudioManager.Instance.Play(saveAudio); //play save sound

        SaveLoadMaster.SaveGeneralData();
        SaveLoadMaster.SavePlayerData();

        yield return new WaitForSeconds(2f);

        ActiveSaveImage(false); //hide save image
    }

    public void SaveOptions()
    {
        SaveLoadMaster.SaveOptionsData(); //save options data (on start screen)
    }

    public bool LoadOptions()
    {
        var instance = SaveLoadMaster.LoadOptionsData(); //load options (for start screen)

        return instance != null;
    }

    #endregion

}

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

    #region private serialize fields

    [SerializeField] private GameObject m_SaveImage;

    #endregion

    #region Initialize

    private void Start()
    {
        ActiveSaveImage(false);
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
        StartCoroutine(SaveGame());
    }

    private IEnumerator SaveGame()
    {
        ActiveSaveImage(true);
        AudioManager.Instance.Play("Respawn Torch Activation");

        SaveLoadMaster.SaveGeneralData();
        SaveLoadMaster.SavePlayerData();

        yield return new WaitForSeconds(2f);

        ActiveSaveImage(false);
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
        m_SaveImage.SetActive(value);
    }
    #endregion

}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour {

    #region Singleton
    public static LoadSceneManager Instance;

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

    [SerializeField] private GameObject m_LoadSceneUI;
    [SerializeField] private Slider m_LoadSlider;

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        m_LoadSceneUI.SetActive(false); //hide load ui
    }

    private IEnumerator LoadSceneAsyncWithUI(string sceneName)
    {
        yield return ScreenFaderManager.Instance.FadeToBlack();

        SetActiveLoadScene(true);

        yield return ScreenFaderManager.Instance.FadeToClear();

        yield return LoadSceneAsync(sceneName);

        yield return new WaitForSeconds(0.02f); //TODO: Remove 

        yield return ScreenFaderManager.Instance.FadeToBlack();

        SetActiveLoadScene(false);

        yield return ScreenFaderManager.Instance.FadeToClear();
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            var progress = Mathf.Clamp01(operation.progress / .9f);

            m_LoadSlider.value = progress;

            yield return null;
        }
    }

    private void SetActiveLoadScene(bool active)
    {
        m_LoadSceneUI.SetActive(active);

        m_LoadSlider.value = 0f;
    }

    #endregion

    #region public methods

    public void Load(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncWithUI(sceneName));
    }

    public void LoadWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public IEnumerator LoadWithFade(string sceneName, string locationName, Vector2 spawnPosition)
    {
        yield return ScreenFaderManager.Instance.FadeToBlack();

        yield return LoadSceneAsync(sceneName);

        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.RespawnWithSpawnPosition(spawnPosition);
            GameMaster.Instance.RecreateSceneState(locationName);
        }

        yield return new WaitForSeconds(1.5f);

        yield return ScreenFaderManager.Instance.FadeToClear();

        if (!string.IsNullOrEmpty(locationName) & AnnouncerManager.Instance != null)
            AnnouncerManager.Instance.DisplayAnnouncerMessage(new AnnouncerManager.Message(locationName, AnnouncerManager.Message.MessageType.Scene, 3f));
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;
using System.Linq;

public class GameMaster : MonoBehaviour {

    public Transform RespawnPoint;
    public bool isPlayerDead;

    private GameObject m_PlayerToRespawn;
    private bool isPlayerRespawning;

    [SerializeField] private string Music = "Background";

    #region Singleton
    public static GameMaster Instance;

    public void Awake()
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

        #region Initialize Managers
        Initialize("Managers/EventSystem");

        Initialize("Managers/AudioManager");

        Initialize("Managers/LoadSceneManager");

        Initialize("Managers/ScreenFaderManager");

        Initialize("Managers/PauseMenuManager");

        Initialize("Managers/AnnouncerManager");

        Initialize("Managers/DialogueManager");

        Initialize("Managers/UIManager");

        InitializeRespawnPoint();

        InitalizePlayerToRespawn();
        #endregion

        InitializeBackgroundMusic();

        InitializeSceneState();
    }

    #endregion


    #region SceneRecreation
    private List<State> ScenesState;

    public string SceneName;

    private void Start()
    {
        if (Camera.main.GetComponent<Camera2DFollow>().target == null)
            InitializePlayerRespawn(false);
    }

    public void SetPlayerPosition(Vector2 spawnPosition)
    {
        if (!GameObject.FindGameObjectWithTag("Player"))
        {
            RespawnWithoutFade(spawnPosition);
        }
    }

    public void RecreateSceneState(string sceneName)
    {
        SceneName = sceneName;

        var searchResult = ScenesState.FirstOrDefault(x => x.SceneName == SceneName);

        if (searchResult != null)
        {
            foreach (var item in searchResult.ObjectsState)
                Recreate(item.Key, item.Value);

            foreach (var item in searchResult.ObjectsPosition)
                Recreate(item.Key, item.Value);
        }
    }

    private void Recreate(string name, bool state)
    {
        var searchGameObjectResult = GameObject.Find(name);

        if (searchGameObjectResult != null)
        {
            Destroy(searchGameObjectResult);
        }
    }

    private void Recreate(string name, Vector2 state)
    {
        var searchGameObjectResult = GameObject.Find(name);

        if (searchGameObjectResult != null)
        {
            searchGameObjectResult.transform.position = state;
        }
    }

    public void SaveBoolState(string name, bool state)
    {
        var searchResult = ScenesState.FirstOrDefault(x => x.SceneName == SceneName);

        if (searchResult == null)
        {
            var newState = new State(SceneName);

            newState.ObjectsState.Add(name, state);

            ScenesState.Add(newState);
        }
        else
        {
            if (!searchResult.IsExistInBool(name))
                searchResult.ObjectsState.Add(name, state);
        }
    }

    public void SavePositionState(string name, Vector2 state)
    {
        var searchResult = ScenesState.FirstOrDefault(x => x.SceneName == SceneName);

        if (searchResult == null)
        {
            var newState = new State(SceneName);

            newState.ObjectsPosition.Add(name, state);

            ScenesState.Add(newState);
        }
        else
        {
            name = ClearName(name);

            if (!searchResult.IsExistInPosition(name))
            {
                searchResult.ObjectsPosition.Add(name, state);
            }
            else
            {
                searchResult.ObjectsPosition[name] = state;
            }
        }
    }

    private string ClearName(string name)
    {
        return name.Replace("(Clone)", "");
    }

    #endregion

    #region Initializers
    void Initialize(string name)
    {
        var gameObjectToInstantiate = Resources.Load(name);
        Instantiate(gameObjectToInstantiate);
    }

    private void InitializeSceneState()
    {
        if (ScenesState == null)
            ScenesState = new List<State>();
    }

    void InitializeRespawnPoint()
    {
        if (RespawnPoint == null)
            RespawnPoint = GameObject.FindWithTag("RespawnPoint").transform;

        if (RespawnPoint == null)
        {
            Debug.LogError("GameMaster: Can't find Respawn Point on scene");
        }
    }

    private void InitializeBackgroundMusic()
    {
        AudioManager.Instance.SetBackgroundMusic(Music);
    }

    void InitalizePlayerToRespawn()
    {
        m_PlayerToRespawn = Resources.Load("Player") as GameObject;

        if (m_PlayerToRespawn == null)
        {
            Debug.LogError("GameMaster: Can't fint Player to respawn");
        }
    }

    public void ChangeRespawnPoint(Transform respawnPointTransform)
    {
        if (respawnPointTransform != null)
        {
            if (!ReferenceEquals(RespawnPoint, respawnPointTransform))
            {
                if (RespawnPoint != null)
                    RespawnPoint.GetComponent<RespawnPoint>().SetActiveFlame(false);
                
                RespawnPoint = respawnPointTransform;
            }
        }
        else
        {
            Debug.LogError("GameMaster: can't reasign m_Respawn point, because new respawn point is null");
        }
    }

    public void InitializePlayerRespawn(bool isPlayerDied)
    {
        if (!isPlayerRespawning)
        {
            if (RespawnPoint != null)
            {
                if (m_PlayerToRespawn != null)
                {
                    isPlayerRespawning = true;

                    if (isPlayerDied)
                        StartCoroutine(RespawnWithFade());
                    else
                        RespawnWithoutFade();
                }
                else
                {
                    Debug.LogError("GameMaster: Couldn't respawn player, because GameMaster couldn't load Player from Resources.");
                }
            }
            else
            {
                Debug.LogError("GameMaster: Can't respawn player, because GameMaster couldn't found RespawnPoint on scene.");
            }
        }
    }
    #endregion

    private IEnumerator RespawnWithFade()
    {
        isPlayerDead = true;

        yield return new WaitForSeconds(1f);

        yield return ScreenFaderManager.Instance.FadeToBlack();

        Camera.main.GetComponent<Camera2DFollow>().ChangeCameraPosition(RespawnPoint.position);
        var playerTransform = RespawnWithoutFade();
        Camera.main.GetComponent<Camera2DFollow>().ChangeCameraTarget(playerTransform);

        isPlayerDead = false;

        yield return new WaitForSeconds(0.5f);

        yield return ScreenFaderManager.Instance.FadeToClear();
    }

    private Transform RespawnWithoutFade()
    {
        isPlayerDead = true;

        var playerGameObject = Instantiate(m_PlayerToRespawn, RespawnPoint.position, m_PlayerToRespawn.transform.rotation);

        isPlayerRespawning = false;
        isPlayerDead = false;

        return playerGameObject.transform;
    }

    private void RespawnWithoutFade(Vector2 respawnPosition)
    {
        var respawnPlayer = Instantiate(m_PlayerToRespawn);
        respawnPlayer.transform.position = respawnPosition;

        Camera.main.GetComponent<Camera2DFollow>().SetTarget(respawnPlayer.transform);
    }
}

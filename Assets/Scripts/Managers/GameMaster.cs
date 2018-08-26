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

            #region Initialize Managers
            Initialize("Managers/EventSystem");

            Initialize("Managers/AudioManager");

            Initialize("Managers/LoadSceneManager");

            Initialize("Managers/ScreenFaderManager");

            Initialize("Managers/PauseMenuManager");

            Initialize("Managers/AnnouncerManager");

            Initialize("Managers/DialogueManager");

            Initialize("Managers/UIManager");

            Initialize("Managers/JournalManager");

            InitializeRespawnPoint();

            InitalizePlayerToRespawn();
            #endregion

            InitializeBackgroundMusic();

            InitializeSceneState();
        }
    }

    #endregion

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

    #region SceneRecreation
    private List<State> ScenesState;

    public string SceneName;

    public enum RecreateType { Object, Position, Dialogue, ChestItem, Task }

    #region Recreate

    public void RecreateSceneState(string sceneName)
    {
        SceneName = sceneName;

        var searchResult = ScenesState.FirstOrDefault(x => x.SceneName == SceneName);

        if (searchResult != null)
        {
            Recreate(searchResult.ObjectsState, RecreateType.Object);
            Recreate(searchResult.ObjectsPosition, RecreateType.Position);
            Recreate(searchResult.Tasks, RecreateType.Task);

            Recreate(searchResult.DialogueIsComplete, RecreateType.Dialogue);
            Recreate(searchResult.ChestItems, RecreateType.ChestItem);
        }
    }

    private void Recreate(List<string> list, RecreateType recreateType)
    {
        foreach (var item in list)
        {
            RecreateObjectState(item, 0, recreateType);
        }
    }

    private void Recreate<T>(Dictionary<string, T> list, RecreateType recreateType)
    {
        if (recreateType == RecreateType.Position)
        {
            foreach (var item in list)
            {
                RecreateObjectState(item.Key, item.Value, recreateType);
            }
        }
        else if (recreateType == RecreateType.ChestItem)
        {
            foreach (var item in list)
            {
                RecreateObjectState((string)(object)item.Value, item.Key, recreateType);
            }
        }
    }

    private void RecreateObjectState<T>(string objectToFind, T value, RecreateType recreateType)
    {
        var searchGameObjectResult = GameObject.Find(objectToFind);

        Debug.LogError("Need to destroy - " + objectToFind);

        if (searchGameObjectResult != null)
        { 
            switch (recreateType)
            {
                case RecreateType.Object:
                    Debug.LogError(objectToFind);
                    Destroy(searchGameObjectResult);
                    break;

                case RecreateType.Dialogue:
                    searchGameObjectResult.GetComponent<DialogueTrigger>().dialogue.IsDialogueFinished = true;
                    break;

                case RecreateType.Task:
                    if (searchGameObjectResult.GetComponent<TaskManager>() != null)
                        searchGameObjectResult.GetComponent<TaskManager>().DestroyObject();
                    break;

                case RecreateType.Position:
                    searchGameObjectResult.transform.position = (Vector2)(object)value;
                    break;

                case RecreateType.ChestItem:
                    searchGameObjectResult.GetComponent<Chest>().RemoveFromChest((string)(object)value);
                    break;
            }
        }
    }

    #endregion

    #region SaveState

    private State GetState(string sceneName)
    {
        State searchResult = null;

        if (!string.IsNullOrEmpty(sceneName))
            searchResult = ScenesState.FirstOrDefault(x => x.SceneName == sceneName);
        else
            searchResult = ScenesState.FirstOrDefault(x => x.SceneName == SceneName);

        if (searchResult == null)
        {
            searchResult = new State(SceneName);

            ScenesState.Add(searchResult);
        }

        return searchResult;
    }

    public void SaveState<T>(string name, T value, RecreateType recreateType, string sceneName = "")
    {
        var state = GetState(sceneName);
        
        switch (recreateType)
        {
            case RecreateType.Object:

                if (!state.IsExistInBool(name))
                    state.ObjectsState.Add(name);

                break;

            case RecreateType.Dialogue:

                state.DialogueIsComplete.Add(name);

                break;

            case RecreateType.Task:

                state.Tasks.Add(name);

                break;

            case RecreateType.ChestItem:

                state.ChestItems.Add((string)(object)value, name);

                break;

            case RecreateType.Position:

                name = ClearName(name);

                if (!state.IsExistInPosition(name))
                {
                    state.ObjectsPosition.Add(name, (Vector3)(object)value);
                }
                else
                {
                    state.ObjectsPosition[name] = (Vector3)(object)value;
                }

                break;
        }
    }

    #endregion

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

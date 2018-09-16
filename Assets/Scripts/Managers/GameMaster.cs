using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;
using System.Linq;

public class GameMaster : MonoBehaviour {

    #region public fields

    public enum RecreateType { Object, Position, Dialogue, ChestItem, Task }

    public Transform m_RespawnPoint;
    [HideInInspector] public Vector3 m_RespawnPointPosition;
    public bool isPlayerDead;
    public string SceneName;

    #endregion

    #region private fields

    private GameObject m_PlayerToRespawn;
    private bool isPlayerRespawning;
    [SerializeField] private string Music = "Background";

    #endregion

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

            Initialize("Managers/InfoManager");

            Initialize("Managers/SaveLoadManager");

            Initialize("Managers/MapManager");

            if (StartScreenManager.IsLoadPressed)
            {
                SaveLoadManager.Instance.LoadGameData();

                InfoManager.Instance.RecreateState();
            }

            
            InitializeRespawnPoint();

            InitalizePlayerToRespawn();

            InitializeBackgroundMusic();

            InitializeSceneState();
            
            #endregion
        }
    }

    #endregion

    private void Start()
    {
        if (Camera.main.GetComponent<Camera2DFollow>().target == null)
            InitializePlayerRespawn(false);
    }

    #region SceneRecreation

    #region Recreate

    public void RecreateSceneState(string sceneName)
    {
        SceneName = sceneName;

        var searchResult = State.ScenesState.FirstOrDefault(x => x.SceneName == SceneName);

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

        if (searchGameObjectResult != null)
        { 
            switch (recreateType)
            {
                case RecreateType.Object:
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
                    var position = (ObjectPosition)(object)value;
                    searchGameObjectResult.transform.position = new Vector2(position.x, position.y);
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
            searchResult = State.ScenesState.FirstOrDefault(x => x.SceneName == sceneName);
        else
            searchResult = State.ScenesState.FirstOrDefault(x => x.SceneName == SceneName);

        if (searchResult == null)
        {
            searchResult = new State(SceneName);

            State.ScenesState.Add(searchResult);
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
                    state.ObjectsPosition.Add(name, (ObjectPosition)(object)value);
                }
                else
                {
                    state.ObjectsPosition[name] = (ObjectPosition)(object)value;
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
        if (State.ScenesState == null)
            State.ScenesState = new List<State>();
        else
            RecreateSceneState(SceneName);
    }

    void InitializeRespawnPoint()
    {
        if (m_RespawnPoint == null)
        {
            m_RespawnPoint = GameObject.FindWithTag("RespawnPoint").transform;
        }

        if (m_RespawnPointPosition == Vector3.zero)
        {
            m_RespawnPointPosition = m_RespawnPoint.position;
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
            if (!ReferenceEquals(m_RespawnPointPosition, respawnPointTransform))
            {
                if (m_RespawnPoint != null)
                    m_RespawnPoint.GetComponent<RespawnPoint>().SetActiveFlame(false);
                
                m_RespawnPoint = respawnPointTransform;
                m_RespawnPointPosition = respawnPointTransform.position;
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
            if (m_RespawnPointPosition != null)
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

    #region Respawn

    public void SetPlayerPosition(Vector2 spawnPosition)
    {
        if (!GameObject.FindGameObjectWithTag("Player"))
        {
            RespawnWithoutFade(spawnPosition);
        }
    }

    private IEnumerator RespawnWithFade()
    {
        isPlayerDead = true;

        yield return new WaitForSeconds(1f);

        yield return ScreenFaderManager.Instance.FadeToBlack();

        var playerTransform = RespawnWithoutFade();

        isPlayerDead = false;

        yield return new WaitForSeconds(0.5f);

        yield return ScreenFaderManager.Instance.FadeToClear();
    }

    private Transform RespawnWithoutFade()
    {
        isPlayerDead = true;

        var playerGameObject = Instantiate(m_PlayerToRespawn);
        playerGameObject.transform.position = m_RespawnPointPosition;

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

    #endregion
}

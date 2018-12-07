using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityStandardAssets._2D;

public class GameMaster : MonoBehaviour {

#region public fields

    public enum RecreateType { Object, Position, Dialogue, ChestItem, Task } //types of object state recriation

    public Transform m_RespawnPoint; //current respawn point
    [HideInInspector] public Vector3 m_RespawnPointPosition; //respawn position
    [HideInInspector] public bool IsPlayerDead; //is player dead
    public string SceneName; //current scene name

#endregion

#region private fields

    [Header("Respawn")]
    [SerializeField] private GameObject m_PlayerToRespawn;
    [SerializeField] private Transform m_ReturnPoint;

    [SerializeField] private Audio BackgroundMusic;

    private bool m_IsPlayerRespawning; //is player respawning
    public bool m_IsPlayerReturning;

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

            //Initialize("Managers/AnnouncerManager");

            Initialize("Managers/DialogueManager");

            Initialize("Managers/UIManager");

            Initialize("Managers/InfoManager");

            Initialize("Managers/SaveLoadManager");

            Initialize("Managers/LocalizationManager");

            Initialize("Managers/FPSManager");

#if MOBILE_INPUT
            Initialize("Managers/MobileTouchControl");
#endif

            //is load button was pressed in start screen
            if (StartScreenManager.IsLoadPressed) 
            {
                SaveLoadManager.Instance.LoadGameData();

                InfoManager.Instance.RecreateState();
            }
            
            InitializeRespawnPointPosition();

            AudioManager.Instance.SetBackgroundMusic(BackgroundMusic.name);

            InitializeSceneState();
            
#endregion
        }
    }

#endregion

#region Initializers
    private void Initialize(string name)
    {
        var gameObjectToInstantiate = Resources.Load(name);
        Instantiate(gameObjectToInstantiate);
    }

    private void InitializeSceneState()
    {
        if (State.ScenesState == null) //if there is no current state for this scene
            State.ScenesState = new List<State>();
        else //recreate scene state
            RecreateSceneState(SceneName);
    }

    private void InitializeRespawnPointPosition()
    {
        if (m_RespawnPointPosition == Vector3.zero & m_RespawnPoint != null)
        {
            m_RespawnPointPosition = m_RespawnPoint.position;
        }
    }

    public void ChangeRespawnPoint(Transform respawnPointTransform)
    {
        if (!ReferenceEquals(m_RespawnPointPosition, respawnPointTransform))
        {
            if (m_RespawnPoint != null) //deactivate previous respawn point flame
                m_RespawnPoint.GetComponent<RespawnPoint>().SetActiveFlame(false);

            //change respawn point
            m_RespawnPoint = respawnPointTransform;
            m_RespawnPointPosition = respawnPointTransform.position;
        }
    }

    public void StartPlayerRespawn(bool respawnWithFade)
    {
        if (!m_IsPlayerRespawning) //if player is not respawning now
        {
            m_IsPlayerRespawning = true;

            if (respawnWithFade)
                StartCoroutine(RespawnWithFade());
            else
                RespawnWithoutFade();
        }
    }
#endregion

    //put player on the scene
    private void Start()
    {
        StartPlayerRespawn(false); 
    }

#region SceneRecreation

#region Recreate

    public void RecreateSceneState(string sceneName)
    {
        SceneName = sceneName; //save scene name

        var searchResult = State.ScenesState.FirstOrDefault(x => x.SceneName == SceneName); //find saved state

        //recreate all objects
        if (searchResult != null) 
        {
            Recreate(searchResult.ObjectsState, RecreateType.Object);
            Recreate(searchResult.ObjectsPosition, RecreateType.Position);
            Recreate(searchResult.Tasks, RecreateType.Task);

            Recreate(searchResult.DialogueIsComplete, RecreateType.Dialogue);
            Recreate(searchResult.ChestItems, RecreateType.ChestItem);
        }
    }

    //recreate simple objects state
    private void Recreate(List<string> list, RecreateType recreateType)
    {
        foreach (var item in list)
        {
            RecreateObjectState(item, 0, recreateType);
        }
    }

    //recreate complex objects state
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

    //general recriation
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
            searchResult = new State(string.IsNullOrEmpty(sceneName) ? SceneName : sceneName);

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

    //clear object name
    private string ClearName(string name)
    {
        return name.Replace("(Clone)", "");
    }

#endregion

#region Respawn

    public void SetReturnPoint(Transform returnPoint)
    {
        if (returnPoint != m_ReturnPoint)
        {
            m_ReturnPoint = returnPoint;
        }
    }

    public void RespawnPlayerOnReturnPoint(GameObject player)
    {
        if (m_ReturnPoint != null & !m_IsPlayerReturning)
        {
            m_IsPlayerReturning = true;

            StartCoroutine(PlayerOnReturnPoint(player));
        }
    }

    private IEnumerator PlayerOnReturnPoint(GameObject player)
    {
        player.SetActive(false);

        yield return ScreenFaderManager.Instance.FadeToBlack();

        player.transform.position = m_ReturnPoint.transform.position;

        yield return new WaitForSeconds(0.5f);

        player.SetActive(true);

        yield return ScreenFaderManager.Instance.FadeToClear();

        m_IsPlayerReturning = false;
    }

    public void RespawnWithSpawnPosition(Vector2 spawnPosition)
    {
        StartCoroutine(RespawnWithoutFade(spawnPosition, 1.5f));
    }

    private IEnumerator RespawnWithFade()
    {
        IsPlayerDead = true;

        yield return new WaitForSeconds(1f);

        yield return ScreenFaderManager.Instance.FadeToBlack();

        RespawnWithoutFade();

        IsPlayerDead = false;

        yield return new WaitForSeconds(0.5f);

        yield return ScreenFaderManager.Instance.FadeToClear();
    }

    private void RespawnWithoutFade()
    {
        IsPlayerDead = true;

        var playerGameObject = Instantiate(m_PlayerToRespawn);
        playerGameObject.transform.position = m_RespawnPointPosition;

        m_IsPlayerRespawning = false;
        IsPlayerDead = false;
    }

    private IEnumerator RespawnWithoutFade(Vector2 respawnPosition, float waitTimer)
    {
        var respawnPlayer = Instantiate(m_PlayerToRespawn);
        respawnPlayer.transform.position = respawnPosition;

        //restrict player movement
        var playerBody = respawnPlayer.transform.GetChild(0).gameObject;

        playerBody.GetComponent<PlatformerCharacter2D>().enabled = false;

        yield return new WaitForSeconds(waitTimer);

        playerBody.GetComponent<PlatformerCharacter2D>().enabled = true;
    }

#endregion
}

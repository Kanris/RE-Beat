using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

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
        Initialize("EventSystem");

        Initialize("AudioManager");

        Initialize("LoadSceneManager");

        Initialize("ScreenFaderManager");

        Initialize("PauseMenuManager");

        InitializeRespawnPoint();

        InitalizePlayerToRespawn();
        #endregion

        RespawnPlayer();
    }

    #endregion


    private Transform m_RespawnPoint;
    private GameObject m_PlayerToRespawn;

    void Initialize(string name)
    {
        var gameObjectToInstantiate = Resources.Load(name);
        Instantiate(gameObjectToInstantiate);
    }

    void InitializeRespawnPoint()
    {
        m_RespawnPoint = GameObject.FindWithTag("RespawnPoint").transform;

        if (m_RespawnPoint == null)
        {
            Debug.LogError("GameMaster: Can't find Respawn Point on scene");
        }
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
            if (!ReferenceEquals(m_RespawnPoint, respawnPointTransform))
            {
                if (m_RespawnPoint != null)
                    m_RespawnPoint.GetComponent<RespawnPoint>().SetActiveFlame(false);
                
                m_RespawnPoint = respawnPointTransform;
            }
        }
        else
        {
            Debug.LogError("GameMaster: can't reasign m_Respawn point, because new respawn point is null");
        }
    }

    public void RespawnPlayer()
    {
        if (m_RespawnPoint != null)
        {
            if (m_PlayerToRespawn != null)
            {
                Instantiate(m_PlayerToRespawn, m_RespawnPoint.position, m_RespawnPoint.rotation);
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

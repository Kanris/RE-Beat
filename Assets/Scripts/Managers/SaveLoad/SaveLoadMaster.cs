using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public static class SaveLoadMaster {

    #region private const fields

    private const string SAVE_PLAYER_FILE_NAME = "Player.sav"; //player save file name
    private const string SAVE_GENERAL_FILE_NAME = "General.sav"; //general save file name
    private const string SAVE_OPTIONS_FILE_NAME = "Options.sav"; //options save file name

    #endregion

    #region public methods

    public static void SavePlayerData()
    {
        SaveData<PlayerGameData>(SAVE_PLAYER_FILE_NAME);
    }

    public static void SaveGeneralData()
    {
        SaveData<GeneralGameData>(SAVE_GENERAL_FILE_NAME);
    }

    public static void SaveOptionsData()
    {
        SaveData<OptionsGameData>(SAVE_OPTIONS_FILE_NAME);
    }

    public static PlayerGameData LoadPlayerData()
    {
        return LoadData<PlayerGameData>(SAVE_PLAYER_FILE_NAME);
    }

    public static GeneralGameData LoadGeneralData()
    {
        return LoadData<GeneralGameData>(SAVE_GENERAL_FILE_NAME);
    }

    public static OptionsGameData LoadOptionsData()
    {
        return LoadData<OptionsGameData>(SAVE_OPTIONS_FILE_NAME);
    }

    public static string GetSceneToLoad()
    {
        if (IsSaveFileExists(SAVE_GENERAL_FILE_NAME)) //if general save file exists
        {
            using (var stream = new FileStream(GetPathToSaveFile(SAVE_GENERAL_FILE_NAME), FileMode.Open)) //open general save file
            {
                var binaryFormatter = new BinaryFormatter();
                var gameData = binaryFormatter.Deserialize(stream) as GeneralGameData;

                return gameData.CurrentScene; //get scene name
            }
        }

        return string.Empty;
    }

    #endregion

    #region private methods

    private static void RecreateState(IGameData gameData)
    {
        if (gameData != null)
            gameData.RecreateState();
    }

    private static void SaveData<T>(string path)
        where T : new()
    {
        using (var stream = new FileStream(GetPathToSaveFile(path), FileMode.Create)) //create save file
        {
            var binaryFormatter = new BinaryFormatter();
            var dataToSave = new T();

            binaryFormatter.Serialize(stream, dataToSave);
        }
    }

    private static T LoadData<T>(string path)
        where T : class, IGameData
    {
        if (IsSaveFileExists(path)) //if file exists
        {
            using (var stream = new FileStream(GetPathToSaveFile(path), FileMode.Open)) //open saved file
            {
                var binaryFormatter = new BinaryFormatter();
                var gameData = binaryFormatter.Deserialize(stream) as T;

                RecreateState(gameData);

                return gameData;
            }
        }

        return null;
    }

    private static bool IsSaveFileExists(string fileName)
    {
        return File.Exists(GetPathToSaveFile(fileName));
    }

    private static string GetPathToSaveFile(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName;
    }

    #endregion

}

interface IGameData
{
    void RecreateState();
}

[System.Serializable]
public class GeneralGameData : IGameData
{
    #region fields

    public List<State> ScenesState; //get state scene
    public string CurrentScene; //current scene name

    public float RespawnPointX; //current respawn point x
    public float RespawnPointY; //current respawn point y

    public Dictionary<string, Task> CurrentTasks; //current tasks list
    public Dictionary<string, Task> CompletedTasks; //complete tasks list
    #endregion

    #region constructor

    public GeneralGameData()
    {
        //get current game state
        ScenesState = State.ScenesState;
        CurrentScene = SceneManager.GetActiveScene().name;
        RespawnPointX = GameMaster.Instance.m_RespawnPointPosition.x;
        RespawnPointY = GameMaster.Instance.m_RespawnPointPosition.y;

        if (InfoManager.Instance != null)
        {
            CurrentTasks = InfoManager.Instance.CurrentTasks;
            CompletedTasks = InfoManager.Instance.CompletedTasks;
        }
        else
            Debug.LogError("Can't save journal state");
    }

    #endregion

    #region public methods

    public void RecreateState()
    {
        //load saved scene state
        State.ScenesState = ScenesState;
        GameMaster.Instance.m_RespawnPointPosition = new Vector3(RespawnPointX, RespawnPointY);

        if (InfoManager.Instance != null)
        {
            InfoManager.Instance.CurrentTasks = CurrentTasks;
            InfoManager.Instance.CompletedTasks = CompletedTasks;
        }
        else
            Debug.LogError("Can't recreate journal state");
    }

    #endregion
}

[System.Serializable]
public class PlayerGameData : IGameData
{
    #region fields

    public int DamageAmount; //player damage amount
    public float AttackSpeed; //player attack speed
    public Inventory PlayerInventory; //player inventory
    public int CurrentPlayerHealth; //current health
    public int Coins; //current amount of money

    #endregion

    #region constructor

    public PlayerGameData()
    {
        //save current player state
        DamageAmount = PlayerStats.DamageAmount;
        AttackSpeed = PlayerStats.MeleeAttackSpeed;
        PlayerInventory = PlayerStats.PlayerInventory;
        CurrentPlayerHealth = PlayerStats.CurrentPlayerHealth;
        Coins = PlayerStats.Scrap;
    }

    #endregion

    #region public methods

    public void RecreateState()
    {
        //load saved player state
        PlayerStats.DamageAmount = DamageAmount;
        PlayerStats.MeleeAttackSpeed = AttackSpeed;
        PlayerStats.PlayerInventory = PlayerInventory;
        PlayerStats.CurrentPlayerHealth = CurrentPlayerHealth;
        PlayerStats.Scrap = Coins;
    }

    #endregion
}

[System.Serializable]
public class OptionsGameData : IGameData
{
    #region public fields

    public int ResolutionIndex;
    public bool IsFullscreen;
    public float VolumeMaster;
    public float VolumeEnvironment;
    public string LocalizationToLoad;

    #endregion

    #region constructor

    public OptionsGameData()
    {
        this.ResolutionIndex = StartScreenManager.ResolutionIndex;
        this.IsFullscreen = StartScreenManager.IsFullscreen;
        this.VolumeMaster = StartScreenManager.VolumeMaster;
        this.VolumeEnvironment = StartScreenManager.VolumeEnvironment;
        this.LocalizationToLoad = StartScreenManager.LocalizationToLoad;
    }

    #endregion

    #region public methods

    public void RecreateState()
    {
        StartScreenManager.ResolutionIndex = ResolutionIndex;
        StartScreenManager.IsFullscreen = IsFullscreen;
        StartScreenManager.VolumeMaster = VolumeMaster;
        StartScreenManager.VolumeEnvironment = VolumeEnvironment;
        StartScreenManager.LocalizationToLoad = LocalizationToLoad;
    }

    #endregion
}

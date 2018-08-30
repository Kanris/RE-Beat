using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public static class SaveLoadMaster {

    #region fields

    private const string SAVE_PLAYER_FILE_NAME = "Player.sav";
    private const string SAVE_GENERAL_FILE_NAME = "General.sav";

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

    public static PlayerGameData LoadPlayerData()
    {
        return LoadData<PlayerGameData>(SAVE_PLAYER_FILE_NAME);
    }

    public static GeneralGameData LoadGeneralData()
    {
        return LoadData<GeneralGameData>(SAVE_GENERAL_FILE_NAME);
    }

    public static string GetLoadScene()
    {
        if (IsSaveFileExists(SAVE_GENERAL_FILE_NAME))
        {
            using (var stream = new FileStream(GetPathToSaveFile(SAVE_GENERAL_FILE_NAME), FileMode.Open))
            {
                var binaryFormatter = new BinaryFormatter();
                var gameData = binaryFormatter.Deserialize(stream) as GeneralGameData;

                return gameData.CurrentScene;
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
        using (var stream = new FileStream(GetPathToSaveFile(path), FileMode.Create))
        {
            var binaryFormatter = new BinaryFormatter();
            var dataToSave = new T();

            if (dataToSave is GeneralGameData)
            {
                var generalGameData = dataToSave as GeneralGameData;
            }
            binaryFormatter.Serialize(stream, dataToSave);
        }
    }

    private static T LoadData<T>(string path)
        where T : class, IGameData
    {
        if (IsSaveFileExists(path))
        {
            using (var stream = new FileStream(GetPathToSaveFile(path), FileMode.Open))
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

    public List<State> ScenesState;
    public string CurrentScene;

    public float RespawnPointX;
    public float RespawnPointY;

    public Dictionary<string, Task> CurrentTasks;
    public Dictionary<string, Task> CompletedTasks;
    #endregion

    #region constructor

    public GeneralGameData()
    {
        ScenesState = State.ScenesState;
        CurrentScene = SceneManager.GetActiveScene().name;
        RespawnPointX = GameMaster.Instance.m_RespawnPointPosition.x;
        RespawnPointY = GameMaster.Instance.m_RespawnPointPosition.y;

        if (JournalManager.Instance != null)
        {
            CurrentTasks = JournalManager.Instance.CurrentTasks;
            CompletedTasks = JournalManager.Instance.CompletedTasks;
        }
        else
            Debug.LogError("Can't save journal state");
    }

    #endregion

    #region public methods

    public void RecreateState()
    {
        State.ScenesState = ScenesState;
        GameMaster.Instance.m_RespawnPointPosition = new Vector3(RespawnPointX, RespawnPointY);

        if (JournalManager.Instance != null)
        {
            JournalManager.Instance.CurrentTasks = CurrentTasks;
            JournalManager.Instance.CompletedTasks = CompletedTasks;
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

    public int DamageAmount;
    public float AttackSpeed;
    public float Invincible;
    public Inventory PlayerInventory;
    public int CurrentPlayerHealth;

    #endregion

    #region constructor

    public PlayerGameData()
    {
        DamageAmount = PlayerStats.DamageAmount;
        AttackSpeed = PlayerStats.AttackSpeed;
        Invincible = PlayerStats.Invincible;
        PlayerInventory = PlayerStats.PlayerInventory;
        CurrentPlayerHealth = PlayerStats.CurrentPlayerHealth;
    }

    #endregion

    #region public methods

    public void RecreateState()
    {
        PlayerStats.DamageAmount = DamageAmount;
        PlayerStats.AttackSpeed = AttackSpeed;
        PlayerStats.Invincible = Invincible;
        PlayerStats.PlayerInventory = PlayerInventory;
        PlayerStats.CurrentPlayerHealth = CurrentPlayerHealth;
    }

    #endregion
}

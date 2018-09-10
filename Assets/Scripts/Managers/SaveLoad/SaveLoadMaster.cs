﻿using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public static class SaveLoadMaster {

    #region fields

    private const string SAVE_PLAYER_FILE_NAME = "Player.sav";
    private const string SAVE_GENERAL_FILE_NAME = "General.sav";
    private const string SAVE_OPTIONS_FILE_NAME = "Options.sav";

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

    public int DamageAmount;
    public float AttackSpeed;
    public float Invincible;
    public Inventory PlayerInventory;
    public int CurrentPlayerHealth;
    public int Coins;

    #endregion

    #region constructor

    public PlayerGameData()
    {
        DamageAmount = PlayerStats.DamageAmount;
        AttackSpeed = PlayerStats.AttackSpeed;
        Invincible = PlayerStats.Invincible;
        PlayerInventory = PlayerStats.PlayerInventory;
        CurrentPlayerHealth = PlayerStats.CurrentPlayerHealth;
        Coins = PlayerStats.Coins;
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
        PlayerStats.Coins = Coins;
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

    #endregion

    #region constructor

    public OptionsGameData()
    {
        this.ResolutionIndex = StartScreenManager.ResolutionIndex;
        this.IsFullscreen = StartScreenManager.IsFullscreen;
        this.VolumeMaster = StartScreenManager.VolumeMaster;
        this.VolumeEnvironment = StartScreenManager.VolumeEnvironment;
    }

    #endregion

    #region public methods

    public void RecreateState()
    {
        StartScreenManager.ResolutionIndex = ResolutionIndex;
        StartScreenManager.IsFullscreen = IsFullscreen;
        StartScreenManager.VolumeMaster = VolumeMaster;
        StartScreenManager.VolumeEnvironment = VolumeEnvironment;
    }

    #endregion
}

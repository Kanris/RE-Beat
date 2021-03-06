﻿using UnityEngine;

public class MoveToNextScene : MonoBehaviour {

    #region public fields

    public delegate void VoidDelegate(bool value);
    public static event VoidDelegate IsMoveToNextScene;

    #endregion

    #region private fields

    #region serialize fields

    [Header("Next scene properties")]
    [SerializeField] private string NextScene; //next scene name
    [SerializeField] private string NextScenename; //next scene caption

    [Header("Player spawn location")]
    [SerializeField] private Vector2 SpawnPosition; //where to spawn player

    [Header("Effects")]
    [SerializeField] private Audio BackgroundMusic; //play new background music

    [Header("Player")]
    [SerializeField] private bool m_IsLookingRight = true;

    #endregion

    private bool m_IsPlayerNear; //is player near to the teleport
    private bool m_IsDestroyingItem; //is item is near the teleport

    #endregion

    #region private methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if player is near the teleport
        if (collision.CompareTag("Player") & !m_IsPlayerNear) 
        {
            m_IsPlayerNear = true; //player is near the teleport

            if (!string.IsNullOrEmpty(NextScene)) //if there is next scene name
            {
                GameMaster.Instance.SceneName = NextScenename;

                if (IsMoveToNextScene != null)
                {
                    IsMoveToNextScene(true); //notify that player teleport to the next scene
                }

                LoadSceneManager.Instance.StartCoroutine(
                    LoadSceneManager.Instance.LoadWithFade(NextScene, NextScenename, SpawnPosition, m_IsLookingRight)); //start teleport with fade

                if (!string.IsNullOrEmpty(BackgroundMusic))
                    AudioManager.Instance.SetBackgroundMusic(BackgroundMusic); //change background music
            }
            else
                Debug.LogError("MoveToNextScene.OnTriggerEnter2D: NextScene variable is not initialized.");
        }
        //if item in scene move trigger
        else if (collision.CompareTag("Item") & !m_IsDestroyingItem) 
        {
            //destroy item
            m_IsDestroyingItem = true;
            Destroy(collision.gameObject);
            m_IsDestroyingItem = false;
        }
    }

    #endregion
}

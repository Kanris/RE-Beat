﻿using System.Collections;
using System.Collections.Generic;
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

    private GameObject m_LoadSceneUI;
    private Slider m_LoadSlider;

    // Use this for initialization
    void Start () {

        InitializeLoadSceneUI();

        InitializeLoadSlider();

        m_LoadSceneUI.SetActive(false);
    }

    private void InitializeLoadSceneUI()
    {
        var transformGameChild = transform.GetChild(0);

        if (transformGameChild != null)
        {
            m_LoadSceneUI = transformGameChild.gameObject;
        }
        else
        {
            Debug.LogError("LoadSceneManager.InitializeLoadSceneUI: Can't find child");
        }          
    }

    private void InitializeLoadSlider()
    {
        m_LoadSlider = m_LoadSceneUI.GetComponentInChildren<Slider>();

        if (m_LoadSlider == null)
        {
            Debug.LogError("LoadSceneManager.InitializeLoadSlider: Can't find slider in m_LoadSlider");
        }
    }
	
	public void Load(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return ScreenFaderManager.Instance.FadeToBlack();

        SetActiveLoadScene(true);

        yield return ScreenFaderManager.Instance.FadeToClear();
        //TODO: SaveGame

        var operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            var progress = Mathf.Clamp01(operation.progress / .9f);

            if (m_LoadSlider != null) m_LoadSlider.value = progress;

            yield return null;
        }

        yield return new WaitForSeconds(2f); //TODO: Remove 

        yield return ScreenFaderManager.Instance.FadeToBlack();

        SetActiveLoadScene(false);

        yield return ScreenFaderManager.Instance.FadeToClear();
    }

    private void SetActiveLoadScene(bool active)
    {
        if (m_LoadSceneUI != null)
            m_LoadSceneUI.SetActive(active);

        if (m_LoadSlider != null)
            m_LoadSlider.value = 0f;
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
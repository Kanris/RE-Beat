using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAppearOnTrigger : MonoBehaviour {

    [SerializeField] private GameObject ObjectToAppear;
    [SerializeField] private GameObject ShowOnDestroy;
    [SerializeField] private bool DestroyOnTrigger;

    private bool m_IsQuitting;

    #region Initialize

    private void Start()
    {
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting;
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting;
    }

    #endregion

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AppearObject();
            DestroyThisTrigger();
        }
    }

    private void AppearObject()
    {
        if (ObjectToAppear != null)
        {
            ObjectToAppear.SetActive(true);
        }
    }

    private void DestroyThisTrigger()
    {
        if (DestroyOnTrigger)
        {
            GameMaster.Instance.SaveState(gameObject.name, 0, GameMaster.RecreateType.Object);
            Destroy(gameObject);
        }
    }

    #region OnDestroy

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting)
        {
            if (ShowOnDestroy != null)
            {
                ShowOnDestroy.SetActive(true);
            }
        }
    }

    #endregion
}

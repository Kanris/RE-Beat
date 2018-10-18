using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour {

    #region Singleton
    public static UIManager Instance;

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

    #region private fields

    #region serialize fields

    [SerializeField] private GameObject m_UI;
    [SerializeField] private GameObject m_LifePanel;
    [SerializeField] private GameObject m_LifeImage;

    #endregion

    private List<GameObject> m_HealthInPanel = new List<GameObject>();

    #endregion

    #region private methods

    #endregion

    #region public methods

    public void AddHealth(int amount)
    {
        for (int index = 0; index < amount; index++)
        {
            m_HealthInPanel.Add(Instantiate(m_LifeImage, m_LifePanel.transform));
        }
    }

    public void RemoveHealth(int amount)
    {
        for (int index = m_HealthInPanel.Count - 1; index >= 0 & m_HealthInPanel.Count > 0 & amount > 0; index--, amount--)
        {
            var objectToDestroy = m_HealthInPanel[index];
            m_HealthInPanel.RemoveAt(index);

            objectToDestroy.GetComponent<Animator>().SetTrigger("Prepare");
            Destroy(objectToDestroy, 1.6f);
        }
    }

    public void Clear()
    {
        foreach (var health in m_HealthInPanel)
        {
            Destroy(health);
        }

        m_HealthInPanel.Clear();
    }

    #endregion
}

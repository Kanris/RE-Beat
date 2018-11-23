using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

            InitializeList();
        }
    }

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField] private GameObject m_UI;
    [SerializeField] private GameObject m_LifePanel;
    [SerializeField] private Image m_BulletImage;

    #endregion

    private List<GameObject> m_HealthInPanel = new List<GameObject>();
    private int m_CurrentActiveHPIndex = 0;

    #endregion

    private void InitializeList()
    {
        for (var index = 0; index < m_LifePanel.transform.childCount; index++)
        {
            m_HealthInPanel.Add(m_LifePanel.transform.GetChild(index).gameObject);
        }

        m_CurrentActiveHPIndex = m_HealthInPanel.Count - 1;
    }

    #region public methods

    public void AddHealth()
    {
        if (m_CurrentActiveHPIndex <= 5)
        {
            m_HealthInPanel[m_CurrentActiveHPIndex].GetComponent<Animator>().SetBool("Disable", false);

            if (m_CurrentActiveHPIndex < 5)
                m_CurrentActiveHPIndex++;

        }
    }

    public void RemoveHealth()
    {
        if (m_CurrentActiveHPIndex >= 0)
        {
            m_HealthInPanel[m_CurrentActiveHPIndex].GetComponent<Animator>().SetBool("Disable", true);

            if (m_CurrentActiveHPIndex > 0)
                m_CurrentActiveHPIndex--;
        }
    }

    public void ResetState()
    {
        for (var index = 0; index < m_HealthInPanel.Count; index++)
        {
            m_HealthInPanel[index].GetComponent<Animator>().SetBool("Disable", false);
        }

        m_CurrentActiveHPIndex = m_HealthInPanel.Count - 1;
    }

    #region bullet

    public void BulletCooldown(float cooldown)
    {
        StartCoroutine(DisplayBulletCooldown(cooldown));
    }

    private IEnumerator DisplayBulletCooldown(float time)
    {
        m_BulletImage.fillAmount = 0f;

        var tickTime = time * .1f;

        while (m_BulletImage.fillAmount < 1)
        {
            yield return new WaitForSeconds(tickTime);
            m_BulletImage.fillAmount += .1f;
        }
    }

    #endregion

    #endregion
}

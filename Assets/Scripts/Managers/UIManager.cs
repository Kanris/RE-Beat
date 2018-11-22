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
        }
    }

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField] private GameObject m_UI;
    [SerializeField] private GameObject m_LifePanel;
    [SerializeField] private GameObject m_LifeImage;
    [SerializeField] private Image m_BulletImage;

    #endregion

    private List<GameObject> m_HealthInPanel = new List<GameObject>();
    private int m_CurrentActiveHPIndex = 0;

    #endregion

    #region private methods

    #endregion

    #region public methods

    public void AddHealth(int amount)
    {
        if (m_HealthInPanel.Count == 0)
        {
            for (int index = 0; index < amount; index++)
            {
                m_HealthInPanel.Add(Instantiate(m_LifeImage, m_LifePanel.transform));
            }

            m_CurrentActiveHPIndex = m_HealthInPanel.Count - 1;
        }
        else
        {
            var border = m_CurrentActiveHPIndex + amount;

            for (; m_CurrentActiveHPIndex <= border; m_CurrentActiveHPIndex++)
            {
                m_HealthInPanel[m_CurrentActiveHPIndex].GetComponent<Animator>().SetTrigger("Idle");
            }
        }
    }

    public void RemoveHealth(int amount)
    {
        if (m_CurrentActiveHPIndex >= 0)
        {
            m_HealthInPanel[m_CurrentActiveHPIndex].GetComponent<Animator>().SetTrigger("Remove");

            m_CurrentActiveHPIndex--;
        }
    }

    public void ClearHealth()
    {
        foreach (var health in m_HealthInPanel)
        {
            Destroy(health);
        }

        m_HealthInPanel.Clear();
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

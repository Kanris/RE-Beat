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

    [SerializeField] private TextMeshProUGUI Text; //current coins amount
    [SerializeField] private TextMeshProUGUI AddCoins; //coins to add
    [SerializeField] private GameObject m_UI;
    [SerializeField] private GameObject m_LifePanel;
    [SerializeField] private GameObject m_LifeImage;

    #endregion

    private List<GameObject> m_HealthInPanel = new List<GameObject>();

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        PlayerStats.OnCoinsAmountChange += ChangeCoinsAmount; //subscribe on coins amount change
    }

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

    public IEnumerator ChangeCoinsAmount(int value)
    { 
        if (Text != null & AddCoins != null)
        {
            AddCoins.gameObject.SetActive(true);
            AddCoins.text = "+" + value.ToString();

            yield return new WaitForSeconds(1f);

            var currentCoinsCount = int.Parse(Text.text);
            var addAmount = value;

            for (int index = 0; index < value; index++)
            {
                currentCoinsCount += 1;
                Text.text = currentCoinsCount.ToString();

                addAmount -= 1;
                AddCoins.text = "+" + addAmount.ToString();

                yield return new WaitForSeconds(0.01f);
            }

            AddCoins.gameObject.SetActive(false);
        }
    }

    #endregion
}

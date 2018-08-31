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

    private GameObject m_UI;
    private GameObject m_LayoutGrid;
    private List<GameObject> m_HealthInPanel = new List<GameObject>();

    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private TextMeshProUGUI AddCoins;

    // Use this for initialization
    void Start () {

        InitializeUI();

        InitializeLayoutGrid();

        PlayerStats.OnCoinsAmountChange += ChangeCoinsAmount;
    }

    private void InitializeUI()
    {
        if (transform.childCount > 0)
        {
            m_UI = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("UIManager.InitializeUI: Can't find child in UIManager.");
        }
    }

    private void InitializeLayoutGrid()
    {
        m_LayoutGrid = m_UI.transform.GetChild(0).gameObject;

        if (m_LayoutGrid == null)
        {
            Debug.LogError("UIManager.InitializeLayoutGrid: Can't find layout grid");
        }
    }

    private void SetActiveUI(bool isActive)
    {
        m_UI.SetActive(isActive);
    }

    public void AddHealth(int amount)
    {
        for (int index = 0; index < amount; index++)
        {
            var healthObject = GetHealthObject();
            m_HealthInPanel.Add(healthObject);
        }
    }

    public void RemoveHealth(int amount)
    {
        for (int index = m_HealthInPanel.Count - 1; index >= 0 & m_HealthInPanel.Count > 0 & amount > 0; index--, amount--)
        {
            var objectToDestroy = m_HealthInPanel[index];
            m_HealthInPanel.RemoveAt(index);

            objectToDestroy.GetComponent<Animator>().SetTrigger("Prepare");
            Destroy(objectToDestroy, 3f);
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

    private GameObject GetHealthObject()
    {
        var health = Resources.Load("UI/Health") as GameObject;
        var healthGameObject = Instantiate(health);
        healthGameObject.transform.SetParent(m_LayoutGrid.transform);
        
        return healthGameObject;
    }
}

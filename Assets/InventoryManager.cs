using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class InventoryManager : MonoBehaviour {

    #region serialize fields

    [SerializeField] private GameObject m_InventoryUI;
    [SerializeField] private Transform m_ButtonsGrid;
    [SerializeField] private GameObject m_DescriptionUI;
    [SerializeField] private TextMeshProUGUI m_ItemNameText;
    [SerializeField] private TextMeshProUGUI m_ItemDescriptionText;
    #endregion

    #region private fields

    private List<InventoryItem> m_ItemsList;
    private static Sprite[] itemsSpriteAtlas;

    #endregion

    #region public fields

    public static InventoryManager Instance;
    public delegate void DelegateVoid(bool state);
    public event DelegateVoid OnInventoryOpen;

    #endregion

    #region private methods

    #region initialize

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

            m_ItemsList = new List<InventoryItem>();
        }
    }

    // Use this for initialization
    void Start () {

        InitializeItemsAtlas();
        m_InventoryUI.SetActive(false);
    }

    private void InitializeItemsAtlas()
    {
        //string tileTextureName = GetComponent<SpriteRenderer>().sprite.texture.name;
        itemsSpriteAtlas = Resources.LoadAll<Sprite>("Items/items1");
    }

    private Sprite GetSpriteFromAtlass(string name)
    {
        Sprite returnSprite = null;

        if (itemsSpriteAtlas != null)
        {
            returnSprite = itemsSpriteAtlas.SingleOrDefault(x => x.name == name);
        }

        return returnSprite;
    }

    #endregion

    // Update is called once per frame
    void Update () {
		
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (m_InventoryUI.activeSelf)
                m_DescriptionUI.SetActive(false);

            m_InventoryUI.SetActive(!m_InventoryUI.activeSelf);

            if (OnInventoryOpen != null)
                OnInventoryOpen(m_InventoryUI.activeSelf);
        }

	}

    #endregion

    #region public methods

    public void InitializeInventory()
    {
        if (PlayerStats.PlayerInventory.m_Bag.Count > 0)
        {
            foreach (var inventoryItem in PlayerStats.PlayerInventory.m_Bag)
            {
                AddItem(inventoryItem);
            }
        }
    }

    public void AddItem(Item item)
    {
        var resourceItemButton = Resources.Load("UI/InventoryItem") as GameObject;
        var instantiateItemButton = Instantiate(resourceItemButton, m_ButtonsGrid);

        var newInventoryItem = instantiateItemButton.GetComponent<InventoryItem>();

        newInventoryItem.Initialize(item, GetSpriteFromAtlass(item.Image),
            m_DescriptionUI, m_ItemNameText, m_ItemDescriptionText);

        m_ItemsList.Add(newInventoryItem);
    }

    public void RemoveItem(string name)
    {
        var itemToRemove = m_ItemsList.FirstOrDefault(x => x.m_ItemName == name);

        if (itemToRemove != null)
        {
            m_ItemsList.Remove(itemToRemove);
            Destroy(itemToRemove.gameObject);
        }
    }

    #endregion
}

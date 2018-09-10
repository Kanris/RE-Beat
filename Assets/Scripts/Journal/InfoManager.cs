using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class InfoManager : MonoBehaviour {

    #region serialize fields

    [SerializeField] private GameObject m_JournalUI;
    [SerializeField] private TextPage m_Page;
    [SerializeField] private GameObject m_Bookmarks;
    [SerializeField] private Transform m_Content;

    #endregion

    #region private fields

    private int m_CurrentOpenBookmark = 0;
    private Dictionary<int, List<Button>> m_ButtonsList;
    private static Sprite[] itemsSpriteAtlas;

    #endregion

    #region public fields

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnJournalOpen;

    public Dictionary<string, Task> CurrentTasks;
    public Dictionary<string, Task> CompletedTasks;

    #endregion

    #region private methods

    #region Singleton

    public static InfoManager Instance;

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

            m_ButtonsList = new Dictionary<int, List<Button>>();
            InitializeButtonsDictionary();
        }
    }

    #endregion
   
    // Use this for initialization
    void Start () {
        
        m_JournalUI.SetActive(false);

        if (CurrentTasks == null)
            CurrentTasks = new Dictionary<string, Task>();

        if (CompletedTasks == null)
            CompletedTasks = new Dictionary<string, Task>();

        InitializeItemsAtlas();
    }

    #region Initialize

    private void InitializeItemsAtlas()
    {
        itemsSpriteAtlas = Resources.LoadAll<Sprite>("Items/items1");
    }

    private void InitializeButtonsDictionary()
    {
        for(int index = 0; index < m_Bookmarks.transform.childCount; index++)
        {
            m_ButtonsList.Add(index, new List<Button>());
        }
    }

    #endregion

    // Update is called once per frame
    void Update () {
		
        if (Input.GetKeyDown(KeyCode.J))
        {
            InfoManagement(0);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            InfoManagement(2);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            InfoManagement(3);
        }
    }

    private void InfoManagement(int id)
    {
        var value = true;

        if (m_CurrentOpenBookmark != id)
        {
            OpenBookmark(id);
        }
        else if (m_JournalUI.activeSelf)
        {
            value = false;
        }

        m_JournalUI.SetActive(value);

        OnJournalOpen(m_JournalUI.activeSelf);
    }

    private void ChangeButtonsVision(bool value, IEnumerable<Button> buttons)
    {
        foreach (var item in buttons)
        {
            item.gameObject.SetActive(value);
        }
    }

    private Button CreateTaskButton(string name)
    {
        var buttonFromResources = Resources.Load("Managers/Journal/TaskButton") as GameObject;
        var instantiateTaskButton = Instantiate(buttonFromResources, m_Content);

        instantiateTaskButton.name = name;
        instantiateTaskButton.GetComponentInChildren<TextMeshProUGUI>().text = name;

        return instantiateTaskButton.GetComponent<Button>();
    }

    private Button CreateItemButton(Item item)
    {
        var resourceItemButton = Resources.Load("Managers/Journal/InventoryItem") as GameObject;
        var instantiateItemButton = Instantiate(resourceItemButton, m_Content);

        instantiateItemButton.name = item.Name;
        instantiateItemButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.Name;
        instantiateItemButton.transform.GetChild(1).GetComponent<Image>().sprite = itemsSpriteAtlas.SingleOrDefault(x => x.name == item.Image);

        instantiateItemButton.GetComponent<InventoryItem>().Initialize(item, m_Page);

        instantiateItemButton.SetActive(false);

        return instantiateItemButton.GetComponent<Button>();
    }

    #endregion

    #region public methods

    public void OpenBookmark(int id)
    {
        ChangeButtonsVision(true, m_ButtonsList[id]);
        ChangeButtonsVision(false, m_ButtonsList[m_CurrentOpenBookmark]);

        m_Bookmarks.transform.GetChild(id).transform.position -= new Vector3(10, 0);
        m_Bookmarks.transform.GetChild(m_CurrentOpenBookmark).transform.position -= new Vector3(-10, 0);

        m_CurrentOpenBookmark = id;
        m_Page.ClearText();
        AudioManager.Instance.Play("OpenJournal");
    }

    public void CloseJournal()
    {
        m_JournalUI.SetActive(false);
    }

    #endregion

    #region task methods

    public void DisplayTaskText(string taskName)
    {
        if (CurrentTasks.ContainsKey(taskName))
        {
            m_Page.ShowText(CurrentTasks[taskName].Text);
        }
        else if (CompletedTasks.ContainsKey(taskName))
        {
            m_Page.ShowText(CompletedTasks[taskName].Text);
        }
    }

    public bool AddTask(string taskName, string taskText)
    {
        if (!CurrentTasks.ContainsKey(taskName))
        {
            m_ButtonsList[0].Add(CreateTaskButton(taskName));
            var newTask = new Task(taskName, taskText);
            CurrentTasks.Add(taskName, newTask);

            return true;
        }

        return false;
    }

    public void RecreateState()
    {
        if (CurrentTasks != null)
        {
            if (m_ButtonsList[0] != null)
            {
                foreach (var item in CurrentTasks)
                {
                    m_ButtonsList[0].Add(CreateTaskButton(item.Value.Name));
                }
            }
        }

        if (CompletedTasks != null)
        {
            if (m_ButtonsList[1] != null)
            {
                foreach (var item in CompletedTasks)
                {
                    m_ButtonsList[1].Add(CreateTaskButton(item.Key));
                }
            }
        }

        if (PlayerStats.PlayerInventory != null)
        {
            if (m_ButtonsList[2] != null)
            {
                foreach (var item in PlayerStats.PlayerInventory.m_Bag)
                {
                    AddItem(item);
                }
            }
        }
    }

    public bool UpdateTask(string taskName, string taskText)
    {
        if (CurrentTasks.ContainsKey(taskName))
        {
            CurrentTasks[taskName].TaskUpdate(taskText);

            return true;
        }

        return false;
    }

    public bool CompleteTask(string taskname, string taskText)
    {
        if (CurrentTasks.ContainsKey(taskname))
        {
            CurrentTasks[taskname].TaskComplete(taskText);

            CompletedTasks.Add(taskname, CurrentTasks[taskname]);
            CurrentTasks.Remove(taskname);

            return true;
        }

        return false;
    }

    #endregion

    #region inventory methods

    public void AddItem(Item item)
    {
        var button = CreateItemButton(item);

        m_ButtonsList[2].Add(button);
    }

    public void RemoveItem(string name)
    {
        var itemToRemove = m_ButtonsList[2].FirstOrDefault(x => x.name == name);

        if (itemToRemove != null)
        {
            m_ButtonsList[2].Remove(itemToRemove);
            Destroy(itemToRemove.gameObject);
        }
    }

    #endregion
}
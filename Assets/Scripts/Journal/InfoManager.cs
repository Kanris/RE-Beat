using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class InfoManager : MonoBehaviour {

    #region public fields

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnJournalOpen; //event on journal open

    public Dictionary<string, Task> CurrentTasks; //current tasks list
    public Dictionary<string, Task> CompletedTasks; //complete task list

    #endregion

    #region private fields

    #region serialize fields

    [SerializeField] private GameObject m_JournalUI; //journal ui
    [SerializeField] private TextPage m_Page; //main page text
    [SerializeField] private GameObject m_Bookmarks; //bookmark list
    [SerializeField] private Transform m_Content; //buttons grid

    #endregion

    private int m_CurrentOpenBookmark = 0; //current open tab
    private Dictionary<int, List<Button>> m_ButtonsList; //buttons list
    private static Sprite[] itemsSpriteAtlas; //atals for items

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

    #region Initialize

    // Use this for initialization
    private void Start () {
        
        m_JournalUI.SetActive(false); //hide journal ui

        if (CurrentTasks == null)
            CurrentTasks = new Dictionary<string, Task>(); //initialize current tasks

        if (CompletedTasks == null)
            CompletedTasks = new Dictionary<string, Task>(); //initialize completed tasks

        itemsSpriteAtlas = Resources.LoadAll<Sprite>("Items/items1"); //initialize items atlas
    }

    private void InitializeButtonsDictionary()
    {
        for(int index = 0; index < m_Bookmarks.transform.childCount; index++)  //initialize buttons list for each bookmark
        {
            m_ButtonsList.Add(index, new List<Button>());
        }
    }

    #endregion

    // Update is called once per frame
    private void Update () {
		
        if (Input.GetKeyDown(KeyCode.J)) //open journal
        {
            InfoManagement(0);
        }
        else if (Input.GetKeyDown(KeyCode.I)) //open inventory
        {
            InfoManagement(2);
        }
        else if (Input.GetKeyDown(KeyCode.B)) //open "bestiary"
        {
            InfoManagement(3);
        }
    }

    private void InfoManagement(int id)
    {
        var value = true; //open journal

        if (m_CurrentOpenBookmark != id) //if need to open another bookmark
        {
            OpenBookmark(id); 
        }
        else if (m_JournalUI.activeSelf) //if journal is already open
        {
            value = false; //close journal
        }

        m_JournalUI.SetActive(value);

        OnJournalOpen(m_JournalUI.activeSelf); //notify that journal open/close
    }

    private void ChangeButtonsVisibility(bool value, IEnumerable<Button> buttons)
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

        instantiateTaskButton.name = name; //set button name to task
        instantiateTaskButton.GetComponentInChildren<TextMeshProUGUI>().text = name; //change button caption to the task name

        return instantiateTaskButton.GetComponent<Button>();
    }

    private Button CreateItemButton(Item item)
    {
        var resourceItemButton = Resources.Load("Managers/Journal/InventoryItem") as GameObject;
        var instantiateItemButton = Instantiate(resourceItemButton, m_Content);

        instantiateItemButton.name = item.Name; //change button name to the item name
        instantiateItemButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.Name; //change button caption to the item name
        instantiateItemButton.transform.GetChild(1).GetComponent<Image>().sprite = itemsSpriteAtlas.SingleOrDefault(x => x.name == item.Image); //add item image from atlas

        instantiateItemButton.GetComponent<InventoryItem>().Initialize(item, m_Page); //initialize item button info

        instantiateItemButton.SetActive(false); //hide button

        return instantiateItemButton.GetComponent<Button>();
    }

    #endregion

    #region public methods

    public void OpenBookmark(int id)
    {
        ChangeButtonsVisibility(false, m_ButtonsList[m_CurrentOpenBookmark]); //hide current bookmark buttons
        ChangeButtonsVisibility(true, m_ButtonsList[id]); //show new bookmark buttons

        //change bookmarks positions too show player what bookmark is currently open
        m_Bookmarks.transform.GetChild(id).transform.position -= new Vector3(10, 0);
        m_Bookmarks.transform.GetChild(m_CurrentOpenBookmark).transform.position -= new Vector3(-10, 0);

        m_CurrentOpenBookmark = id; //change current book mark id
        m_Page.ClearText(); //clear main text 

        AudioManager.Instance.Play("OpenJournal"); //play open journal sound
    }

    public void CloseJournal()
    {
        m_JournalUI.SetActive(false); //hide journal ui
        OnJournalOpen(false); //notify that journal is close
    }

    #endregion

    #region task methods

    public void DisplayTaskText(string taskName)
    {
        if (CurrentTasks.ContainsKey(taskName)) //search task in current tasks
        {
            m_Page.ShowText(CurrentTasks[taskName].Text); //show task description
        }
        else if (CompletedTasks.ContainsKey(taskName)) //search task in completed tasks
        {
            m_Page.ShowText(CompletedTasks[taskName].Text); //show task description
        }
    }

    public bool AddTask(string taskName, string taskText)
    {
        if (!CurrentTasks.ContainsKey(taskName)) //add new task if it is not already added
        {
            m_ButtonsList[0].Add(CreateTaskButton(taskName)); //create new task button
            var newTask = new Task(taskName, taskText); //create new task
            CurrentTasks.Add(taskName, newTask); //add task to the list

            return true; //task was successfuly added
        }

        return false; //can't add task
    }

    public void RecreateState()
    {
        if (CurrentTasks != null)
        {
            if (m_ButtonsList[0] != null) //if current task buttons is initialized
            {
                foreach (var item in CurrentTasks) //add all current tasks from the saved state
                {
                    m_ButtonsList[0].Add(CreateTaskButton(item.Value.Name)); 
                }
            }
        }

        if (CompletedTasks != null)
        {
            if (m_ButtonsList[1] != null) //if completed task buttons is initialized
            {
                foreach (var item in CompletedTasks) //add all completed tasks from the saved state
                {
                    m_ButtonsList[1].Add(CreateTaskButton(item.Key));
                }
            }
        }

        if (PlayerStats.PlayerInventory != null)
        {
            if (m_ButtonsList[2] != null) //if inventory buttons is initialized
            {
                foreach (var item in PlayerStats.PlayerInventory.m_Bag) //add all items from the saved state
                {
                    AddItem(item);
                }
            }
        }
    }

    public bool UpdateTask(string taskName, string taskText)
    {
        if (CurrentTasks.ContainsKey(taskName)) //if task in the journal
        {
            CurrentTasks[taskName].TaskUpdate(taskText); //updated task

            return true; //task was updated
        }

        return false; //can't updated task
    }

    public bool CompleteTask(string taskname, string taskText)
    {
        if (CurrentTasks.ContainsKey(taskname)) //if task is in the journal
        {
            CurrentTasks[taskname].TaskComplete(taskText); //complete task

            //move task from the current task to the completed
            CompletedTasks.Add(taskname, CurrentTasks[taskname]);
            CurrentTasks.Remove(taskname);

            return true; //task was successfuly updated
        }

        return false; //can't updated task
    }

    #endregion

    #region inventory methods

    public void AddItem(Item item)
    {
        var button = CreateItemButton(item); //create new item button

        m_ButtonsList[2].Add(button); //add new item to the button list
    }

    public void RemoveItem(string name)
    {
        var itemToRemove = m_ButtonsList[2].FirstOrDefault(x => x.name == name); //search item to delete

        if (itemToRemove != null) //if item was successfuly found
        {
            m_ButtonsList[2].Remove(itemToRemove); //remove item from the buttons
            Destroy(itemToRemove.gameObject); //remove item from the inventory
        }
    }

    #endregion
}
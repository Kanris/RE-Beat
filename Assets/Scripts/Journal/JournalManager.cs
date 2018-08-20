using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalManager : MonoBehaviour {

    #region Singleton

    public static JournalManager Instance;

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
    
    [SerializeField] private TextPage page;
    [SerializeField] private Transform content;
    [SerializeField] private Transform completeButton;
    [SerializeField] private Transform currentButton;

    private GameObject m_JournalUI;
    private Dictionary<string, Task> CurrentTasks;
    private Dictionary<string, Task> CompletedTasks;
    private string m_CurrentOpenPage;
    private float m_UpdateSearchTime;
    private Player m_Player;

    // Use this for initialization
    void Start () {

        InitializeJournalUI();

        page.ClearText();

        CurrentTasks = new Dictionary<string, Task>();

        CompletedTasks = new Dictionary<string, Task>();

        SetActiveUI(false);

    }

    #region Initialize

    private void InitializeJournalUI()
    {
        if (transform.childCount > 0)
        {
            m_JournalUI = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("JournalManager.InitializeJournalUI: can't initialize JournalUI there is no children on gameobject");
        }
    }

    #endregion

    // Update is called once per frame
    void Update () {
		
        if (Input.GetKeyDown(KeyCode.J))
        {
            page.ClearText();

            AudioManager.Instance.Play("OpenJournal");
            m_Player.TriggerPlayerBussy(!m_JournalUI.activeSelf);

            SetActiveUI(!m_JournalUI.activeSelf);
        }

        if (m_Player == null)
        {
            if (m_UpdateSearchTime < Time.time)
            {
                SearchForTarget();
            }
        }
	}

    private void SetActiveUI(bool value)
    {
        m_JournalUI.SetActive(value);
    }

    private void SearchForTarget()
    {
        var player = GameObject.FindWithTag("Player");

        if (player == null)
            m_UpdateSearchTime = Time.time + 1f;
        else
        {
            m_Player = player.GetComponent<Player>();
        }

    }

    public void DisplayTaskText(string taskName)
    {
        if (CurrentTasks.ContainsKey(taskName))
        {
            page.ShowText(CurrentTasks[taskName].Text);
        }
        else if (CompletedTasks.ContainsKey(taskName))
        {
            page.ShowText(CompletedTasks[taskName].Text);
        }
    }

    public bool AddTask(string taskName, string taskText)
    {
        if (!CurrentTasks.ContainsKey(taskName))
        {
            var taskButton = CreateTaskButton(taskName);
            var newTask = CreateNewTask(taskName, taskText, taskButton);
            CurrentTasks.Add(taskName, newTask);

            return true;
        }

        return false;
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

            OpenPage("completed");

            return true;
        }

        return false;
    }

    public void OpenPage(string name)
    {
        if (m_CurrentOpenPage != name)
        {
            m_CurrentOpenPage = name;
            page.ClearText();


            AudioManager.Instance.Play("OpenJournal");

            if (name == "completed")
            {
                OpenPage(false);
            }
            else
            {
                OpenPage(true);
            }
        }
    }

    private void OpenPage(bool isCurrentPressed)
    {
        ChangeButtonsVision(isCurrentPressed, CurrentTasks.Values);
        ChangeButtonsVision(!isCurrentPressed, CompletedTasks.Values);

        var moveVector = new Vector3(-10, 0);

        if (!isCurrentPressed)
        {
            moveVector *= -1;
        }

        currentButton.position += moveVector;
        completeButton.position -= moveVector;
    }

    private void ChangeButtonsVision(bool value, IEnumerable<Task> tasks)
    {
        foreach (var item in tasks)
        {
            item.TaskButton.gameObject.SetActive(value);
        }
    }

    private Task CreateNewTask(string taskName, string taskText, Button buttonTask)
    {
        return new Task(taskName, taskText, buttonTask);
    }

    private Button CreateTaskButton(string name)
    {
        var buttonFromResources = Resources.Load("Managers/Journal/TaskButton");
        var instantiateTaskButton = Instantiate(buttonFromResources, content) as GameObject;

        instantiateTaskButton.name = name;
        instantiateTaskButton.GetComponentInChildren<TextMeshProUGUI>().text = name;

        return instantiateTaskButton.GetComponent<Button>();
    }
}

[System.Serializable]
public class Task
{
    public string Name;
    public string Text;
    public Button TaskButton;
    public bool IsTaskComplete;

    public delegate void VoidDelegate();
    public event VoidDelegate OnTaskComplete;
    public event VoidDelegate OnTaskUpdate;

    public Task(string taskName, string taskText, Button taskButton)
    {
        this.Name = taskName;
        this.Text = taskText;
        this.TaskButton = taskButton;

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message("<#000000>" + Name + "</color> task has been added to journal - <#000000>J</color>", 3f));
    }

    public void TaskUpdate(string text)
    {
        this.Text = text + this.Text;
        if (OnTaskUpdate != null) OnTaskUpdate();

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message("<#000000>" + Name + "</color> task has been updated - <#000000>J</color>", 3f));
    }

    public void TaskComplete(string text)
    {
        IsTaskComplete = true;
        this.Text = text + this.Text;

        if (OnTaskComplete != null) OnTaskComplete();

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message("<#000000>" + Name + "</color> task has been complete - <#000000>J</color>", 3f));
    }
}

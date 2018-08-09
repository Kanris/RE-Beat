using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private Page page;
    [SerializeField] private Transform taskGrid;

    private GameObject m_JournalUI;
    private Dictionary<string, Task> TaskJournal;

	// Use this for initialization
	void Start () {

        InitializeJournalUI();

        page.ClearText();

        TaskJournal = new Dictionary<string, Task>();

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
            SetActiveUI(!m_JournalUI.activeSelf);
        }
	}

    private void SetActiveUI(bool value)
    {
        m_JournalUI.SetActive(value);
    }

    public void DisplayTaskText(string name)
    {
        if (TaskJournal.ContainsKey(name))
        {
            page.ShowText(TaskJournal[name].Text);
        }
    }

    public bool AddTask(string name, string task)
    {
        if (!TaskJournal.ContainsKey(name))
        {
            var taskButton = CreateTaskButton(name);
            var newTask = CreateNewTask(name, task, taskButton);
            TaskJournal.Add(name, newTask);

            return true;
        }

        return false;
    }

    public bool UpdateTask(string name, string task)
    {
        if (TaskJournal.ContainsKey(name))
        {
            TaskJournal[name].TaskUpdate(task);

            return true;
        }

        return false;
    }

    public bool CompleteTask(string name)
    {
        if (TaskJournal.ContainsKey(name))
        {
            TaskJournal[name].TaskComplete();
            TaskJournal.Remove(name);

            return true;
        }

        return false;
    }

    private Task CreateNewTask(string name, string task, Button buttonTask)
    {
        return new Task(name, task, buttonTask);
    }

    private Button CreateTaskButton(string name)
    {
        var buttonFromResources = Resources.Load("TaskButton");
        var instantiateTaskButton = Instantiate(buttonFromResources, taskGrid) as GameObject;

        instantiateTaskButton.name = name;

        return instantiateTaskButton.GetComponent<Button>();
    }
}

[System.Serializable]
public class Task
{
    public string Name;
    public string Text;
    public Button ButtonTask;
    public bool IsTaskComplete;

    public delegate void VoidDelegate();
    public event VoidDelegate OnTaskComplete;
    public event VoidDelegate OnTaskUpdate;

    public Task(string name, string text, Button buttonTask)
    {
        this.Name = name;
        this.Text = text;
        this.ButtonTask = buttonTask;

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message(Name + " task has been added to journal <#000000>J</color>", 3f));
    }

    public void TaskComplete()
    {
        IsTaskComplete = true;
        if(OnTaskComplete != null) OnTaskComplete();

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message(Name + " task has been complete", 3f));

        JournalManager.Destroy(ButtonTask.gameObject);
    }

    public void TaskUpdate(string text)
    {
        this.Text = text + this.Text;
        if (OnTaskUpdate != null) OnTaskUpdate();

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message(Name + " task has been updated", 3f));
    }
}

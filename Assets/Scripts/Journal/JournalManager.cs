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

    public void DisplayTaskText(string taskName)
    {
        if (TaskJournal.ContainsKey(taskName))
        {
            page.ShowText(TaskJournal[taskName].Text);
        }
    }

    public bool AddTask(string taskName, string taskText)
    {
        if (!TaskJournal.ContainsKey(taskName))
        {
            var taskButton = CreateTaskButton(taskName);
            var newTask = CreateNewTask(taskName, taskText, taskButton);
            TaskJournal.Add(taskName, newTask);

            return true;
        }

        return false;
    }

    public bool UpdateTask(string taskName, string taskText)
    {
        if (TaskJournal.ContainsKey(taskName))
        {
            TaskJournal[taskName].TaskUpdate(taskText);

            return true;
        }

        return false;
    }

    public bool CompleteTask(string taskname)
    {
        if (TaskJournal.ContainsKey(taskname))
        {
            TaskJournal[taskname].TaskComplete();
            TaskJournal.Remove(taskname);

            return true;
        }

        return false;
    }

    private Task CreateNewTask(string taskName, string taskText, Button buttonTask)
    {
        return new Task(taskName, taskText, buttonTask);
    }

    private Button CreateTaskButton(string name)
    {
        var buttonFromResources = Resources.Load("TaskButton");
        var instantiateTaskButton = Instantiate(buttonFromResources, taskGrid) as GameObject;

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

    public void TaskComplete()
    {
        IsTaskComplete = true;
        if(OnTaskComplete != null) OnTaskComplete();

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message("<#000000>" + Name + "</color> task has been complete - <#000000>J</color>", 3f));

        JournalManager.Destroy(TaskButton.gameObject);
    }

    public void TaskUpdate(string text)
    {
        this.Text = text + this.Text;
        if (OnTaskUpdate != null) OnTaskUpdate();

        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message("<#000000>" + Name + "</color> task has been updated - <#000000>J</color>", 3f));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    public string Name;
    public string Text;
    public bool IsTaskComplete;

    public delegate void VoidDelegate();
    public event VoidDelegate OnTaskComplete;
    public event VoidDelegate OnTaskUpdate;

    public Task(string taskName, string taskText)
    {
        this.Name = taskName;
        this.Text = taskText;

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


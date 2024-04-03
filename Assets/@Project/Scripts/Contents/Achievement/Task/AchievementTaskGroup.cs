using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TaskGroupState
{
    Inactive,
    Running,
    Complete
}

[System.Serializable]
public class AchievementTaskGroup
{
    [SerializeField]
    private AchievementTask[] tasks;

    public IReadOnlyList<AchievementTask> Tasks => tasks;
    public Achievement Owner { get; private set; }
    public bool IsAllTaskComplete => tasks.All(x => x.IsComplete);
    public bool IsComplete => State == TaskGroupState.Complete;
    public TaskGroupState State { get; private set; }

    public AchievementTaskGroup(AchievementTaskGroup copyTarget)
    {
        tasks = copyTarget.Tasks.Select(x => Object.Instantiate(x)).ToArray();
    }

    public void Setup(Achievement owner)
    {
        Owner = owner;
        foreach (var task in tasks)
            task.Setup(owner);
    }

    public void Start()
    {
        State = TaskGroupState.Running;
        foreach (var task in tasks)
            task.Start();
    }

    public void End()
    {
        State = TaskGroupState.Complete;
        foreach (var task in tasks)
            task.End();
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        foreach (var task in tasks)
        {
            if (task.IsTarget(category, target))
                task.ReceiveReport(successCount);
        }
    }

    public void Complete()
    {
        if (IsComplete)
            return;

        State = TaskGroupState.Complete;

        foreach (var task in tasks)
        {
            if (!task.IsComplete)
                task.Complete();
        }
    }
}

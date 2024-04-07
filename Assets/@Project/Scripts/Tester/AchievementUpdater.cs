using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementUpdater : MonoBehaviour
{
    [SerializeField] protected TaskCategory taskCategory;
    [SerializeField] protected TaskTarget taskTarget;
    protected int value;
    protected void Report()
    {
        Reporter.Report(taskCategory, taskTarget, value);
    }
}

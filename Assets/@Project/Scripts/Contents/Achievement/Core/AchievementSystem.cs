using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SocialPlatforms.Impl;
using Steamworks;
public class AchievementSystem
{
    #region Save Path
    private const string kSaveRootPath = "achievementSystem";
    private const string kActiveAchievementsSavePath = "activeAchievements";
    private const string kCompletedAchievementsSavePath = "completedAchievements";
    #endregion

    #region Events
    public delegate void AchievementRegisteredHandler(Achievement newAchievement);
    public delegate void AchievementIsReadyToCompleteHandler(Achievement achievement);
    public delegate void AchievementCompletedHandler(Achievement achievement);
    public delegate void AchievementCanceledHandler(Achievement achievement);
    #endregion

    private static bool isApplicationQuitting;

    private GameObject Prefab_AchievementAlarmTable { get; set; }
    public GameObject Prefab_CompleteAlarmUI { get; private set; }
    public GameObject Go_CurrentAchievementAlarmTable { get; private set; }

    private List<Achievement> activeAchievements = new List<Achievement>();
    private List<Achievement> completedAchievements = new List<Achievement>();

    private AchievementDatabase achievementDatabase;

    public event AchievementRegisteredHandler onAchievementRegistered;
    public event AchievementIsReadyToCompleteHandler onAchievementIsReadyToComplete;
    public event AchievementCompletedHandler onAchievementCompleted;
    public event AchievementCanceledHandler onAchievementCanceled;

    public IReadOnlyList<Achievement> ActiveAchievements => activeAchievements;
    public IReadOnlyList<Achievement> CompletedAchievements => completedAchievements;

    public void Init()
    {
        achievementDatabase = Resources.Load<AchievementDatabase>("Data/AchievementDatabase");
        Prefab_CompleteAlarmUI = Resources.Load<GameObject>("Prefabs/UI/Others/UI_AchievementAlarm");
        Prefab_AchievementAlarmTable = Resources.Load<GameObject>("Prefabs/UI/Others/UI_AchievementAlarmTable");

        GameObject achievementUpdater = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Common/@AchievementCommonUpdater"));
        achievementUpdater.name = achievementUpdater.name.Replace("(Clone)", "");

        if (!Load())
        {
            foreach (var achievement in achievementDatabase.Achievements)
            {
                Register(achievement);
            }
        }
        foreach (var achievement in achievementDatabase.achievements)
        {
            Giver(achievement);
        }

    }

    public void SaveOnQuit()
    {
        isApplicationQuitting = true;
        Save();
    }

    private void Giver(Achievement achievement)
    {
        if (!ContainsInCompletedAchievements(achievement) && !ContainsInActiveAchievements(achievement))
        {
            Debug.Log($"퀘스트 [{achievement.name}]를 새로 등록하였습니다");
            Register(achievement);
        }
    }

    public Achievement Register(Achievement achievement)
    {
        var newAchievement = achievement.Clone();

        if (newAchievement is Achievement)
        {
            newAchievement.onCompleted += OnAchievementCompleted;
            newAchievement.onWaitForComplete += OnAchievementIsReadyToComplete;

            activeAchievements.Add(newAchievement);

            newAchievement.OnRegister();
            onAchievementRegistered?.Invoke(newAchievement);
        }
        else
        {
            newAchievement.onCompleted += OnAchievementCompleted;
            newAchievement.onWaitForComplete += OnAchievementIsReadyToComplete;
            newAchievement.onCanceled += OnAchievementCanceled;

            activeAchievements.Add(newAchievement);

            newAchievement.OnRegister();
            onAchievementRegistered?.Invoke(newAchievement);
        }
        return newAchievement;
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        ReceiveReport(activeAchievements, category, target, successCount);
    }

    public void ReceiveReport(TaskCategory category, TaskTarget target, int successCount)
        => ReceiveReport(category.CodeName, target.Value, successCount);
    public void ReceiveReport(TaskCategory category, string target, int successCount)
        => ReceiveReport(category.CodeName, target, successCount);
    public void ReceiveReport(string category, TaskTarget target, int successCount)
        => ReceiveReport(category, target.Value, successCount);

    private void ReceiveReport(List<Achievement> achievements, string category, object target, int successCount)
    {
        foreach (var Achievement in achievements.ToArray())
            Achievement.ReceiveReport(category, target, successCount);
    }

    public void ReceiveRewardsAndCompleteAchievement(string codeName)
    {
        var achievement = activeAchievements.FirstOrDefault(a => a.CodeName == codeName && a.IsWaitingForCompletion);
        if (achievement != null)
        {
            achievement.ReceiveRewardsAndComplete();
        }
        else
        {
            Debug.LogWarning("지정된 코드 이름의 업적을 찾을 수 없거나, 업적이 완료 대기 상태가 아님.");
        }
    }

    public void CompleteWaitingAchievements()
    {
        foreach (var achievement in activeAchievements.ToList())
        {
            if (achievement.IsWaitingForCompletion)
                achievement.Complete();
        }
    }
    public bool ContainsInActiveAchievements(Achievement achievement) => activeAchievements.Any(x => x.CodeName == achievement.CodeName);

    public bool ContainsInCompletedAchievements(Achievement achievement) => completedAchievements.Any(x => x.CodeName == achievement.CodeName);

    private void Save()
    {
        var root = new JObject();
        root.Add(kActiveAchievementsSavePath, CreateSaveDatas(activeAchievements));
        root.Add(kCompletedAchievementsSavePath, CreateSaveDatas(completedAchievements));

        PlayerPrefs.SetString(kSaveRootPath, root.ToString());
        PlayerPrefs.Save();
    }

    private bool Load()
    {
        if (PlayerPrefs.HasKey(kSaveRootPath))
        {
            var root = JObject.Parse(PlayerPrefs.GetString(kSaveRootPath));

            LoadSaveDatas(root[kActiveAchievementsSavePath], achievementDatabase, LoadActiveAchievement);
            LoadSaveDatas(root[kCompletedAchievementsSavePath], achievementDatabase, LoadCompletedAchievement);

            return true;
        }
        else
            return false;
    }

    private JArray CreateSaveDatas(IReadOnlyList<Achievement> achievements)
    {
        var saveDatas = new JArray();
        foreach (var achievement in achievements)
        {
            //if (achievement.IsSavable)
                saveDatas.Add(JObject.FromObject(achievement.ToSaveData()));
        }
        return saveDatas;
    }

    private void LoadSaveDatas(JToken datasToken, AchievementDatabase database, System.Action<AchievementSaveData, Achievement> onSuccess)
    {
        try
        {
            var datas = datasToken as JArray;
            foreach (var data in datas)
            {
                var saveData = data.ToObject<AchievementSaveData>();
                var achievement = database.FindAchievementBy(saveData.codeName);
                onSuccess.Invoke(saveData, achievement);
            }
        }
        catch
        {
            Debug.LogError("업적데이터 로드 실패(\"AchievementDatabase\" SO파일 내용의 누락 또는, 인스턴스 업적 등록이 원인. 후자일경우 무시 바람)");
        }
    }

    private void LoadActiveAchievement(AchievementSaveData saveData, Achievement achievement)
    {
        var newAchievement = Register(achievement);
        newAchievement.LoadFrom(saveData);

        if (newAchievement.State == AchievementState.WaitingForCompletion || newAchievement.State == AchievementState.Complete)
            SteamAchievementBridge.UnlockAchievement(newAchievement.CodeName); // 로드 시, 스팀 도전과제 연동
    }

    private void LoadCompletedAchievement(AchievementSaveData saveData, Achievement achievement)
    {
        var newAchievement = achievement.Clone();
        newAchievement.LoadFrom(saveData);

        if (newAchievement is Achievement)
            completedAchievements.Add(newAchievement);
        else
            completedAchievements.Add(newAchievement);

        SteamAchievementBridge.UnlockAchievement(newAchievement.CodeName); // 로드 시, 스팀 도전과제 연동
    }

    #region Callback
    private void OnAchievementCompleted(Achievement achievement)
    {
        activeAchievements.Remove(achievement);
        completedAchievements.Add(achievement);

        onAchievementCompleted?.Invoke(achievement);
    }
    private void OnAchievementIsReadyToComplete(Achievement achievement)
    {
        DisplayCompleteAlarm(achievement);

        onAchievementIsReadyToComplete?.Invoke(achievement);

        SteamAchievementBridge.UnlockAchievement(achievement.CodeName); // 업적이 사실상 완료조건을 달성한 시점이기에, Steam 도전과제도 함께 해제
    }

    private void OnAchievementCanceled(Achievement achievement)
    {
        activeAchievements.Remove(achievement);
        onAchievementCanceled?.Invoke(achievement);

        MonoBehaviour.Destroy(achievement, Time.deltaTime);
    }
    #endregion
    private void DisplayCompleteAlarm(Achievement achievement)
    {
        if (Go_CurrentAchievementAlarmTable != null)
        {
            // 해당 Table에 Item 추가
        }
        else
        {
            // UI_AchievementAlarmTable 생성, 초기화
            Go_CurrentAchievementAlarmTable = MonoBehaviour.Instantiate(Prefab_AchievementAlarmTable);
            try { Go_CurrentAchievementAlarmTable.transform.SetParent(GameObject.Find("@UI_Root").transform, false); }
            catch { }
            // 해당 Table에 Item 추가
        }

        string TCodeName = $"ACHIEVEMENT-{achievement.CodeName}-NAME";
        string TCodeDesc = $"ACHIEVEMENT-{achievement.CodeName}-DESC";
        string TName = LocalizationSettings.StringDatabase.GetLocalizedString("Localization_Achievement Table", TCodeName, LocalizationSettings.SelectedLocale);
        string TDesc = LocalizationSettings.StringDatabase.GetLocalizedString("Localization_Achievement Table", TCodeDesc, LocalizationSettings.SelectedLocale);

        string desc = $"[{TName}] {TDesc}";
        
        Go_CurrentAchievementAlarmTable.GetComponent<UI_AchievementAlarmTable>().AddItem(desc);
    }

}

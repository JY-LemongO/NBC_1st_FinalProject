using UnityEngine;
using Steamworks;

/// <summary>
/// 게임 로직과 Steam 도전과제 사이를 이어주는 브릿지.
/// </summary>
public static class SteamAchievementBridge
{
    /// <summary>
    /// Steam 도전과제 API Name으로 언락.
    /// </summary>
    public static void UnlockAchievement(string apiName)
    {
        // 에디터에서 테스트 안 하고 싶은 경우
//#if UNITY_EDITOR
//        return;
//#endif

        if (!SteamManager.Initialized)
        {
            Debug.LogWarning($"[SteamAchievementBridge] SteamManager not initialized. Skip achievement: {apiName}");
            return;
        }

        if (string.IsNullOrEmpty(apiName))
        {
            Debug.LogWarning("[SteamAchievementBridge] apiName is null or empty.");
            return;
        }

        try
        {
            bool achieved;
            bool success = SteamUserStats.GetAchievement(apiName, out achieved);

            if (!success)
            {
                Debug.LogWarning($"[SteamAchievementBridge] GetAchievement failed or achievement not found: {apiName}");
                return;
            }

            if (achieved)
            {
                // 이미 언락된 상태면 그냥 정보용 로그만.
                Debug.Log($"[SteamAchievementBridge] Already unlocked: {apiName}");
                return;
            }

            SteamUserStats.SetAchievement(apiName);

            if (!SteamUserStats.StoreStats())
            {
                Debug.LogWarning($"[SteamAchievementBridge] StoreStats failed for: {apiName}");
            }
            else
            {
                Debug.Log($"[SteamAchievementBridge] Unlocked achievement: {apiName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SteamAchievementBridge] Exception while unlocking {apiName}: {e}");
        }
    }
}

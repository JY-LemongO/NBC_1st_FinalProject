using Steamworks;
using UnityEngine;

public static class SteamAchievementDebug
{
    public static void ResetAchievement(string apiName)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning($"[SteamAchievementDebug] Steam not initialized. Skip reset: {apiName}");
            return;
        }

        if (string.IsNullOrEmpty(apiName))
        {
            Debug.LogWarning("[SteamAchievementDebug] apiName is null or empty.");
            return;
        }

        try
        {
            bool success = SteamUserStats.ClearAchievement(apiName);
            if (!success)
            {
                Debug.LogWarning($"[SteamAchievementDebug] ClearAchievement failed or not found: {apiName}");
                return;
            }

            if (!SteamUserStats.StoreStats())
            {
                Debug.LogWarning($"[SteamAchievementDebug] StoreStats failed after reset: {apiName}");
            }
            else
            {
                Debug.Log($"[SteamAchievementDebug] Achievement reset (locked): {apiName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SteamAchievementDebug] Exception while resetting {apiName}: {e}");
        }
    }
}

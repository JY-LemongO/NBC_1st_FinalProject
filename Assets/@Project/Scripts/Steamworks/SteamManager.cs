using UnityEngine;
using Steamworks;

/// <summary>
/// Steamworks.NET 초기화 & 콜백 관리용 매니저.
/// 프로젝트에 한 개만 존재하면 됨.
/// </summary>
public class SteamManager : MonoBehaviour
{
    private static SteamManager _instance;
    public static SteamManager Instance => _instance;

    /// <summary>
    /// SteamAPI.Init() 성공 여부
    /// </summary>
    public static bool Initialized { get; private set; }

    [Header("Editor에서 실행할지 여부")]
    [SerializeField] private bool _initializeInEditor = false; // 일단은 필요시 여기에서 true/false 직접 수정..

    /// <summary>
    /// 씬 로드 전에 자동으로 생성되는 부트스트랩.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        // 이미 인스턴스가 있으면 새로 만들지 않음
        if (_instance != null) return;

        var go = new GameObject("SteamManager");
        _instance = go.AddComponent<SteamManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        // 중복 생성 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        TryInitialize();
    }

    private void TryInitialize()
    {
#if UNITY_EDITOR
        if (!_initializeInEditor)
        {
            Debug.Log("[SteamManager] Editor에서 Steam 초기화 건너뜀 (_initializeInEditor = false).");
            return;
        }
#endif

        if (Initialized)
            return;

        try
        {
            // Steamworks.NET에서 제공하는 기본 체크 (있으면 사용)
            if (!Packsize.Test())
            {
                Debug.LogError("[SteamManager] Packsize Test failed. (Steamworks.NET 설정 문제)");
                return;
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[SteamManager] DllCheck Test failed. (steam_api.dll 문제)");
                return;
            }

            Initialized = SteamAPI.Init();

            if (!Initialized)
            {
                Debug.LogError("[SteamManager] SteamAPI.Init() 실패.");
            }
            else
            {
                Debug.Log("[SteamManager] SteamAPI.Init() 성공.");
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[SteamManager] steam_api DLL을 찾지 못했습니다. " + e);
            Initialized = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SteamManager] Steam 초기화 중 예외 발생: " + e);
            Initialized = false;
        }
    }

    private void Update()
    {
        if (!Initialized)
            return;

        try
        {
            SteamAPI.RunCallbacks();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SteamManager] SteamAPI.RunCallbacks() 중 예외: " + e);
        }


        // Lock Back Test: F9 키를 누르면 해당 스팀 도전과제를 잠금 상태로 되돌림
        //if (Input.GetKeyDown(KeyCode.F9))
        //{
        //    SteamAchievementDebug.ResetAchievement("DONE_TUTORIALS");
        //    SteamAchievementDebug.ResetAchievement("HIGHEST_STAGE_1");
        //    SteamAchievementDebug.ResetAchievement("KILL_BOSS_1");
        //    SteamAchievementDebug.ResetAchievement("UNLOCK_PERK_1");
        //}

    }

    private void OnApplicationQuit()
    {
        if (!Initialized)
            return;

        try
        {
            SteamAPI.Shutdown();
            Initialized = false;
            Debug.Log("[SteamManager] SteamAPI.Shutdown()");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SteamManager] SteamAPI.Shutdown() 중 예외: " + e);
        }
    }
}

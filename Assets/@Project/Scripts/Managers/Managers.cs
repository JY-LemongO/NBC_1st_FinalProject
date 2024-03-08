using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    #region # Core
    ResourceManager _resouceManager = new ResourceManager();
    UIManager _uiManager = new UIManager();

    public static ResourceManager RM => Instance?._resouceManager;
    public static UIManager UI => Instance?._uiManager;
    #endregion

    #region # Contents



    #endregion



    private static void Init()
    {
        if(s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if(go == null)
            {
                go = new GameObject("@Managers");
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();
        }
    }

    /// <summary>
    /// 씬이 넘어갈 때 각 매니저의 초기화를 실행
    /// </summary>
    public void Clear()
    {
        // To-Do 초기화가 필요한 매니저들의 각 클래스에 Clear 함수를 이곳에서 호출.
    }
}

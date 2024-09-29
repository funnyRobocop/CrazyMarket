using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SDKWrapper : MonoBehaviour
{
    
    public static Action GetDataEvent;
    public static Action<int> RewardVideoEvent;
    public static void RewVideoShow(int id) => Instance.RewardedShow(id);

    
    public static SDKWrapper Instance;
    public static SavedData savesData = new SavedData();

    public static bool SDKEnabled;
    public static bool isAuth;

    public static void SaveProgress()
    { 
    }

    public static void NewLeaderboardScores(string nameLB, int score)
    {
    }

    public static void ResetSaveProgress()
    {
    }
    
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    public void RewardedShow(int id)
    {
    }
}

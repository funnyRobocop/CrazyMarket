using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private int maxLives = 10;
    [SerializeField] private float liveRegenTimeInMinutes = 30f;

    public int CurrentLives => SDKWrapper.savesData.currentLives;
    public int Stars => SDKWrapper.savesData.stars;
    public int Coins => SDKWrapper.savesData.coins;
    public string CurrentBackgroundId => SDKWrapper.savesData.currentBackgroundId;

    private bool isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("ResourceManager");
            Instance = go.AddComponent<ResourceManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SDKWrapper.GetDataEvent += OnDataLoaded;
        if (SDKWrapper.SDKEnabled)
        {
            OnDataLoaded();
        }
    }

    private void OnDestroy()
    {
        SDKWrapper.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        InitializeResources();
        isInitialized = true;
        InvokeRepeating(nameof(RegenerateLive), 60f, 60f);
    }

    private void InitializeResources()
    {
        if (SDKWrapper.savesData.isFirstSession)
        {
            SDKWrapper.savesData.currentLives = maxLives;
            SDKWrapper.savesData.maxLives = maxLives;
            SDKWrapper.savesData.stars = 0;
            SDKWrapper.savesData.coins = 0;
            SDKWrapper.savesData.currentBackgroundId = "0";
            SDKWrapper.savesData.lastLiveRegenTimeTicks = DateTime.Now.Ticks;
            SDKWrapper.savesData.isFirstSession = false;
            SDKWrapper.SaveProgress();
        }
    }

    public void SetCurrentBackground(string backgroundId)
    {
        if (!isInitialized) return;
        SDKWrapper.savesData.currentBackgroundId = backgroundId;
        SDKWrapper.SaveProgress();
    }

    public void SpendLive()
    {
        if (!isInitialized) return;
        if (SDKWrapper.savesData.currentLives > 0)
        {
            SDKWrapper.savesData.currentLives--;
            SDKWrapper.SaveProgress();
        }
    }

    public void AddStars(int amount)
    {
        if (!isInitialized) return;
        SDKWrapper.savesData.stars += amount;
        SDKWrapper.SaveProgress();
        if (SDKWrapper.isAuth)
{
        SDKWrapper.NewLeaderboardScores("StarsLeaderboard", SDKWrapper.savesData.stars);
}
    }

    public void AddCoins(int amount)
    {
        if (!isInitialized) return;
        SDKWrapper.savesData.coins += amount;
        SDKWrapper.SaveProgress();
    }

    public bool SpendCoins(int amount)
    {
        if (!isInitialized) return false;
        if (SDKWrapper.savesData.coins >= amount)
        {
            SDKWrapper.savesData.coins -= amount;
            SDKWrapper.SaveProgress();
            return true;
        }
        return false;
    }

    private void RegenerateLive()
    {
        if (!isInitialized) return;
        if (SDKWrapper.savesData.currentLives < SDKWrapper.savesData.maxLives)
        {
            DateTime lastRegenTime = new DateTime(SDKWrapper.savesData.lastLiveRegenTimeTicks);
            TimeSpan timeSinceLastRegen = DateTime.Now - lastRegenTime;
            int livesToRegen = Mathf.Min(SDKWrapper.savesData.maxLives - SDKWrapper.savesData.currentLives,
                                         Mathf.FloorToInt((float)timeSinceLastRegen.TotalMinutes / liveRegenTimeInMinutes));
            if (livesToRegen > 0)
            {
                SDKWrapper.savesData.currentLives += livesToRegen;
                SDKWrapper.savesData.lastLiveRegenTimeTicks = DateTime.Now.Ticks;
                SDKWrapper.SaveProgress();
            }
        }
    }

    public void ResetAllProgress()
    {
        if (!isInitialized) return;
        SDKWrapper.ResetSaveProgress();
        InitializeResources();
        Debug.Log("Весь прогресс был сброшен.");
    }

    public void SetCurrentLevel(int level)
    {
        if (!isInitialized) return;
        SDKWrapper.savesData.currentLevel = level;
        SDKWrapper.SaveProgress();
    }

    public int GetCurrentLevel()
    {
        return isInitialized ? SDKWrapper.savesData.currentLevel : 1;
    }
}
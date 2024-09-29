using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private int maxLives = 10;
    [SerializeField] private float liveRegenTimeInMinutes = 30f;

    public int CurrentLives => YandexGame.savesData.currentLives;
    public int Stars => YandexGame.savesData.stars;
    public int Coins => YandexGame.savesData.coins;
    public string CurrentBackgroundId => YandexGame.savesData.currentBackgroundId;

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
        YandexGame.GetDataEvent += OnDataLoaded;
        if (YandexGame.SDKEnabled)
        {
            OnDataLoaded();
        }
    }

    private void OnDestroy()
    {
        YandexGame.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        InitializeResources();
        isInitialized = true;
        InvokeRepeating(nameof(RegenerateLive), 60f, 60f);
    }

    private void InitializeResources()
    {
        if (YandexGame.savesData.isFirstSession)
        {
            YandexGame.savesData.currentLives = maxLives;
            YandexGame.savesData.maxLives = maxLives;
            YandexGame.savesData.stars = 0;
            YandexGame.savesData.coins = 0;
            YandexGame.savesData.currentBackgroundId = "0";
            YandexGame.savesData.lastLiveRegenTimeTicks = DateTime.Now.Ticks;
            YandexGame.savesData.isFirstSession = false;
            YandexGame.SaveProgress();
        }
    }

    public void SetCurrentBackground(string backgroundId)
    {
        if (!isInitialized) return;
        YandexGame.savesData.currentBackgroundId = backgroundId;
        YandexGame.SaveProgress();
    }

    public void SpendLive()
    {
        if (!isInitialized) return;
        if (YandexGame.savesData.currentLives > 0)
        {
            YandexGame.savesData.currentLives--;
            YandexGame.SaveProgress();
        }
    }

    public void AddStars(int amount)
    {
        if (!isInitialized) return;
        YandexGame.savesData.stars += amount;
        YandexGame.SaveProgress();
        if (YandexGame.auth)
{
        YandexGame.NewLeaderboardScores("StarsLeaderboard", YandexGame.savesData.stars);
}
    }

    public void AddCoins(int amount)
    {
        if (!isInitialized) return;
        YandexGame.savesData.coins += amount;
        YandexGame.SaveProgress();
    }

    public bool SpendCoins(int amount)
    {
        if (!isInitialized) return false;
        if (YandexGame.savesData.coins >= amount)
        {
            YandexGame.savesData.coins -= amount;
            YandexGame.SaveProgress();
            return true;
        }
        return false;
    }

    private void RegenerateLive()
    {
        if (!isInitialized) return;
        if (YandexGame.savesData.currentLives < YandexGame.savesData.maxLives)
        {
            DateTime lastRegenTime = new DateTime(YandexGame.savesData.lastLiveRegenTimeTicks);
            TimeSpan timeSinceLastRegen = DateTime.Now - lastRegenTime;
            int livesToRegen = Mathf.Min(YandexGame.savesData.maxLives - YandexGame.savesData.currentLives,
                                         Mathf.FloorToInt((float)timeSinceLastRegen.TotalMinutes / liveRegenTimeInMinutes));
            if (livesToRegen > 0)
            {
                YandexGame.savesData.currentLives += livesToRegen;
                YandexGame.savesData.lastLiveRegenTimeTicks = DateTime.Now.Ticks;
                YandexGame.SaveProgress();
            }
        }
    }

    public void ResetAllProgress()
    {
        if (!isInitialized) return;
        YandexGame.ResetSaveProgress();
        InitializeResources();
        Debug.Log("Весь прогресс был сброшен.");
    }

    public void SetCurrentLevel(int level)
    {
        if (!isInitialized) return;
        YandexGame.savesData.currentLevel = level;
        YandexGame.SaveProgress();
    }

    public int GetCurrentLevel()
    {
        return isInitialized ? YandexGame.savesData.currentLevel : 1;
    }
}
using UnityEngine;
using System;
using System.Collections.Generic;

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem Instance { get; private set; }

    [SerializeField] private QuestDatabase questDatabase;
    private const int MAX_DAILY_QUESTS = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadQuestProgress();
            CheckAndRefreshDailyQuests();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SDKWrapper.GetDataEvent += OnDataLoaded;
    }

    private void OnDestroy()
    {
        SDKWrapper.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        LoadQuestProgress();
        CheckAndRefreshDailyQuests();
    }

    public void CheckAndRefreshDailyQuests()
    {
        DateTime currentTime = DateTime.Now;
        DateTime lastRefreshTime = string.IsNullOrEmpty(SDKWrapper.savesData.lastQuestRefreshTime) 
            ? DateTime.MinValue 
            : DateTime.Parse(SDKWrapper.savesData.lastQuestRefreshTime);

        if (currentTime >= lastRefreshTime.AddHours(24) || SDKWrapper.savesData.activeQuestIds.Length == 0)
        {
            RefreshDailyQuests();
        }
    }

    private void RefreshDailyQuests()
    {
        SDKWrapper.savesData.activeQuestIds = new int[MAX_DAILY_QUESTS];
        SDKWrapper.savesData.questProgress = new int[MAX_DAILY_QUESTS];
        SDKWrapper.savesData.claimedRewards = new bool[MAX_DAILY_QUESTS];

        List<QuestData> availableQuests = new List<QuestData>(questDatabase.quests);
        for (int i = 0; i < MAX_DAILY_QUESTS && availableQuests.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableQuests.Count);
            SDKWrapper.savesData.activeQuestIds[i] = availableQuests[randomIndex].id;
            SDKWrapper.savesData.questProgress[i] = 0;
            SDKWrapper.savesData.claimedRewards[i] = false;
            availableQuests.RemoveAt(randomIndex);
        }

        SDKWrapper.savesData.lastQuestRefreshTime = DateTime.Now.ToString();
        SDKWrapper.SaveProgress();
        Debug.Log("Daily quests refreshed. Next refresh at: " + DateTime.Now.AddHours(24));
    }

    public void UpdateQuestProgress(QuestType type, int progress = 1)
    {
        for (int i = 0; i < SDKWrapper.savesData.activeQuestIds.Length; i++)
        {
            int questId = SDKWrapper.savesData.activeQuestIds[i];
            QuestData quest = GetQuestById(questId);
            if (quest != null && quest.type == type && !SDKWrapper.savesData.claimedRewards[i])
            {
                SDKWrapper.savesData.questProgress[i] = Mathf.Min(SDKWrapper.savesData.questProgress[i] + progress, quest.requiredProgress);
                SDKWrapper.SaveProgress();
                Debug.Log($"Updated quest {questId} progress: {SDKWrapper.savesData.questProgress[i]}/{quest.requiredProgress}");
            }
        }
    }

    public int GetQuestProgress(int questId)
    {
        for (int i = 0; i < SDKWrapper.savesData.activeQuestIds.Length; i++)
        {
            if (SDKWrapper.savesData.activeQuestIds[i] == questId)
            {
                return SDKWrapper.savesData.questProgress[i];
            }
        }
        return 0;
    }

    public List<QuestData> GetActiveQuests()
    {
        CheckAndRefreshDailyQuests();
        List<QuestData> activeQuests = new List<QuestData>();
        for (int i = 0; i < SDKWrapper.savesData.activeQuestIds.Length; i++)
        {
            QuestData quest = GetQuestById(SDKWrapper.savesData.activeQuestIds[i]);
            if (quest != null)
            {
                activeQuests.Add(quest);
            }
        }
        return activeQuests;
    }

    public bool IsQuestCompleted(int questId)
    {
        for (int i = 0; i < SDKWrapper.savesData.activeQuestIds.Length; i++)
        {
            if (SDKWrapper.savesData.activeQuestIds[i] == questId)
            {
                QuestData quest = GetQuestById(questId);
                return quest != null && SDKWrapper.savesData.questProgress[i] >= quest.requiredProgress;
            }
        }
        return false;
    }

    public bool IsRewardClaimed(int questId)
    {
        for (int i = 0; i < SDKWrapper.savesData.activeQuestIds.Length; i++)
        {
            if (SDKWrapper.savesData.activeQuestIds[i] == questId)
            {
                return SDKWrapper.savesData.claimedRewards[i];
            }
        }
        return false;
    }

    public void ClaimReward(int questId)
    {
        for (int i = 0; i < SDKWrapper.savesData.activeQuestIds.Length; i++)
        {
            if (SDKWrapper.savesData.activeQuestIds[i] == questId)
            {
                QuestData quest = GetQuestById(questId);
                if (quest != null && IsQuestCompleted(questId) && !SDKWrapper.savesData.claimedRewards[i])
                {
                    ResourceManager.Instance.AddCoins(quest.reward);
                    SDKWrapper.savesData.claimedRewards[i] = true;
                    SDKWrapper.SaveProgress();
                    Debug.Log($"Claimed reward for quest {questId}: {quest.reward} coins");
                }
                break;
            }
        }
    }

    public QuestData GetQuestById(int id)
    {
        return questDatabase.GetQuestById(id);
    }

    public TimeSpan GetTimeUntilNextRefresh()
    {
        DateTime now = DateTime.Now;
        DateTime lastRefreshTime = string.IsNullOrEmpty(SDKWrapper.savesData.lastQuestRefreshTime) 
            ? DateTime.MinValue 
            : DateTime.Parse(SDKWrapper.savesData.lastQuestRefreshTime);
        DateTime nextRefresh = lastRefreshTime.AddHours(24);

        if (nextRefresh <= now)
        {
            return TimeSpan.Zero;
        }

        return nextRefresh - now;
    }

    private void LoadQuestProgress()
    {
        // Данные уже загружены через SDKWrapper.savesData
        // Здесь можно добавить дополнительную логику, если необходимо
    }

    public void ResetAllQuests()
    {
        RefreshDailyQuests();
    }

    // Дополнительные методы для отладки или управления квестами
    public void DebugCompleteAllQuests()
    {
        for (int i = 0; i < SDKWrapper.savesData.activeQuestIds.Length; i++)
        {
            QuestData quest = GetQuestById(SDKWrapper.savesData.activeQuestIds[i]);
            if (quest != null)
            {
                SDKWrapper.savesData.questProgress[i] = quest.requiredProgress;
            }
        }
        SDKWrapper.SaveProgress();
        Debug.Log("All active quests completed (debug)");
    }

    public void DebugResetQuestProgress()
    {
        for (int i = 0; i < SDKWrapper.savesData.questProgress.Length; i++)
        {
            SDKWrapper.savesData.questProgress[i] = 0;
            SDKWrapper.savesData.claimedRewards[i] = false;
        }
        SDKWrapper.SaveProgress();
        Debug.Log("All quest progress reset (debug)");
    }
}
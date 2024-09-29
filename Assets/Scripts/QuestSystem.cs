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
        YandexGame.GetDataEvent += OnDataLoaded;
    }

    private void OnDestroy()
    {
        YandexGame.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        LoadQuestProgress();
        CheckAndRefreshDailyQuests();
    }

    public void CheckAndRefreshDailyQuests()
    {
        DateTime currentTime = DateTime.Now;
        DateTime lastRefreshTime = string.IsNullOrEmpty(YandexGame.savesData.lastQuestRefreshTime) 
            ? DateTime.MinValue 
            : DateTime.Parse(YandexGame.savesData.lastQuestRefreshTime);

        if (currentTime >= lastRefreshTime.AddHours(24) || YandexGame.savesData.activeQuestIds.Length == 0)
        {
            RefreshDailyQuests();
        }
    }

    private void RefreshDailyQuests()
    {
        YandexGame.savesData.activeQuestIds = new int[MAX_DAILY_QUESTS];
        YandexGame.savesData.questProgress = new int[MAX_DAILY_QUESTS];
        YandexGame.savesData.claimedRewards = new bool[MAX_DAILY_QUESTS];

        List<QuestData> availableQuests = new List<QuestData>(questDatabase.quests);
        for (int i = 0; i < MAX_DAILY_QUESTS && availableQuests.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableQuests.Count);
            YandexGame.savesData.activeQuestIds[i] = availableQuests[randomIndex].id;
            YandexGame.savesData.questProgress[i] = 0;
            YandexGame.savesData.claimedRewards[i] = false;
            availableQuests.RemoveAt(randomIndex);
        }

        YandexGame.savesData.lastQuestRefreshTime = DateTime.Now.ToString();
        YandexGame.SaveProgress();
        Debug.Log("Daily quests refreshed. Next refresh at: " + DateTime.Now.AddHours(24));
    }

    public void UpdateQuestProgress(QuestType type, int progress = 1)
    {
        for (int i = 0; i < YandexGame.savesData.activeQuestIds.Length; i++)
        {
            int questId = YandexGame.savesData.activeQuestIds[i];
            QuestData quest = GetQuestById(questId);
            if (quest != null && quest.type == type && !YandexGame.savesData.claimedRewards[i])
            {
                YandexGame.savesData.questProgress[i] = Mathf.Min(YandexGame.savesData.questProgress[i] + progress, quest.requiredProgress);
                YandexGame.SaveProgress();
                Debug.Log($"Updated quest {questId} progress: {YandexGame.savesData.questProgress[i]}/{quest.requiredProgress}");
            }
        }
    }

    public int GetQuestProgress(int questId)
    {
        for (int i = 0; i < YandexGame.savesData.activeQuestIds.Length; i++)
        {
            if (YandexGame.savesData.activeQuestIds[i] == questId)
            {
                return YandexGame.savesData.questProgress[i];
            }
        }
        return 0;
    }

    public List<QuestData> GetActiveQuests()
    {
        CheckAndRefreshDailyQuests();
        List<QuestData> activeQuests = new List<QuestData>();
        for (int i = 0; i < YandexGame.savesData.activeQuestIds.Length; i++)
        {
            QuestData quest = GetQuestById(YandexGame.savesData.activeQuestIds[i]);
            if (quest != null)
            {
                activeQuests.Add(quest);
            }
        }
        return activeQuests;
    }

    public bool IsQuestCompleted(int questId)
    {
        for (int i = 0; i < YandexGame.savesData.activeQuestIds.Length; i++)
        {
            if (YandexGame.savesData.activeQuestIds[i] == questId)
            {
                QuestData quest = GetQuestById(questId);
                return quest != null && YandexGame.savesData.questProgress[i] >= quest.requiredProgress;
            }
        }
        return false;
    }

    public bool IsRewardClaimed(int questId)
    {
        for (int i = 0; i < YandexGame.savesData.activeQuestIds.Length; i++)
        {
            if (YandexGame.savesData.activeQuestIds[i] == questId)
            {
                return YandexGame.savesData.claimedRewards[i];
            }
        }
        return false;
    }

    public void ClaimReward(int questId)
    {
        for (int i = 0; i < YandexGame.savesData.activeQuestIds.Length; i++)
        {
            if (YandexGame.savesData.activeQuestIds[i] == questId)
            {
                QuestData quest = GetQuestById(questId);
                if (quest != null && IsQuestCompleted(questId) && !YandexGame.savesData.claimedRewards[i])
                {
                    ResourceManager.Instance.AddCoins(quest.reward);
                    YandexGame.savesData.claimedRewards[i] = true;
                    YandexGame.SaveProgress();
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
        DateTime lastRefreshTime = string.IsNullOrEmpty(YandexGame.savesData.lastQuestRefreshTime) 
            ? DateTime.MinValue 
            : DateTime.Parse(YandexGame.savesData.lastQuestRefreshTime);
        DateTime nextRefresh = lastRefreshTime.AddHours(24);

        if (nextRefresh <= now)
        {
            return TimeSpan.Zero;
        }

        return nextRefresh - now;
    }

    private void LoadQuestProgress()
    {
        // Данные уже загружены через YandexGame.savesData
        // Здесь можно добавить дополнительную логику, если необходимо
    }

    public void ResetAllQuests()
    {
        RefreshDailyQuests();
    }

    // Дополнительные методы для отладки или управления квестами
    public void DebugCompleteAllQuests()
    {
        for (int i = 0; i < YandexGame.savesData.activeQuestIds.Length; i++)
        {
            QuestData quest = GetQuestById(YandexGame.savesData.activeQuestIds[i]);
            if (quest != null)
            {
                YandexGame.savesData.questProgress[i] = quest.requiredProgress;
            }
        }
        YandexGame.SaveProgress();
        Debug.Log("All active quests completed (debug)");
    }

    public void DebugResetQuestProgress()
    {
        for (int i = 0; i < YandexGame.savesData.questProgress.Length; i++)
        {
            YandexGame.savesData.questProgress[i] = 0;
            YandexGame.savesData.claimedRewards[i] = false;
        }
        YandexGame.SaveProgress();
        Debug.Log("All quest progress reset (debug)");
    }
}
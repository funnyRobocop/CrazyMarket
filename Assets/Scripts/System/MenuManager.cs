using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [SerializeField] private Button playButton;
    [SerializeField] private Button questsButton;
    [SerializeField] private Text levelText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text starsText;
    [SerializeField] private Text coinsText;
    [SerializeField] private GameObject questsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Red Dots")]
    [SerializeField] private Image PiggyRedDot;
    [SerializeField] private Image GiftRedDot;
    [SerializeField] private Image QuestRedDot;
    [SerializeField] private Image RewardsRedDot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Debug.Log(Application.systemLanguage);
    }

    private void Start()
    {
        SDKWrapper.GetDataEvent += OnDataLoaded;
        SetupButtonListeners();
        InitializeGame();
    }

    private void OnDestroy()
    {
        SDKWrapper.GetDataEvent -= OnDataLoaded;
    }

    private void SetupButtonListeners()
    {
        if (playButton != null) playButton.onClick.AddListener(StartLevel);
        if (questsButton != null) questsButton.onClick.AddListener(OpenQuestsPanel);
    }

    private void OnDataLoaded()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.CheckAndRefreshDailyQuests();
            QuestSystem.Instance.UpdateQuestProgress(QuestType.GameEntry);
        }
        else
        {
            Debug.LogError("QuestSystem instance not found in MenuManager!");
        }

        RefreshUI();
    }

private void StartLevel()
{
    if (SDKWrapper.savesData.currentLevel <= 6)
    {
        // До 6 уровня включительно загружаем сцены по порядку с 2 по 7
        int sceneToLoad = SDKWrapper.savesData.currentLevel + 1; // +1 потому что уровни начинаются с 2
        SceneManager.LoadScene(sceneToLoad);
    }
    else
    {
        // После 6 уровня загружаем случайный уровень
        int rnd = Random.Range(2, 12); // Изменено с 11 на 12
        SceneManager.LoadScene(rnd);
    }
}

    private void OpenQuestsPanel()
    {
        if (questsPanel != null)
        {
            questsPanel.SetActive(true);

            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.CheckAndRefreshDailyQuests();
            }

            QuestUIManager questUIManager = questsPanel.GetComponent<QuestUIManager>();
            if (questUIManager != null)
            {
                questUIManager.RefreshQuestDisplay();
            }
            else
            {
                Debug.LogError("QuestUIManager component not found on questsPanel!");
            }
        }
        else
        {
            Debug.LogError("Quests panel is not assigned in MenuManager!");
        }
    }

    public void ShowSettingsPanel()
    {
        settingsPanel.SetActive(true);
       
    }
     public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
        
    }

    public void RefreshUI()
    {
        UpdateUI();
        CheckRedDots();
    }

    private void UpdateUI()
    {
        if (levelText != null)
        {
            levelText.text = "Играть уровень " + SDKWrapper.savesData.currentLevel;
        }

        if (livesText != null)
        {
            livesText.text = "" + SDKWrapper.savesData.currentLives;
        }

        if (starsText != null)
        {
            starsText.text = "" + SDKWrapper.savesData.stars;
        }

        if (coinsText != null)
        {
            coinsText.text = "" + SDKWrapper.savesData.coins;
        }
    }

    public void CheckRedDots()
    {
        if (PiggyBankManager.Instance != null && PiggyRedDot != null)
        {
            PiggyRedDot.enabled = PiggyBankManager.Instance.CanCollectCoins();
        }

        if (DailyGiftManager.Instance != null && GiftRedDot != null)
        {
            GiftRedDot.enabled = DailyGiftManager.Instance.CanClaimGift();
        }

        if (QuestSystem.Instance != null && QuestRedDot != null)
        {
            bool hasCompletedUnclaimed = false;
            List<QuestData> activeQuests = QuestSystem.Instance.GetActiveQuests();
            foreach (QuestData quest in activeQuests)
            {
                if (QuestSystem.Instance.IsQuestCompleted(quest.id) && !QuestSystem.Instance.IsRewardClaimed(quest.id))
                {
                    hasCompletedUnclaimed = true;
                    break;
                }
            }
            QuestRedDot.enabled = hasCompletedUnclaimed;
        }

        if (RewardsManager.Instance != null && RewardsRedDot != null)
        {
            RewardsRedDot.enabled = RewardsManager.Instance.HasUnclaimedRewards();
        }
        else
        {
            Debug.LogWarning("RewardsManager.Instance is null in MenuManager.CheckRedDots");
        }
    }

    public void GoToPiggy()
    {
        SceneManager.LoadScene("PiggyBox");
    }

    // Метод для обновления UI после возвращения из других сцен
    public void OnReturnToMenu()
    {
        RefreshUI();
    }

    // Метод для обработки изменений в игровых данных
    public void OnGameDataChanged()
    {
        RefreshUI();
    }

    // Вспомогательный метод для загрузки сцены
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Метод для выхода из игры (если требуется)
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
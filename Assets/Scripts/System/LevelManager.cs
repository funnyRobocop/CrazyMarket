using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using YG;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Button winButton;
    [SerializeField] private Button loseRestartButton;
    [SerializeField] private Button loseMenuButton;
    [SerializeField] private Text levelText;
    [SerializeField] private Text starsEarnedText;
    [SerializeField] private GameObject bottomPanel;
    [SerializeField] private GameObject SettigsPanel;


    [Header("Star Settings")]
    [SerializeField] private int maxStars = 10;
    [SerializeField] private int minStars = 1;
    [SerializeField] private float maxStarTime = 0.4f;
    [SerializeField] private float minStarTime = 0.9f;

    [Header("Background")]
    [SerializeField] private SpriteRenderer levelBackgroundRenderer;
    [SerializeField] private Sprite[] backgroundSprites;

     [Header("LevelSounds")]
     [SerializeField] private AudioClip looserSound;
     [SerializeField] private AudioClip winSound;


    private float levelStartTime;
    private float totalLevelTime;
    private float timeForMaxStars;
    private float timeForMinStars;

    private LevelTimer levelTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        YandexGame.GetDataEvent += OnDataLoaded;
        InitializeUI();
        InitializeLevelTimer();
        CalculateStarTimings();
        StartLevelTimer();
        LoadLevelBackground();

        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ResourceManager не найден! Пожалуйста, убедитесь, что он инициализирован.");
        }
        if (PiggyBankManager.Instance == null)
        {
            Debug.LogError("PiggyBankManager не найден! Пожалуйста, убедитесь, что он инициализирован.");
        }
    }

    private void OnDestroy()
    {
        YandexGame.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        UpdateLevelText();
    }

    private void InitializeUI()
    {
        if (winButton != null) winButton.onClick.AddListener(ReturnToMenu);
        if (loseRestartButton != null) loseRestartButton.onClick.AddListener(RestartLevel);
        if (loseMenuButton != null) loseMenuButton.onClick.AddListener(ReturnToMenu);

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        UpdateLevelText();
    }

    private void InitializeLevelTimer()
    {
        levelTimer = FindObjectOfType<LevelTimer>();
        if (levelTimer == null)
        {
            Debug.LogError("LevelTimer не найден на сцене!");
            return;
        }
        totalLevelTime = levelTimer.GetTotalTime();
    }

    private void CalculateStarTimings()
    {
        if (totalLevelTime <= 0)
        {
            Debug.LogError("Общее время уровня установлено некорректно!");
            return;
        }
        timeForMaxStars = totalLevelTime * maxStarTime;
        timeForMinStars = totalLevelTime * minStarTime;
    }

    private void StartLevelTimer()
    {
        levelStartTime = Time.time;
    }

    private void LoadLevelBackground()
    {
        if (levelBackgroundRenderer != null && ResourceManager.Instance != null)
        {
            string backgroundId = YandexGame.savesData.currentBackgroundId;
            Sprite backgroundSprite = GetBackgroundSpriteById(backgroundId);
            if (backgroundSprite != null)
            {
                levelBackgroundRenderer.sprite = backgroundSprite;
            }
            else
            {
                Debug.LogWarning($"Спрайт фона с ID {backgroundId} не найден. Использую первый доступный фон.");
                levelBackgroundRenderer.sprite = backgroundSprites[0];
            }
        }
        else
        {
            Debug.LogError("LevelBackgroundRenderer или ResourceManager не настроены.");
        }
    }

    private Sprite GetBackgroundSpriteById(string id)
    {
        if (int.TryParse(id, out int index) && index >= 0 && index < backgroundSprites.Length)
        {
            return backgroundSprites[index];
        }
        Debug.LogWarning($"Фон с ID {id} не найден в массиве спрайтов.");
        return null;
    }

    public void CheckWinCondition()
    {
        if (AllFruitsCleared())
        {
            StartCoroutine(ShowWinPanelDelayed());
        }
    }

    private bool AllFruitsCleared()
    {
        return GameObject.FindObjectsOfType<DraggableItem>().Length == 0;
    }

    private IEnumerator ShowWinPanelDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        if (levelTimer != null)
        {
            levelTimer.StopTimer();
            levelTimer.HideFacePanel();
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            SoundManager.Instance.PlaySound(winSound);
            HideBottonPanel();
            float levelDuration = Time.time - levelStartTime;
            int starsEarned = CalculateEarnedStars(levelDuration);

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddStars(starsEarned);
                UpdateStarsEarnedText(starsEarned);
            }
            else
            {
                Debug.LogError("ResourceManager.Instance равен null! Невозможно добавить звезды.");
            }
            if (PiggyBankManager.Instance != null)
            {
                PiggyBankManager.Instance.AddCoinsForLevelCompletion();
                Debug.Log("Монеты добавлены в копилку за завершение уровня.");
            }
            else
            {
                Debug.LogError("PiggyBankManager.Instance равен null! Невозможно добавить монеты в копилку.");
            }

            YandexGame.savesData.currentLevel++;
            YandexGame.SaveProgress();

            PauseGame();
        }
        else
        {
            Debug.LogError("Панель победы отсутствует. Невозможно показать панель победы.");
        }
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            SoundManager.Instance.PlaySound(looserSound);
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.SpendLive();
            }
            else
            {
                Debug.LogError("ResourceManager.Instance равен null! Невозможно потратить жизнь.");
            }
            PauseGame();
            HideBottonPanel();
            SoundManager.Instance.PlaySound(looserSound);
        }
        else
        {
            Debug.LogError("Панель проигрыша отсутствует. Невозможно показать панель проигрыша.");
        }
    }
    public void ShowSettingsPanel()
    {
        SettigsPanel.SetActive(true);
        PauseGame();
        levelTimer.FreezeTimer();
    }
     public void CloseSettingsPanel()
    {
        SettigsPanel.SetActive(false);
        ResumeGame();
        levelTimer.UnfreezeTimer();
    }

    private int CalculateEarnedStars(float levelDuration)
    {
        if (levelDuration <= timeForMaxStars)
        {
            return maxStars;
        }
        else if (levelDuration >= timeForMinStars)
        {
            return minStars;
        }
        else
        {
            float t = (levelDuration - timeForMaxStars) / (timeForMinStars - timeForMaxStars);
            return Mathf.RoundToInt(Mathf.Lerp(maxStars, minStars, t));
        }
    }

    private void UpdateStarsEarnedText(int stars)
    {
        if (starsEarnedText != null)
        {
            starsEarnedText.text = $"Звезды: +{stars}";
        }
        else
        {
            Debug.LogError("starsEarnedText равен null!");
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {   
        bottomPanel.SetActive(true);
        Time.timeScale = 1f;
    }

    private void RestartLevel()
    {
        ResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        ResumeGame();
        SceneManager.LoadScene("MenuScene");
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Уровень " + YandexGame.savesData.currentLevel;
        }
        else
        {
            Debug.LogError("levelText равен null!");
        }
    }

    public void HideBottonPanel()
    {
        bottomPanel.SetActive(false);
    }
}
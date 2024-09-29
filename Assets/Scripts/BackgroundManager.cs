using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance { get; private set; }

    [System.Serializable]
    public class BackgroundItem
    {
        public string id;
        public Sprite backgroundSprite;
        public int price;
        public GameObject checkmark;
        public Button buyButton;
        public Text priceText;
    }

    public List<BackgroundItem> backgroundItems = new List<BackgroundItem>();
    public Button openBackgroundPanelButton;
    public GameObject backgroundPanel;
    public GameObject confirmPurchasePanel;
    public GameObject confirmActivatePanel;
    public Text confirmPurchaseText;
    public Text playerCoinsText;
    public Button confirmPurchaseYesButton;
    public Button confirmPurchaseNoButton;
    public Button confirmActivateYesButton;
    public Button confirmActivateNoButton;
    public Button closePanelButton;
    public AudioClip claimBG;
    public AudioClip NOmoneySound;

    [Header("Not Enough Coins Panel")]
    public GameObject notEnoughCoinsPanel;
    public Text notEnoughCoinsText;

    private BackgroundItem selectedBackground;

    public event System.Action OnBackgroundsUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeBackgrounds();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        YandexGame.GetDataEvent += OnDataLoaded;
        SetupButtonListeners();
        UpdatePlayerCoins();
        LoadCurrentBackground();
        UpdateAllBackgroundUI();

        if (notEnoughCoinsPanel != null)
        {
            notEnoughCoinsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Not Enough Coins Panel is not assigned!");
        }
    }

    private void OnDestroy()
    {
        YandexGame.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        LoadCurrentBackground();
        UpdateAllBackgroundUI();
    }

    private void InitializeBackgrounds()
    {
        if (backgroundItems.Count == 0)
        {
            Debug.LogError("BackgroundManager: Список фонов пуст!");
            return;
        }

        for (int i = 0; i < backgroundItems.Count; i++)
        {
            BackgroundItem item = backgroundItems[i];
            if (item.backgroundSprite == null)
            {
                Debug.LogError($"BackgroundManager: Спрайт для фона {item.id} не назначен!");
                continue;
            }
            int index = i;
            item.buyButton.onClick.AddListener(() => OnBackgroundClicked(index));
        }

        // Разблокируем первый фон
        YandexGame.savesData.unlockedBackgrounds[0] = true;
        YandexGame.SaveProgress();
    }

    private void SetupButtonListeners()
    {
        if (openBackgroundPanelButton != null) openBackgroundPanelButton.onClick.AddListener(OpenBackgroundPanel);
        if (confirmPurchaseYesButton != null) confirmPurchaseYesButton.onClick.AddListener(OnConfirmPurchase);
        if (confirmPurchaseNoButton != null) confirmPurchaseNoButton.onClick.AddListener(OnCancelPurchase);
        if (confirmActivateYesButton != null) confirmActivateYesButton.onClick.AddListener(OnConfirmActivate);
        if (confirmActivateNoButton != null) confirmActivateNoButton.onClick.AddListener(OnCancelActivate);
        if (closePanelButton != null) closePanelButton.onClick.AddListener(CloseBackgroundPanel);
    }

    private void UpdateAllBackgroundUI()
    {
        foreach (var item in backgroundItems)
        {
            UpdateBackgroundItemUI(item);
        }
    }

    private void UpdateBackgroundItemUI(BackgroundItem item)
    {
        bool isUnlocked = IsBackgroundUnlocked(item.id);
        bool isActive = item.id == YandexGame.savesData.currentBackgroundId;

        if (item.priceText != null)
        {
            item.priceText.text = isUnlocked ? "Выбрать" : item.price.ToString();
        }
        if (item.checkmark != null) item.checkmark.SetActive(isActive);
        if (item.buyButton != null)
        {
            Text buttonText = item.buyButton.GetComponentInChildren<Text>();
            if (buttonText != null) buttonText.text = isUnlocked ? "Выбрать" : "Купить";
            item.buyButton.interactable = !isActive;
        }
    }

    private void OnBackgroundClicked(int index)
    {
        if (index < 0 || index >= backgroundItems.Count)
        {
            Debug.LogError($"BackgroundManager: Неверный индекс фона: {index}");
            return;
        }

        selectedBackground = backgroundItems[index];
        bool isUnlocked = IsBackgroundUnlocked(selectedBackground.id);

        if (isUnlocked)
        {
            ShowConfirmActivatePanel();
        }
        else
        {
            ShowConfirmPurchasePanel();
        }
    }

    private void ShowConfirmPurchasePanel()
    {
        if (confirmPurchasePanel != null)
        {
            confirmPurchasePanel.SetActive(true);
            if (confirmPurchaseText != null)
            {
                confirmPurchaseText.text = $"Купить фон за {selectedBackground.price} монет?";
            }
        }
    }

    private void ShowConfirmActivatePanel()
    {
        if (confirmActivatePanel != null)
        {
            confirmActivatePanel.SetActive(true);
        }
    }

    private void OnConfirmPurchase()
    {
        if (ResourceManager.Instance != null && ResourceManager.Instance.SpendCoins(selectedBackground.price))
        {
            int backgroundId = int.Parse(selectedBackground.id);
            YandexGame.savesData.unlockedBackgrounds[backgroundId] = true;
            YandexGame.SaveProgress();
            SetCurrentBackground(selectedBackground.id);
            UpdatePlayerCoins();
            Debug.Log($"Фон {selectedBackground.id} куплен");
            SoundManager.Instance.PlaySound(claimBG);
        }
        else
        {
            Debug.Log("Недостаточно монет");
            ShowNotEnoughCoinsPanel();
            SoundManager.Instance.PlaySound(NOmoneySound);
        }
        if (confirmPurchasePanel != null) confirmPurchasePanel.SetActive(false);
    }

    private void OnConfirmActivate()
    {
        SetCurrentBackground(selectedBackground.id);
        if (confirmActivatePanel != null) confirmActivatePanel.SetActive(false);
    }

    private void OnCancelPurchase()
    {
        if (confirmPurchasePanel != null) confirmPurchasePanel.SetActive(false);
    }

    private void OnCancelActivate()
    {
        if (confirmActivatePanel != null) confirmActivatePanel.SetActive(false);
    }

    public void OpenBackgroundPanel()
    {
        if (backgroundPanel != null) backgroundPanel.SetActive(true);
    }

    public void CloseBackgroundPanel()
    {
        if (backgroundPanel != null) backgroundPanel.SetActive(false);
    }

    private void UpdatePlayerCoins()
    {
        if (playerCoinsText != null && ResourceManager.Instance != null)
        {
            playerCoinsText.text = "" + ResourceManager.Instance.Coins;
        }
    }

    private void LoadCurrentBackground()
    {
        SetCurrentBackground(YandexGame.savesData.currentBackgroundId);
    }

    public void SetCurrentBackground(string backgroundId)
    {
        YandexGame.savesData.currentBackgroundId = backgroundId;
        YandexGame.SaveProgress();

        UpdateAllBackgroundUI();
        Debug.Log($"Текущий фон изменен на {backgroundId}");
        OnBackgroundsUpdated?.Invoke();
    }

    public string GetCurrentBackgroundId()
    {
        return YandexGame.savesData.currentBackgroundId;
    }

    public BackgroundItem GetBackgroundItem(string id)
    {
        return backgroundItems.Find(item => item.id == id);
    }

    public bool IsBackgroundUnlocked(string id)
    {
        int backgroundId = int.Parse(id);
        return YandexGame.savesData.unlockedBackgrounds[backgroundId];
    }

    public void UnlockBackgroundFromReward(string backgroundId)
    {
        int id = int.Parse(backgroundId);
        YandexGame.savesData.unlockedBackgrounds[id] = true;
        YandexGame.SaveProgress();
        var background = backgroundItems.Find(b => b.id == backgroundId);
        if (background != null)
        {
            UpdateBackgroundItemUI(background);
            Debug.Log($"Фон {backgroundId} разблокирован через награду");
        }
        OnBackgroundsUpdated?.Invoke();
    }

    public void ResetBackgrounds()
    {
        for (int i = 1; i < YandexGame.savesData.unlockedBackgrounds.Length; i++)
        {
            YandexGame.savesData.unlockedBackgrounds[i] = false;
        }

        YandexGame.savesData.currentBackgroundId = "0";
        YandexGame.SaveProgress();

        SetCurrentBackground(YandexGame.savesData.currentBackgroundId);
        UpdateAllBackgroundUI();
        Debug.Log("Все фоны сброшены, кроме начального");
        OnBackgroundsUpdated?.Invoke();
    }

    private void ShowNotEnoughCoinsPanel()
    {
        if (notEnoughCoinsPanel != null)
        {
            notEnoughCoinsPanel.SetActive(true);
            if (notEnoughCoinsText != null)
            {
                notEnoughCoinsText.text = "Недостаточно монет!";
            }
            StartCoroutine(HideNotEnoughCoinsPanelAfterDelay(2f));
        }
    }

    private IEnumerator HideNotEnoughCoinsPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notEnoughCoinsPanel != null)
        {
            notEnoughCoinsPanel.SetActive(false);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using YG;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance { get; private set; }

    [System.Serializable]
    public class AvatarItem
    {
        public string id;
        public Sprite avatarSprite;
        public int price;
        public GameObject checkmark;
        public Button buyButton;
    }

    public List<AvatarItem> avatarItems = new List<AvatarItem>();
    public Image currentAvatarImage;
    public Button currentAvatarButton;
    public GameObject avatarPanel;
    public GameObject confirmPurchasePanel;
    public GameObject confirmActivatePanel;
    public Text confirmPurchaseText;
    public Text playerCoinsText;
    public Button confirmPurchaseYesButton;
    public Button confirmPurchaseNoButton;
    public Button confirmActivateYesButton;
    public Button confirmActivateNoButton;
    public Button closePanelButton;

    [Header("Not Enough Coins Panel")]
    public GameObject notEnoughCoinsPanel;
    public Text notEnoughCoinsText;

    public AudioClip NOmoneySound;
    public AudioClip buyAvatarSound;

    private AvatarItem selectedAvatar;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAvatars();
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
        LoadCurrentAvatar();

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
        LoadCurrentAvatar();
        UpdateAllAvatarUI();
    }

    private void InitializeAvatars()
    {
        if (avatarItems.Count == 0)
        {
            Debug.LogError("AvatarManager: Список аватаров пуст!");
            return;
        }

        for (int i = 0; i < avatarItems.Count; i++)
        {
            AvatarItem item = avatarItems[i];
            if (item.avatarSprite == null)
            {
                Debug.LogError($"AvatarManager: Спрайт для аватара {item.id} не назначен!");
                continue;
            }
            int index = i;
            item.buyButton.onClick.AddListener(() => OnAvatarClicked(index));
        }

        // Разблокируем первый аватар
        YandexGame.savesData.unlockedAvatars[0] = true;
        YandexGame.SaveProgress();
    }

    private void SetupButtonListeners()
    {
        if (currentAvatarButton != null) currentAvatarButton.onClick.AddListener(OpenAvatarPanel);
        if (confirmPurchaseYesButton != null) confirmPurchaseYesButton.onClick.AddListener(OnConfirmPurchase);
        if (confirmPurchaseNoButton != null) confirmPurchaseNoButton.onClick.AddListener(OnCancelPurchase);
        if (confirmActivateYesButton != null) confirmActivateYesButton.onClick.AddListener(OnConfirmActivate);
        if (confirmActivateNoButton != null) confirmActivateNoButton.onClick.AddListener(OnCancelActivate);
        if (closePanelButton != null) closePanelButton.onClick.AddListener(CloseAvatarPanel);
    }

    private void UpdateAllAvatarUI()
    {
        foreach (var item in avatarItems)
        {
            UpdateAvatarUI(item);
        }
    }

    private void UpdateAvatarUI(AvatarItem item)
    {
        bool isUnlocked = YandexGame.savesData.unlockedAvatars[int.Parse(item.id)];
        bool isActive = item.id == YandexGame.savesData.currentAvatarId;

        if (item.checkmark != null) item.checkmark.SetActive(isActive);
        if (item.buyButton != null)
        {
            Text buttonText = item.buyButton.GetComponentInChildren<Text>();
            if (buttonText != null) buttonText.text = isUnlocked ? "Выбрать" : "Купить";
            item.buyButton.interactable = !isActive;
        }
    }

    private void OnAvatarClicked(int index)
    {
        if (index < 0 || index >= avatarItems.Count)
        {
            Debug.LogError($"AvatarManager: Неверный индекс аватара: {index}");
            return;
        }

        selectedAvatar = avatarItems[index];
        bool isUnlocked = YandexGame.savesData.unlockedAvatars[int.Parse(selectedAvatar.id)];

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
                confirmPurchaseText.text = $"Купить аватар за {selectedAvatar.price} монет?";
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
        if (ResourceManager.Instance != null && ResourceManager.Instance.SpendCoins(selectedAvatar.price))
        {
            int avatarId = int.Parse(selectedAvatar.id);
            YandexGame.savesData.unlockedAvatars[avatarId] = true;
            YandexGame.SaveProgress();
            UpdateAvatarUI(selectedAvatar);
            UpdatePlayerCoins();
            Debug.Log($"Аватар {selectedAvatar.id} куплен");
            SoundManager.Instance.PlaySound(buyAvatarSound);
        }
        else
        {
            Debug.Log("Недостаточно монет");
            ShowNotEnoughCoinsPanel();
        }
        if (confirmPurchasePanel != null) confirmPurchasePanel.SetActive(false);
    }

    private void OnConfirmActivate()
    {
        SetCurrentAvatar(selectedAvatar.id);
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

    public void OpenAvatarPanel()
    {
        if (avatarPanel != null) avatarPanel.SetActive(true);
    }

    public void CloseAvatarPanel()
    {
        if (avatarPanel != null) avatarPanel.SetActive(false);
    }

    private void UpdatePlayerCoins()
    {
        if (playerCoinsText != null && ResourceManager.Instance != null)
        {
            playerCoinsText.text = "" + ResourceManager.Instance.Coins;
        }
    }

    private void LoadCurrentAvatar()
    {
        SetCurrentAvatar(YandexGame.savesData.currentAvatarId);
    }

    public void SetCurrentAvatar(string avatarId)
    {
        YandexGame.savesData.currentAvatarId = avatarId;
        YandexGame.SaveProgress();

        var newAvatar = avatarItems.Find(a => a.id == avatarId);
        if (newAvatar != null && newAvatar.avatarSprite != null && currentAvatarImage != null)
        {
            currentAvatarImage.sprite = newAvatar.avatarSprite;
            foreach (var item in avatarItems)
            {
                UpdateAvatarUI(item);
            }
            PlayerAvatar.UpdateAllAvatars();
            Debug.Log($"Текущий аватар изменен на {avatarId}");
        }
    }

    public Sprite GetCurrentAvatarSprite()
    {
        var currentAvatar = avatarItems.Find(a => a.id == YandexGame.savesData.currentAvatarId);
        if (currentAvatar != null && currentAvatar.avatarSprite != null)
        {
            return currentAvatar.avatarSprite;
        }
        Debug.LogWarning($"AvatarManager: Не удалось найти спрайт для текущего аватара {YandexGame.savesData.currentAvatarId}");
        return null;
    }

    public void UnlockAvatarFromReward(string avatarId)
    {
        int id = int.Parse(avatarId);
        YandexGame.savesData.unlockedAvatars[id] = true;
        YandexGame.SaveProgress();
        var avatar = avatarItems.Find(a => a.id == avatarId);
        if (avatar != null)
        {
            UpdateAvatarUI(avatar);
            Debug.Log($"Аватар {avatarId} разблокирован через награду");
        }
    }

    public void ResetAvatars()
    {
        for (int i = 1; i < YandexGame.savesData.unlockedAvatars.Length; i++)
        {
            YandexGame.savesData.unlockedAvatars[i] = false;
        }

        YandexGame.savesData.currentAvatarId = "0";
        YandexGame.SaveProgress();

        SetCurrentAvatar(YandexGame.savesData.currentAvatarId);
        foreach (var item in avatarItems)
        {
            UpdateAvatarUI(item);
        }

        Debug.Log("Все аватары сброшены, кроме начального");
    }

    private void ShowNotEnoughCoinsPanel()
    {
        if (notEnoughCoinsPanel != null)
        {
            notEnoughCoinsPanel.SetActive(true);
            SoundManager.Instance.PlaySound(NOmoneySound);
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
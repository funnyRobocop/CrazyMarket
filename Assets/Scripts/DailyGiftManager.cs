using UnityEngine;
using UnityEngine.UI;
using System;

public class DailyGiftManager : MonoBehaviour
{
    public static DailyGiftManager Instance { get; private set; }

    [SerializeField] private GameObject giftPanel;
    [SerializeField] private Button giftButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openGiftPanelButton;
    [SerializeField] private Text giftStatusText;
    [SerializeField] private GameObject rewardMessagePanel;
    [SerializeField] private Text rewardMessageText;
    [SerializeField] private Animator giftButtonAnimator;
    [SerializeField] private Text coinsText;
    [SerializeField] private AudioClip claimGift;

    private const int CoinRewardAmount = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        if (giftButtonAnimator == null)
        {
            Debug.LogWarning("Gift Button Animator is not assigned in the inspector.");
        }
        else if (giftButtonAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("Gift Button Animator does not have an Animator Controller assigned.");
        }
    }

    private void Start()
    {
        SDKWrapper.GetDataEvent += OnDataLoaded;
        SetupButtons();
        UpdateGiftStatus();
        InvokeRepeating(nameof(UpdateGiftStatus), 0f, 1f);
    }

    private void OnDestroy()
    {
        SDKWrapper.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        UpdateGiftStatus();
        UpdateCoinsDisplay();
    }

    private void SetupButtons()
    {
        if (giftButton != null)
            giftButton.onClick.AddListener(ClaimDailyGift);

        if (closeButton != null)
            closeButton.onClick.AddListener(HideGiftPanel);

        if (openGiftPanelButton != null)
            openGiftPanelButton.onClick.AddListener(ShowGiftPanel);
        else
            Debug.LogWarning("Open Gift Panel Button is not assigned in the inspector.");
    }

    private void OnEnable()
    {
        UpdateCoinsDisplay();
    }

    public void ShowGiftPanel()
    {
        giftPanel.SetActive(true);
        UpdateGiftStatus();
    }

    public void HideGiftPanel()
    {
        giftPanel.SetActive(false);

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.RefreshUI();
        }
        else
        {
            Debug.LogWarning("MenuManager.Instance is null, unable to refresh UI");
        }
    }

    private void UpdateGiftStatus()
    {
        bool canClaim = CanClaimGift();

        if (giftButtonAnimator != null && giftButtonAnimator.runtimeAnimatorController != null)
        {
            if(giftPanel.activeSelf)
            {
                giftButtonAnimator.SetBool("IsReady", canClaim);
            }
        }

        if (canClaim)
        {
            giftStatusText.text = "Ваш подарок готов!";
            giftButton.interactable = true;
        }
        else
        {
            TimeSpan timeUntilNextGift = GetTimeUntilNextGift();
            giftStatusText.text = $"Следующий подарок через: {timeUntilNextGift.Hours:D2}:{timeUntilNextGift.Minutes:D2}:{timeUntilNextGift.Seconds:D2}";
            giftButton.interactable = false;
        }

        if (openGiftPanelButton != null)
        {
            openGiftPanelButton.interactable = true;
        }
    }

    private void ClaimDailyGift()
    {
        if (!CanClaimGift()) return;

        ResourceManager.Instance.AddCoins(CoinRewardAmount);
        ShowRewardMessage($"Вы получили {CoinRewardAmount} монет!");

        SDKWrapper.savesData.lastGiftTimestamp = DateTime.Now.ToString();
        SDKWrapper.SaveProgress();
        SoundManager.Instance.PlaySound(claimGift);

        UpdateGiftStatus();
        UpdateCoinsDisplay();
    }

    public bool CanClaimGift()
    {
        if (string.IsNullOrEmpty(SDKWrapper.savesData.lastGiftTimestamp)) return true;

        DateTime lastGiftTime = DateTime.Parse(SDKWrapper.savesData.lastGiftTimestamp);
        return (DateTime.Now - lastGiftTime).TotalHours >= 24;
    }

    private TimeSpan GetTimeUntilNextGift()
    {
        if (string.IsNullOrEmpty(SDKWrapper.savesData.lastGiftTimestamp)) return TimeSpan.Zero;

        DateTime lastGiftTime = DateTime.Parse(SDKWrapper.savesData.lastGiftTimestamp);
        DateTime nextGiftTime = lastGiftTime.AddHours(24);
        TimeSpan timeUntilNext = nextGiftTime - DateTime.Now;
        return timeUntilNext > TimeSpan.Zero ? timeUntilNext : TimeSpan.Zero;
    }

    private void ShowRewardMessage(string message)
    {
        rewardMessageText.text = message;
        rewardMessagePanel.SetActive(true);
        Invoke(nameof(HideRewardMessage), 6f);
    }

    private void HideRewardMessage()
    {
        rewardMessagePanel.SetActive(false);
    }

    public void ResetDailyGift()
    {
        SDKWrapper.savesData.lastGiftTimestamp = null;
        SDKWrapper.SaveProgress();
        UpdateGiftStatus();
    }

    private void UpdateCoinsDisplay()
    {
        if (coinsText != null && ResourceManager.Instance != null)
        {
            coinsText.text = "" + ResourceManager.Instance.Coins.ToString();
        }
    }
}
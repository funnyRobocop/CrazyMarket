using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoosterShop : MonoBehaviour
{
    [System.Serializable]
    public class BoosterUIItem
    {
        public BoosterData.BoosterType type;
        public Text nameText;
        public Text priceText;
        public Text quantityText;
        public Button buyButton;
    }

    public List<BoosterUIItem> boosterItems;
    public GameObject shopPanel;
    public GameObject confirmPurchasePanel;
    public Text confirmPurchaseText;
    public Button confirmPurchaseYesButton;
    public Button confirmPurchaseNoButton;
    public Button openShopButton;
    public Button closeShopButton;
    public Text playerCoinsText;
    public AudioClip buyBooster;

    [Header("Not Enough Coins Panel")]
    public GameObject notEnoughCoinsPanel;
    public Text notEnoughCoinsText;
    public AudioClip NOmoneySound;

    private BoosterData.BoosterType selectedBoosterType;

    private void Start()
    {
        YandexGame.GetDataEvent += OnDataLoaded;
        SetupShopUI();
        UpdatePlayerCoinsDisplay();

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
        UpdateAllBoostersUI();
        UpdatePlayerCoinsDisplay();
    }

    private void SetupShopUI()
    {
        foreach (var item in boosterItems)
        {
            item.buyButton.onClick.AddListener(() => OnBoosterSelected(item.type));
            UpdateBoosterUI(item);
        }

        confirmPurchaseYesButton.onClick.AddListener(OnConfirmPurchase);
        confirmPurchaseNoButton.onClick.AddListener(OnCancelPurchase);

        if (openShopButton != null)
            openShopButton.onClick.AddListener(OpenShop);
        else
            Debug.LogWarning("Open Shop Button is not assigned in the inspector!");

        if (closeShopButton != null)
            closeShopButton.onClick.AddListener(CloseShop);
        else
            Debug.LogWarning("Close Shop Button is not assigned in the inspector!");

        shopPanel.SetActive(false);
    }

    private void OnBoosterSelected(BoosterData.BoosterType type)
    {
        selectedBoosterType = type;
        ShowConfirmPurchasePanel();
    }

    private void ShowConfirmPurchasePanel()
    {
        var boosterData = BoosterData.Instance.GetBoosterData(selectedBoosterType);
        confirmPurchaseText.text = $"Купить {boosterData.name} за {boosterData.price} монет?";
        confirmPurchasePanel.SetActive(true);
    }

    private void OnConfirmPurchase()
    {
        var boosterData = BoosterData.Instance.GetBoosterData(selectedBoosterType);
        if (ResourceManager.Instance.SpendCoins(boosterData.price))
        {
            BoosterData.Instance.AddBooster(selectedBoosterType, 1);
            UpdateBoosterUI(boosterItems.Find(item => item.type == selectedBoosterType));
            UpdatePlayerCoinsDisplay();
            Debug.Log($"Бустер {boosterData.name} куплен");
        }
        else
        {
            Debug.Log("Недостаточно монет");
            ShowNotEnoughCoinsPanel();
        }
        confirmPurchasePanel.SetActive(false);
        SoundManager.Instance.PlaySound(buyBooster);
    }

    private void OnCancelPurchase()
    {
        confirmPurchasePanel.SetActive(false);
    }

    private void UpdateBoosterUI(BoosterUIItem item)
    {
        var boosterData = BoosterData.Instance.GetBoosterData(item.type);
        item.nameText.text = boosterData.name;
        item.priceText.text = boosterData.price.ToString();
        item.quantityText.text = BoosterData.Instance.GetBoosterQuantity(item.type).ToString();
    }

    private void UpdatePlayerCoinsDisplay()
    {
        if (playerCoinsText != null)
        {
            playerCoinsText.text = $"{YandexGame.savesData.coins}";
        }
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        UpdatePlayerCoinsDisplay();
        UpdateAllBoostersUI();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    private void UpdateAllBoostersUI()
    {
        foreach (var item in boosterItems)
        {
            UpdateBoosterUI(item);
        }
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
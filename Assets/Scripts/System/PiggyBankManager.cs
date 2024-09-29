using UnityEngine;
using System;
using YG;

public class PiggyBankManager : MonoBehaviour
{
    public static PiggyBankManager Instance { get; private set; }

    [SerializeField] private int coinsToCollect = 10;
    [SerializeField] private int coinsPerLevel = 2;

    public event Action OnCoinsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        // Данные уже загружены в YandexGame.savesData
        OnCoinsChanged?.Invoke();
    }

    public void AddCoinsForLevelCompletion()
    {
        YandexGame.savesData.piggyBankCoins += coinsPerLevel;
        YandexGame.SaveProgress();
        OnCoinsChanged?.Invoke();
        Debug.Log($"Coins added to piggy bank. Current coins: {YandexGame.savesData.piggyBankCoins}");
    }

    public bool CanCollectCoins()
    {
        return YandexGame.savesData.piggyBankCoins >= coinsToCollect;
    }

   public void CollectCoins()
    {
    if (CanCollectCoins())
    {
        // Вызываем рекламу перед сбором монет
        YandexGame.RewVideoShow(0);
    }
    else
    {
        Debug.Log("Недостаточно монет для сбора");
    }
    }

    private void OnRewardedAdFinished(int id)
    {
    if (id == 0) // Проверяем, что это наша реклама (id = 0)
    {
        int coinsToAdd = YandexGame.savesData.piggyBankCoins;
        YandexGame.savesData.coins += coinsToAdd;
        YandexGame.savesData.piggyBankCoins = 0;
        YandexGame.SaveProgress();
        OnCoinsChanged?.Invoke();
        Debug.Log($"Collected {coinsToAdd} coins from piggy bank after watching ad.");
    }
    }

private void OnEnable()
{
    YandexGame.RewardVideoEvent += OnRewardedAdFinished;
}

private void OnDisable()
{
    YandexGame.RewardVideoEvent -= OnRewardedAdFinished;
}

    public int GetCurrentCoins()
    {
        return YandexGame.savesData.piggyBankCoins;
    }

    public int GetCoinsToCollect()
    {
        return coinsToCollect;
    }

    public void ResetPiggyBank()
    {
        YandexGame.savesData.piggyBankCoins = 0;
        YandexGame.SaveProgress();
        OnCoinsChanged?.Invoke();
        Debug.Log("Piggy bank reset");
    }
}
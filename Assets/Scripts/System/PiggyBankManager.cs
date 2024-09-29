using UnityEngine;
using System;

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
        SDKWrapper.GetDataEvent += OnDataLoaded;
    }

    private void OnDestroy()
    {
        SDKWrapper.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        // Данные уже загружены в SDKWrapper.savesData
        OnCoinsChanged?.Invoke();
    }

    public void AddCoinsForLevelCompletion()
    {
        SDKWrapper.savesData.piggyBankCoins += coinsPerLevel;
        SDKWrapper.SaveProgress();
        OnCoinsChanged?.Invoke();
        Debug.Log($"Coins added to piggy bank. Current coins: {SDKWrapper.savesData.piggyBankCoins}");
    }

    public bool CanCollectCoins()
    {
        return SDKWrapper.savesData.piggyBankCoins >= coinsToCollect;
    }

   public void CollectCoins()
    {
    if (CanCollectCoins())
    {
        // Вызываем рекламу перед сбором монет
        SDKWrapper.RewVideoShow(0);
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
        int coinsToAdd = SDKWrapper.savesData.piggyBankCoins;
        SDKWrapper.savesData.coins += coinsToAdd;
        SDKWrapper.savesData.piggyBankCoins = 0;
        SDKWrapper.SaveProgress();
        OnCoinsChanged?.Invoke();
        Debug.Log($"Collected {coinsToAdd} coins from piggy bank after watching ad.");
    }
    }

private void OnEnable()
{
    SDKWrapper.RewardVideoEvent += OnRewardedAdFinished;
}

private void OnDisable()
{
    SDKWrapper.RewardVideoEvent -= OnRewardedAdFinished;
}

    public int GetCurrentCoins()
    {
        return SDKWrapper.savesData.piggyBankCoins;
    }

    public int GetCoinsToCollect()
    {
        return coinsToCollect;
    }

    public void ResetPiggyBank()
    {
        SDKWrapper.savesData.piggyBankCoins = 0;
        SDKWrapper.SaveProgress();
        OnCoinsChanged?.Invoke();
        Debug.Log("Piggy bank reset");
    }
}